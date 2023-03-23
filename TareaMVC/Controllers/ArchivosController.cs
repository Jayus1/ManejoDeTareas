using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareaMVC.Entidades;
using TareaMVC.Servicios;

namespace TareaMVC.Controllers
{
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor= "archivosAdjunstos";

        public ArchivosController(ApplicationDbContext context, 
            IServicioUsuario servicioUsuario, IAlmacenadorArchivos almacenadorArchivos)
        {
            this.context = context;
            this.servicioUsuario = servicioUsuario;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId,
            [FromBody] IEnumerable<IFormFile> archivo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var tarea=  await context.Tareas.FirstOrDefaultAsync(x=> x.Id == tareaId);

            if (tarea is null)
            {
                return NotFound();
            }

            if(tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existeArchivosAdjuntos= 
                await context.ArchivosAdjuntos.AnyAsync(x=> x.TareaId == tareaId);

            var ordenMayor = 0;
            if(existeArchivosAdjuntos)
            {
                ordenMayor = await context.ArchivosAdjuntos
                    .Where(a => a.TareaId == tareaId).Select(a => a.Orden).MaxAsync();
            }

            var resultados = await almacenadorArchivos.Almacenar(contenedor, archivo);

            var archivosAdjuntos = resultados.Select((resultados, indice) => new ArchivoAdjunto
            {
                TareaId= tareaId,
                FechaDeCreacion = DateTime.UtcNow,
                Url=resultados.URL,
                Titulo= resultados.Titulo,
                Orden= ordenMayor + indice + 1
            }).ToList();

            context.AddRange(archivosAdjuntos);
            await context.SaveChangesAsync();

            return archivosAdjuntos.ToList();

        }
    }
}
