using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TareaMVC.Migrations;
using TareaMVC.Models;

namespace TareaMVC.Servicios
{
    public class AlmacenadorArchivosAzure : IAlmacenadorArchivos
    {
        private string connectionString;
        public AlmacenadorArchivosAzure(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorege");
        }

        public async Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, IEnumerable<IFormFile> archivo)
        {
            var cliente = new BlobContainerClient(connectionString, contenedor);
            await cliente.CreateIfNotExistsAsync();

            cliente.SetAccessPolicy(PublicAccessType.Blob);
            var tarea = archivo.Select(async archivo =>
            {
                var nombreArchivoOriginal = Path.GetFileName(archivo.FileName);
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var blob = cliente.GetBlobClient(nombreArchivo);
                var blobHttpHeaders = new BlobHttpHeaders();
                blobHttpHeaders.ContentType = archivo.ContentType;
                await blob.UploadAsync(archivo.OpenReadStream(), blobHttpHeaders);
                return new AlmacenarArchivoResultado()
                {
                    URL = blob.Uri.ToString(),
                    Titulo = nombreArchivoOriginal
                };
            });

            var resultado = await Task.WhenAll(tarea);
            return resultado;

        }

        public async Task Borrar(string ruta, string contenido)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return;
            }

            var cliente = new BlobContainerClient(connectionString, contenido);
            await cliente.CreateIfNotExistsAsync();
            var nombreArchivo = Path.GetFileName(ruta);
            var blob = cliente.GetBlobClient(nombreArchivo);
            await blob.DeleteIfExistsAsync();

        }
    }
}

