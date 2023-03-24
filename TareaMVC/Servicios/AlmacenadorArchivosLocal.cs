using TareaMVC.Models;

namespace TareaMVC.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWebHostEnvironment env;

        public AlmacenadorArchivosLocal(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.env = env;
        }
        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivo)
        {

            var tareas = archivo.Select(async archivo =>
            {
                var nombreArchivoOriginal = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                string folder = Path.Combine(env.WebRootPath, contenedor);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string ruta = Path.Combine(folder, nombreArchivo);
                using (var ms = new MemoryStream())
                {
                    await archivo.CopyToAsync(ms);
                    var contenido = ms.ToArray();
                    await File.WriteAllBytesAsync(ruta, contenido);
                }

                var url = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                var urlArchivo = Path.Combine(url, contenedor, nombreArchivo).Replace("\\", "/");

                return new AlmacenarArchivoResultado
                {
                    URL= urlArchivo,
                    Titulo= nombreArchivoOriginal
                };
            });

            var resultado = await Task.WhenAll(tareas);
            return resultado;

        }

        public Task Borrar(string ruta, string contenido)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.CompletedTask;
            }
            var nombreArchivo = Path.GetFileName(ruta);
            var directorioArchivo = Path.Combine(env.WebRootPath, contenido,nombreArchivo);
            if (File.Exists(directorioArchivo))
            {
                File.Delete(directorioArchivo);
            }

            return Task.CompletedTask;
        }
    }
}
