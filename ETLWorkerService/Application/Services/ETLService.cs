
using ETLWorkerService.Core.Entities;
using ETLWorkerService.Core.Interfaces;
using ETLWorkerService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETLWorkerService.Application.Services
{
    public class ETLService : IETLService
    {
        private readonly ILogger<ETLService> _logger;
        private readonly IDataRepository _dataRepository;
        private readonly OpinionDwContext _dwContext;
        private readonly OpinionRContext _rContext;

        public ETLService(ILogger<ETLService> logger, IDataRepository dataRepository, OpinionDwContext dwContext, OpinionRContext rContext)
        {
            _logger = logger;
            _dataRepository = dataRepository;
            _dwContext = dwContext;
            _rContext = rContext;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ETL process started at: {time}", DateTimeOffset.Now);

            var clients = await _dataRepository.GetClientsAsync();
            var products = await _dataRepository.GetProductsAsync();
            var socialComments = await _dataRepository.GetSocialCommentsAsync();
            var surveys = await _dataRepository.GetSurveysAsync();
            var webReviews = await _dataRepository.GetWebReviewsAsync();

            _logger.LogInformation("Data extraction completed.");

            await LoadDimCliente(clients);
            await LoadDimProducto(products);
            await LoadDimFuente();
            await LoadDimClasificacion();
            await LoadFactOpiniones(socialComments, surveys, webReviews);

            _logger.LogInformation("ETL process finished at: {time}", DateTimeOffset.Now);
        }

        private async Task LoadDimCliente(IEnumerable<Client> clients)
        {
            var existingClients = _dwContext.DimClientes.ToDictionary(c => c.IdCliente, c => c);

            foreach (var client in clients)
            {
                if (!existingClients.ContainsKey(client.IdCliente))
                {
                    _dwContext.DimClientes.Add(new DimCliente
                    {
                        IdCliente = client.IdCliente,
                        Nombre = client.Nombre,
                        Email = client.Email
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
        }

        private async Task LoadDimProducto(IEnumerable<Product> products)
        {
            var existingProducts = _dwContext.DimProductos.ToDictionary(p => p.IdProducto, p => p);

            foreach (var product in products)
            {
                if (!existingProducts.ContainsKey(product.IdProducto))
                {
                    _dwContext.DimProductos.Add(new DimProducto
                    {
                        IdProducto = product.IdProducto,
                        NombreProducto = product.Nombre,
                        NombreCategoria = product.Categoria
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
        }

        private async Task LoadDimFuente()
        {
            var existingFuentes = _dwContext.DimFuentes.Where(f => f.NombreFuente != null).ToDictionary(f => f.NombreFuente, f => f);
            var fuentes = _rContext.Sources.ToList();

            foreach (var fuente in fuentes)
            {
                if (fuente.Nombre != null && !existingFuentes.ContainsKey(fuente.Nombre))
                {
                    _dwContext.DimFuentes.Add(new DimFuente
                    {
                        NombreFuente = fuente.Nombre
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
        }

        private async Task LoadDimClasificacion()
        {
            var existingClasificaciones = _dwContext.DimClasificaciones.Where(c => c.NombreClasificacion != null).ToDictionary(c => c.NombreClasificacion, c => c);
            var clasificaciones = _rContext.Classifications.ToList();

            foreach (var clasificacion in clasificaciones)
            {
                if (clasificacion.Nombre != null && !existingClasificaciones.ContainsKey(clasificacion.Nombre))
                {
                    _dwContext.DimClasificaciones.Add(new DimClasificacion
                    {
                        NombreClasificacion = clasificacion.Nombre
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
        }

        private async Task LoadFactOpiniones(IEnumerable<SocialComment> socialComments, IEnumerable<Survey> surveys, IEnumerable<WebReview> webReviews)
        {
            var dimClientes = _dwContext.DimClientes.ToDictionary(c => c.IdCliente, c => c.ClienteKey);
            var dimProductos = _dwContext.DimProductos.ToDictionary(p => p.IdProducto, p => p.ProductoKey);
            var dimFuentes = _dwContext.DimFuentes.Where(f => f.NombreFuente != null).ToDictionary(f => f.NombreFuente, f => f.FuenteKey);
            var dimClasificaciones = _dwContext.DimClasificaciones.Where(c => c.NombreClasificacion != null).ToDictionary(c => c.NombreClasificacion, c => c.ClasificacionKey);

            foreach (var socialComment in socialComments)
            {
                _dwContext.FactOpiniones.Add(new FactOpiniones
                {
                    FechaKey = int.Parse(socialComment.Fecha.ToString("yyyyMMdd")),
                    ClienteKey = dimClientes.ContainsKey(int.Parse(socialComment.IdCliente.Substring(1))) ? dimClientes[int.Parse(socialComment.IdCliente.Substring(1))] : -1,
                    ProductoKey = dimProductos.ContainsKey(int.Parse(socialComment.IdProducto.Substring(1))) ? dimProductos[int.Parse(socialComment.IdProducto.Substring(1))] : -1,
                    FuenteKey = socialComment.Fuente != null && dimFuentes.ContainsKey(socialComment.Fuente) ? dimFuentes[socialComment.Fuente] : -1,
                    ClasificacionKey = null,
                    Rating = null,
                    PuntajeSatisfaccion = null,
                    SentimentScore = null // TODO: Implement sentiment analysis
                });
            }
            await _dwContext.SaveChangesAsync(); // Save changes after social comments

            foreach (var survey in surveys)
            {
                _dwContext.FactOpiniones.Add(new FactOpiniones
                {
                    FechaKey = int.Parse(survey.Fecha.ToString("yyyyMMdd")),
                    ClienteKey = dimClientes.ContainsKey(survey.IdCliente) ? dimClientes[survey.IdCliente] : -1,
                    ProductoKey = dimProductos.ContainsKey(survey.IdProducto) ? dimProductos[survey.IdProducto] : -1,
                    FuenteKey = survey.Fuente != null && dimFuentes.ContainsKey(survey.Fuente) ? dimFuentes[survey.Fuente] : -1,
                    ClasificacionKey = survey.Clasificacion != null && dimClasificaciones.ContainsKey(survey.Clasificacion) ? dimClasificaciones[survey.Clasificacion] : -1,
                    Rating = null,
                    PuntajeSatisfaccion = survey.PuntajeSatisfaccion,
                    SentimentScore = null // TODO: Implement sentiment analysis
                });
            }
            await _dwContext.SaveChangesAsync(); // Save changes after surveys

            foreach (var webReview in webReviews)
            {
                _dwContext.FactOpiniones.Add(new FactOpiniones
                {
                    FechaKey = int.Parse(webReview.Fecha.ToString("yyyyMMdd")),
                    ClienteKey = dimClientes.ContainsKey(int.Parse(webReview.IdCliente.Substring(1))) ? dimClientes[int.Parse(webReview.IdCliente.Substring(1))] : -1,
                    ProductoKey = dimProductos.ContainsKey(int.Parse(webReview.IdProducto.Substring(1))) ? dimProductos[int.Parse(webReview.IdProducto.Substring(1))] : -1,
                    FuenteKey = dimFuentes.ContainsKey("WebReview") ? dimFuentes["WebReview"] : -1,
                    ClasificacionKey = null,
                    Rating = webReview.Rating,
                    PuntajeSatisfaccion = null,
                    SentimentScore = null // TODO: Implement sentiment analysis
                });
            }
            await _dwContext.SaveChangesAsync(); // Save changes after web reviews
        }
    }
}
