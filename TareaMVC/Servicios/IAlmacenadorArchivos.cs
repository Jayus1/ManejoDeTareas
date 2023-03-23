using TareaMVC.Models;

namespace TareaMVC.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string ruta, string contenido);
        Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, 
            IEnumerable<IFormFile> archivo);


    }
}
