using ETLWorkerService.Domain.Entities;

namespace ETLWorkerService.Application.Interfaces
{
    public interface ITransformationService
    {
        IEnumerable<DimFecha> TransformToDimFecha(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews);
        
        IEnumerable<DimCliente> TransformToDimCliente(IEnumerable<Cliente> clientes);
        IEnumerable<DimProducto> TransformToDimProducto(IEnumerable<Producto> productos);
        IEnumerable<DimFuente> TransformToDimFuente(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews);
        IEnumerable<DimClasificacion> TransformToDimClasificacion(IEnumerable<Encuesta> encuestas);
        
        IEnumerable<FactOpiniones> TransformToFactOpiniones(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews,
            Dictionary<int, int> clienteKeys,
            Dictionary<int, int> productoKeys,
            Dictionary<string, int> fuenteKeys,
            Dictionary<string, int> clasificacionKeys,
            Dictionary<DateTime, int> fechaKeys);
    }
}
