using ETLWorkerService.Core.Entities;
using ETLWorkerService.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class ApiDataRepository : IDataRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiDataRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:SocialMediaApiBaseUrl"] ?? throw new ArgumentNullException("ApiSettings:SocialMediaApiBaseUrl is not configured.");
        }

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            return await Task.FromResult(new List<Client>());
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await Task.FromResult(new List<Product>());
        }

        public async Task<IEnumerable<SocialComment>> GetSocialCommentsAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/comentarios");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiComments = JsonSerializer.Deserialize<List<ComentarioApiDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var socialComments = new List<SocialComment>();
            if (apiComments != null)
            {
                foreach (var apiComment in apiComments)
            {
                socialComments.Add(new SocialComment
                {
                    IdComment = apiComment.IdComment,
                    Fecha = apiComment.Fecha,
                    Comentario = apiComment.Comentario,
                    IdCliente = apiComment.Cliente != null ? apiComment.Cliente.IdCliente : -1,
                    IdProducto = apiComment.Producto != null ? apiComment.Producto.IdProducto : -1,
                    Fuente = (apiComment.Fuente != null && apiComment.Fuente.Nombre != null) ? apiComment.Fuente.Nombre : "Unknown"
                });
            }
            }
            return socialComments;
        }

        public async Task<IEnumerable<Survey>> GetSurveysAsync()
        {
            return await Task.FromResult(new List<Survey>());
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            return await Task.FromResult(new List<WebReview>());
        }
    }
}
