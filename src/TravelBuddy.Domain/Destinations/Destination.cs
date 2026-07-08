using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace TravelBuddy.Destinations
{
    public class Destination : AuditedAggregateRoot<Guid>
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public Guid CategoriaId { get; set; }
        /*public Categoria Categoria { get; set; }*/
        /*public List<Reserva> Reservas { get; set; }*/
        /*public List<Comentario> Comentarios { get; set; }*/

        /*public List<Calificacion> Calificaciones { get; set; }*/

        /*public Destination()
        {
            Reservas = new List<Reserva>();
            Comentarios = new List<Comentario>();
            Calificaciones = new List<Calificacion>();
        }
        */

        public Destination(Guid id, string nombre, string descripcion, string ubicacion, decimal precio, string imagenUrl, bool disponible, Guid categoriaId)
            : base(id)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Ubicacion = ubicacion;
            Precio = precio;
            ImagenUrl = imagenUrl;
            Disponible = disponible;
            CategoriaId = categoriaId;
            FechaCreacion = DateTime.UtcNow;
            FechaActualizacion = DateTime.UtcNow;
            /*Reservas = new List<Reserva>();*/
            /*Comentarios = new List<Comentario>();*/
            /*Calificaciones = new List<Calificacion>();*/
        }
       

    }

}
