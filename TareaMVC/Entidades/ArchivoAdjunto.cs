using Microsoft.EntityFrameworkCore;

namespace TareaMVC.Entidades
{
    public class ArchivoAdjunto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        [Unicode]
        public string Url { get; set; }
        public DateTime FechaDeCreacion { get; set; }
        public int Orden { get; set; }
        public int TareaId { get; set; }
        public Tarea Tarea { get; set; }
    }
}
