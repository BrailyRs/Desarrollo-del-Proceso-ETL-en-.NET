using ETLWorkerService.Application.DTOs;
using ETLWorkerService.Domain.Entities;
using ETLWorkerService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class ApiDataSourceRepository : IDataSourceRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiDataSourceRepository> _logger;
        private readonly string _baseUrl;

        public ApiDataSourceRepository(HttpClient httpClient, ILogger<ApiDataSourceRepository> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration.GetValue<string>("ETLConfiguration:ApiBaseUrl") 
                ?? "http://localhost:8000";
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<IEnumerable<Cliente>> GetClientesAsync()
        {
            var clientesDto = await GetFromApiAsync<List<ClienteDTO>>("/clientes") ?? new List<ClienteDTO>();
            return clientesDto.Select(c => new Cliente
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Email = c.Email
            }).ToList();
        }

        public async Task<IEnumerable<Producto>> GetProductosAsync()
        {
            var productosDto = await GetFromApiAsync<List<ProductoDTO>>("/productos") ?? new List<ProductoDTO>();
            return productosDto.Select(p => new Producto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Categoria = "Sin Categoría"
            }).ToList();
        }

        public async Task<IEnumerable<ComentarioSocial>> GetComentariosSocialesAsync()
        {
            var comentariosDto = await GetFromApiAsync<List<ComentarioApiResponse>>("/comentarios?limit=1000") 
                ?? new List<ComentarioApiResponse>();
            
            return comentariosDto.Select(c => new ComentarioSocial
            {
                IdComentario = c.IdComment,
                IdCliente = c.Cliente?.IdCliente ?? 0,
                IdProducto = c.Producto?.IdProducto ?? 0,
                Fuente = c.Fuente?.Nombre,
                Fecha = c.Fecha,
                SentimentScore = c.SentimientoScore
            }).ToList();
        }

        public async Task<IEnumerable<Encuesta>> GetEncuestasAsync()
        {
            var encuestasDto = await GetFromApiAsync<List<EncuestaApiResponse>>("/encuestas?limit=1000") 
                ?? new List<EncuestaApiResponse>();
            
            return encuestasDto.Select(e => new Encuesta
            {
                IdEncuesta = e.IdOpinion,
                IdCliente = e.Cliente?.IdCliente ?? 0,
                IdProducto = e.Producto?.IdProducto ?? 0,
                Fuente = "Encuesta",
                Fecha = e.Fecha,
                PuntajeSatisfaccion = e.PuntajeSatisfaccion,
                Clasificacion = e.Clasificacion?.Nombre,
                SentimentScore = e.SentimientoScore
            }).ToList();
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            var reviewsDto = await GetFromApiAsync<List<WebReviewApiResponse>>("/reviews?limit=1000") 
                ?? new List<WebReviewApiResponse>();
            
            return reviewsDto.Select(r => new WebReview
            {
                IdReview = r.IdReview,
                IdCliente = r.Cliente?.IdCliente ?? 0,
                IdProducto = r.Producto?.IdProducto ?? 0,
                Fuente = "Web",
                Fecha = r.Fecha,
                Rating = r.Rating,
                Comentario = r.Comentario,
                SentimentScore = r.SentimientoScore
            }).ToList();
        }

        private async Task<T?> GetFromApiAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo datos de API: {endpoint}");
                return default;
            }
        }
    }
}

