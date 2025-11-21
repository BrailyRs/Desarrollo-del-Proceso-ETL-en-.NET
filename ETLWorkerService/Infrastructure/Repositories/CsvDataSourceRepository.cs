using CsvHelper;
using CsvHelper.Configuration;
using ETLWorkerService.Domain.Entities;
using ETLWorkerService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class CsvDataSourceRepository : IDataSourceRepository
    {
        private readonly ILogger<CsvDataSourceRepository> _logger;
        private readonly string _basePath;

        public CsvDataSourceRepository(ILogger<CsvDataSourceRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _basePath = configuration.GetValue<string>("ETLConfiguration:CsvBasePath") 
                ?? "C:/Users/PC/Desktop/Tareas ITLA/Electiva 1 - Big Data/Unidad 5/csv";
        }

        public async Task<IEnumerable<Cliente>> GetClientesAsync()
        {
            return await ReadCsvAsync<Cliente>("clients.csv");
        }

        public async Task<IEnumerable<Producto>> GetProductosAsync()
        {
            var productos = await ReadCsvAsync<Producto>("products.csv");
            return productos.Select(p =>
            {
                p.Categoria = p.Categoria ?? "Sin Categoría";
                return p;
            }).ToList();
        }

        public async Task<IEnumerable<ComentarioSocial>> GetComentariosSocialesAsync()
        {
            // Leer comentarios base
            var comentarios = await ReadCsvAsync<ComentarioSocial>("social_comments.csv");
            
            // Intentar leer datos de sentiment si existen
            var comentariosSentiment = await ReadCsvAsync<ComentarioSocial>("social_comments_sentiment.csv");
            
            if (comentariosSentiment.Any())
            {
                var sentimentDict = comentariosSentiment.ToDictionary(c => c.IdComentario ?? "", c => c.SentimentScore);
                
                return comentarios.Select(c =>
                {
                    if (!string.IsNullOrEmpty(c.IdComentario) && sentimentDict.ContainsKey(c.IdComentario))
                    {
                        c.SentimentScore = sentimentDict[c.IdComentario];
                    }
                    return c;
                }).ToList();
            }
            
            return comentarios;
        }

        public async Task<IEnumerable<Encuesta>> GetEncuestasAsync()
        {
            // Leer encuestas base (parte 1 tiene la mayoría de campos)
            var encuestas = await ReadCsvAsync<Encuesta>("surveys_part1.csv");
            
            // Intentar leer datos de sentiment si existen
            var encuestasSentiment = await ReadCsvAsync<Encuesta>("surveys_sentiment.csv");
            
            if (encuestasSentiment.Any())
            {
                var sentimentDict = encuestasSentiment.ToDictionary(e => e.IdEncuesta, e => e.SentimentScore);
                
                return encuestas.Select(e =>
                {
                    if (sentimentDict.ContainsKey(e.IdEncuesta))
                    {
                        e.SentimentScore = sentimentDict[e.IdEncuesta];
                    }
                    return e;
                }).ToList();
            }
            
            return encuestas;
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            // Leer web reviews base
            var reviews = await ReadCsvAsync<WebReview>("web_reviews.csv");
            
            // Intentar leer datos de sentiment si existen
            var reviewsSentiment = await ReadCsvAsync<WebReview>("web_reviews_sentiment.csv");
            
            if (reviewsSentiment.Any())
            {
                var sentimentDict = reviewsSentiment.ToDictionary(r => r.IdReview ?? "", r => r.SentimentScore);
                
                return reviews.Select(r =>
                {
                    if (!string.IsNullOrEmpty(r.IdReview) && sentimentDict.ContainsKey(r.IdReview))
                    {
                        r.SentimentScore = sentimentDict[r.IdReview];
                    }
                    return r;
                }).ToList();
            }
            
            return reviews;
        }

        private async Task<List<T>> ReadCsvAsync<T>(string fileName)
        {
            var filePath = Path.Combine(_basePath, fileName);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Archivo CSV no encontrado: {filePath}");
                return new List<T>();
            }

            try
            {
                using var reader = new StreamReader(filePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                };
                
                using var csv = new CsvReader(reader, config);
                
                // Registrar ClassMaps para tipos específicos
                if (typeof(T) == typeof(WebReview))
                    csv.Context.RegisterClassMap<WebReviewMap>();
                else if (typeof(T) == typeof(ComentarioSocial))
                    csv.Context.RegisterClassMap<ComentarioSocialMap>();
                else if (typeof(T) == typeof(Encuesta))
                    csv.Context.RegisterClassMap<EncuestaMap>();

                var records = csv.GetRecords<T>().ToList();
                _logger.LogInformation($"Leídos {records.Count} registros de {fileName}");
                return await Task.FromResult(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leyendo archivo CSV: {fileName}");
                throw;
            }
        }
    }
}
