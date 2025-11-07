
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
            await LoadDimFuente(socialComments, surveys, webReviews);
            await LoadDimClasificacion();
            await LoadDimFecha(socialComments, surveys, webReviews);
            await LoadFactOpiniones(socialComments, surveys, webReviews);

            _logger.LogInformation("ETL process finished at: {time}", DateTimeOffset.Now);
        }

        private async Task LoadDimFecha(IEnumerable<SocialComment> socialComments, IEnumerable<Survey> surveys, IEnumerable<WebReview> webReviews)
        {
            var allDates = new List<DateTime>();
            allDates.AddRange(socialComments.Select(sc => sc.Fecha));
            allDates.AddRange(surveys.Select(s => s.Fecha));
            allDates.AddRange(webReviews.Select(wr => wr.Fecha));

            var uniqueDates = allDates.Distinct().ToList();
            var existingFechas = _dwContext.DimFechas.ToDictionary(df => df.FechaKey, df => df);

            foreach (var date in uniqueDates)
            {
                var fechaKey = int.Parse(date.ToString("yyyyMMdd"));
                if (!existingFechas.ContainsKey(fechaKey))
                {
                    _dwContext.DimFechas.Add(new DimFecha
                    {
                        FechaKey = fechaKey,
                        Fecha = date,
                        Dia = date.Day,
                        Mes = date.Month,
                        Anio = date.Year,
                        Trimestre = (date.Month + 2) / 3,
                        DiaDeLaSemana = (int)date.DayOfWeek
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
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

        private async Task LoadDimFuente(IEnumerable<SocialComment> socialComments, IEnumerable<Survey> surveys, IEnumerable<WebReview> webReviews)
        {
            var existingFuentes = _dwContext.DimFuentes.Where(f => f.NombreFuente != null).ToDictionary(f => f.NombreFuente!, f => f);

            // Collect sources from relational context
            var sourcesFromRContext = _rContext.Sources.Select(s => s.Nombre).Where(n => n != null).ToList();

            // Collect sources from extracted data
            var sourcesFromSocialComments = socialComments.Select(sc => sc.Fuente).Where(f => f != null).ToList();
            var sourcesFromSurveys = surveys.Select(s => "Encuesta").ToList();
            var sourcesFromWebReviews = webReviews.Select(wr => "WebReview").ToList(); // Hardcoded "WebReview" source

            var allUniqueSources = sourcesFromRContext
                                    .Union(sourcesFromSocialComments)
                                    .Union(sourcesFromSurveys)
                                    .Union(sourcesFromWebReviews)
                                    .Append("Unknown") // Add "Unknown" as a default source
                                    .Distinct()
                                    .ToList();

            foreach (var sourceName in allUniqueSources)
            {
                if (sourceName != null && !existingFuentes.ContainsKey(sourceName))
                {
                    _dwContext.DimFuentes.Add(new DimFuente
                    {
                        NombreFuente = sourceName
                    });
                }
            }
            await _dwContext.SaveChangesAsync();
        }

        private async Task LoadDimClasificacion()
        {
            var existingClasificaciones = _dwContext.DimClasificaciones.Where(c => c.NombreClasificacion != null).ToDictionary(c => c.NombreClasificacion!, c => c);
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
            var dimFuentes = _dwContext.DimFuentes.Where(f => f.NombreFuente != null).ToDictionary(f => f.NombreFuente!, f => f.FuenteKey);
            var dimClasificaciones = _dwContext.DimClasificaciones.Where(c => c.NombreClasificacion != null).ToDictionary(c => c.NombreClasificacion!, c => c.ClasificacionKey);

            foreach (var socialComment in socialComments)
            {
                _dwContext.FactOpiniones.Add(new FactOpiniones
                {
                    FechaKey = int.Parse(socialComment.Fecha.ToString("yyyyMMdd")),
                    ClienteKey = dimClientes.ContainsKey(socialComment.IdCliente) ? dimClientes[socialComment.IdCliente] : -1,
                    ProductoKey = dimProductos.ContainsKey(socialComment.IdProducto) ? dimProductos[socialComment.IdProducto] : -1,
                    FuenteKey = socialComment.Fuente != null && dimFuentes.ContainsKey(socialComment.Fuente) ? dimFuentes[socialComment.Fuente] : dimFuentes["Unknown"],
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
                    FuenteKey = dimFuentes.ContainsKey("Encuesta") ? dimFuentes["Encuesta"] : dimFuentes["Unknown"],
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
                    ClienteKey = dimClientes.ContainsKey(webReview.IdCliente) ? dimClientes[webReview.IdCliente] : -1,
                    ProductoKey = dimProductos.ContainsKey(webReview.IdProducto) ? dimProductos[webReview.IdProducto] : -1,
                    FuenteKey = dimFuentes.ContainsKey("WebReview") ? dimFuentes["WebReview"] : dimFuentes["Unknown"],
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
