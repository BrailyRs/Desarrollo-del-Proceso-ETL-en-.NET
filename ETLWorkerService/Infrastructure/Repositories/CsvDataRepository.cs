
using ETLWorkerService.Core.Entities;
using ETLWorkerService.Core.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper;
using System.Linq;

namespace ETLWorkerService.Infrastructure.Repositories
{
    public class CsvDataRepository : IDataRepository
    {
        private readonly string _basePath = "C:/Users/PC/Desktop/Tareas ITLA/Electiva 1 - Big Data/Unidad 5/csv";

        public async Task<IEnumerable<Client>> GetClientsAsync()
        {
            using (var reader = new StreamReader(Path.Combine(_basePath, "clients.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<Client>();
                await foreach (var record in csv.GetRecordsAsync<Client>())
                {
                    records.Add(record);
                }
                return records;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            using (var reader = new StreamReader(Path.Combine(_basePath, "products.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ProductMap>();
                var records = new List<Product>();
                await foreach (var record in csv.GetRecordsAsync<Product>())
                {
                    records.Add(record);
                }
                return records;
            }
        }

        public async Task<IEnumerable<SocialComment>> GetSocialCommentsAsync()
        {
            using (var reader = new StreamReader(Path.Combine(_basePath, "social_comments.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<SocialComment>();
                await foreach (var record in csv.GetRecordsAsync<SocialComment>())
                {
                    records.Add(record);
                }
                return records;
            }
        }

        public async Task<IEnumerable<Survey>> GetSurveysAsync()
        {
            using (var reader = new StreamReader(Path.Combine(_basePath, "surveys_part1.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<Survey>();
                await foreach (var record in csv.GetRecordsAsync<Survey>())
                {
                    records.Add(record);
                }
                return records;
            }
        }

        public async Task<IEnumerable<WebReview>> GetWebReviewsAsync()
        {
            using (var reader = new StreamReader(Path.Combine(_basePath, "web_reviews.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<WebReview>();
                await foreach (var record in csv.GetRecordsAsync<WebReview>())
                {
                    records.Add(record);
                }
                return records;
            }
        }

        private sealed class ProductMap : ClassMap<Product>
        {
            public ProductMap()
            {
                Map(m => m.IdProducto).Name("IdProducto");
                Map(m => m.Nombre).Name("Nombre");
                Map(m => m.Categoria).Name("Categoría");
            }
        }
    }
}
