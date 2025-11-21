using CsvHelper.Configuration;
using ETLWorkerService.Domain.Entities;

namespace ETLWorkerService.Infrastructure.Repositories
{
    /// <summary>
    /// Configuración de mapeo para WebReview con conversión de IDs con prefijo
    /// </summary>
    public sealed class WebReviewMap : ClassMap<WebReview>
    {
        public WebReviewMap()
        {
            Map(m => m.IdReview).Name("IdReview");
            Map(m => m.IdCliente).Name("IdCliente").TypeConverter<PrefixedIntConverter>();
            Map(m => m.IdProducto).Name("IdProducto").TypeConverter<PrefixedIntConverter>();
            Map(m => m.Fecha).Name("Fecha");
            Map(m => m.Comentario).Name("Comentario");
            Map(m => m.Rating).Name("Rating");
            Map(m => m.Fuente).Constant("Web");
            Map(m => m.SentimentScore).Name("sentiment_polarity").Optional();
        }
    }

    /// <summary>
    /// Configuración de mapeo para ComentarioSocial con conversión de IDs con prefijo
    /// </summary>
    public sealed class ComentarioSocialMap : ClassMap<ComentarioSocial>
    {
        public ComentarioSocialMap()
        {
            Map(m => m.IdComentario).Name("IdComment");
            Map(m => m.IdCliente).Name("IdCliente").TypeConverter<PrefixedIntConverter>();
            Map(m => m.IdProducto).Name("IdProducto").TypeConverter<PrefixedIntConverter>();
            Map(m => m.Fuente);
            Map(m => m.Fecha);
            Map(m => m.SentimentScore).Name("sentiment_polarity").Optional();
        }
    }

    /// <summary>
    /// Configuración de mapeo para Encuesta
    /// </summary>
    public sealed class EncuestaMap : ClassMap<Encuesta>
    {
        public EncuestaMap()
        {
            Map(m => m.IdEncuesta).Name("IdOpinion");
            Map(m => m.IdCliente).Name("IdCliente");
            Map(m => m.IdProducto).Name("IdProducto");
            Map(m => m.Fuente).Name("Fuente");
            Map(m => m.Fecha).Name("Fecha");
            Map(m => m.PuntajeSatisfaccion).Name("PuntajeSatisfaccion");
            Map(m => m.Clasificacion).Name("Clasificacion");
            Map(m => m.IdClasificacion).Ignore();
            Map(m => m.IdCarga).Ignore();
            Map(m => m.SentimentScore).Name("sentiment_polarity").Optional();
        }
    }
}
