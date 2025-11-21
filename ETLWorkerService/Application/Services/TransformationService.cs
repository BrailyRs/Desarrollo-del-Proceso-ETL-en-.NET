using ETLWorkerService.Application.Interfaces;
using ETLWorkerService.Domain.Entities;

namespace ETLWorkerService.Application.Services
{
    public class TransformationService : ITransformationService
    {
        public IEnumerable<DimFecha> TransformToDimFecha(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews)
        {
            var fechas = new HashSet<DateTime>();
            
            foreach (var c in comentarios) fechas.Add(c.Fecha.Date);
            foreach (var e in encuestas) fechas.Add(e.Fecha.Date);
            foreach (var r in reviews) fechas.Add(r.Fecha.Date);

            return fechas.Select(f => new DimFecha
            {
                FechaKey = int.Parse(f.ToString("yyyyMMdd")),
                Fecha = f,
                Dia = f.Day,
                Mes = f.Month,
                Anio = f.Year,
                Trimestre = (f.Month - 1) / 3 + 1,
                DiaDeLaSemana = (int)f.DayOfWeek
            }).ToList();
        }

        public IEnumerable<DimCliente> TransformToDimCliente(IEnumerable<Cliente> clientes)
        {
            return clientes.Select(c => new DimCliente
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Email = c.Email
            }).ToList();
        }

        public IEnumerable<DimProducto> TransformToDimProducto(IEnumerable<Producto> productos)
        {
            return productos.Select(p => new DimProducto
            {
                IdProducto = p.IdProducto,
                NombreProducto = p.Nombre,
                NombreCategoria = p.Categoria
            }).ToList();
        }

        public IEnumerable<DimFuente> TransformToDimFuente(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews)
        {
            var fuentes = new HashSet<string>();
            
            foreach (var c in comentarios.Where(x => !string.IsNullOrWhiteSpace(x.Fuente)))
                fuentes.Add(c.Fuente!);
            foreach (var e in encuestas.Where(x => !string.IsNullOrWhiteSpace(x.Fuente)))
                fuentes.Add(e.Fuente!);
            foreach (var r in reviews.Where(x => !string.IsNullOrWhiteSpace(x.Fuente)))
                fuentes.Add(r.Fuente!);

            if (!fuentes.Any()) fuentes.Add("Unknown");

            return fuentes.Select(f => new DimFuente { NombreFuente = f }).ToList();
        }

        public IEnumerable<DimClasificacion> TransformToDimClasificacion(IEnumerable<Encuesta> encuestas)
        {
            var clasificaciones = encuestas
                .Where(e => !string.IsNullOrWhiteSpace(e.Clasificacion))
                .Select(e => e.Clasificacion!)
                .Distinct()
                .Select(c => new DimClasificacion { NombreClasificacion = c })
                .ToList();

            if (!clasificaciones.Any())
                clasificaciones.Add(new DimClasificacion { NombreClasificacion = "Sin Clasificación" });

            return clasificaciones;
        }

        public IEnumerable<FactOpiniones> TransformToFactOpiniones(
            IEnumerable<ComentarioSocial> comentarios,
            IEnumerable<Encuesta> encuestas,
            IEnumerable<WebReview> reviews,
            Dictionary<int, int> clienteKeys,
            Dictionary<int, int> productoKeys,
            Dictionary<string, int> fuenteKeys,
            Dictionary<string, int> clasificacionKeys,
            Dictionary<DateTime, int> fechaKeys)
        {
            var facts = new List<FactOpiniones>();
            int unknownFuenteKey = fuenteKeys.ContainsKey("Unknown") ? fuenteKeys["Unknown"] : fuenteKeys.Values.FirstOrDefault();
            int unknownClienteKey = clienteKeys.Values.FirstOrDefault();
            int unknownProductoKey = productoKeys.Values.FirstOrDefault();

            foreach (var comentario in comentarios)
            {
                var fechaDate = comentario.Fecha.Date;
                if (fechaKeys.ContainsKey(fechaDate))
                {
                    var clienteKey = clienteKeys.ContainsKey(comentario.IdCliente)
                        ? clienteKeys[comentario.IdCliente]
                        : unknownClienteKey;
                    
                    var productoKey = productoKeys.ContainsKey(comentario.IdProducto)
                        ? productoKeys[comentario.IdProducto]
                        : unknownProductoKey;

                    if (clienteKey > 0 && productoKey > 0)
                    {
                        var fuenteKey = !string.IsNullOrWhiteSpace(comentario.Fuente) && fuenteKeys.ContainsKey(comentario.Fuente)
                            ? fuenteKeys[comentario.Fuente]
                            : unknownFuenteKey;

                        facts.Add(new FactOpiniones
                        {
                            FechaKey = fechaKeys[fechaDate],
                            ClienteKey = clienteKey,
                            ProductoKey = productoKey,
                            FuenteKey = fuenteKey,
                            ClasificacionKey = null,
                            Rating = null,
                            PuntajeSatisfaccion = null,
                            SentimentScore = comentario.SentimentScore
                        });
                    }
                }
            }

            foreach (var encuesta in encuestas)
            {
                var fechaDate = encuesta.Fecha.Date;
                if (fechaKeys.ContainsKey(fechaDate))
                {
                    var clienteKey = clienteKeys.ContainsKey(encuesta.IdCliente)
                        ? clienteKeys[encuesta.IdCliente]
                        : unknownClienteKey;
                    
                    var productoKey = productoKeys.ContainsKey(encuesta.IdProducto)
                        ? productoKeys[encuesta.IdProducto]
                        : unknownProductoKey;

                    if (clienteKey > 0 && productoKey > 0)
                    {
                        var fuenteKey = !string.IsNullOrWhiteSpace(encuesta.Fuente) && fuenteKeys.ContainsKey(encuesta.Fuente)
                            ? fuenteKeys[encuesta.Fuente]
                            : unknownFuenteKey;

                        int? clasificacionKey = null;
                        if (!string.IsNullOrWhiteSpace(encuesta.Clasificacion) && clasificacionKeys.ContainsKey(encuesta.Clasificacion))
                            clasificacionKey = clasificacionKeys[encuesta.Clasificacion];

                        facts.Add(new FactOpiniones
                        {
                            FechaKey = fechaKeys[fechaDate],
                            ClienteKey = clienteKey,
                            ProductoKey = productoKey,
                            FuenteKey = fuenteKey,
                            ClasificacionKey = clasificacionKey,
                            Rating = null,
                            PuntajeSatisfaccion = encuesta.PuntajeSatisfaccion,
                            SentimentScore = encuesta.SentimentScore
                        });
                    }
                }
            }

            foreach (var review in reviews)
            {
                var fechaDate = review.Fecha.Date;
                if (fechaKeys.ContainsKey(fechaDate))
                {
                    var clienteKey = clienteKeys.ContainsKey(review.IdCliente)
                        ? clienteKeys[review.IdCliente]
                        : unknownClienteKey;
                    
                    var productoKey = productoKeys.ContainsKey(review.IdProducto)
                        ? productoKeys[review.IdProducto]
                        : unknownProductoKey;

                    if (clienteKey > 0 && productoKey > 0)
                    {
                        var fuenteKey = !string.IsNullOrWhiteSpace(review.Fuente) && fuenteKeys.ContainsKey(review.Fuente)
                            ? fuenteKeys[review.Fuente]
                            : unknownFuenteKey;

                        facts.Add(new FactOpiniones
                        {
                            FechaKey = fechaKeys[fechaDate],
                            ClienteKey = clienteKey,
                            ProductoKey = productoKey,
                            FuenteKey = fuenteKey,
                            ClasificacionKey = null,
                            Rating = review.Rating,
                            PuntajeSatisfaccion = null,
                            SentimentScore = review.SentimentScore
                        });
                    }
                }
            }

            return facts;
        }
    }
}
