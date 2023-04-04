using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareaMVC.Entidades;
using TareaMVC.Models;
using TareaMVC.Servicios;

namespace TareaMVC.Controllers
{
    [Route("api/pasos")]
    public class PasosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuario servicioUsuario;

        public PasosController(ApplicationDbContext applicationDbContext, IServicioUsuario servicioUsuario)
        {
            context = applicationDbContext;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpPost("{tareaId:Int}")]
        public async Task<ActionResult<Paso>> Post(int tareaId, [FromBody] PasosCrearDTO pasosCrear)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t=> t.Id == tareaId);

            if(tarea == null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenPasos = await context.Pasos.AnyAsync(p=> p.TareaId == tareaId);

            var ordenMayor = 0;
            if (existenPasos)
            {
                ordenMayor = await context.Pasos
                    .Where(p => p.TareaId == tareaId).Select(o => o.Orden).MaxAsync();
            }

            Paso paso = new Paso();
            paso.TareaId = tareaId;
            paso.Orden = ordenMayor;
            paso.Descripcion=pasosCrear.Descripcion;
            paso.Realizado = pasosCrear.Realizado;

            context.Add(paso);
            await context.SaveChangesAsync();

            return paso;

        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] PasosCrearDTO pasosCrearDTO)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var paso= await context.Pasos.Include(p=> p.Tarea).FirstOrDefaultAsync(p=>p.Id== id);

            if (paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            paso.Descripcion = pasosCrearDTO.Descripcion;
            paso.Realizado = pasosCrearDTO.Realizado;

            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var paso= await context.Pasos.Include(p=>p.Tarea).FirstOrDefaultAsync(p=> p.Id== id);

            if(paso is null)
            {
                return NotFound();
            }

            if (paso.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            context.Remove(paso);
            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("ordenar/{tareaId:Int}")]
        public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync
                (t => t.Id == tareaId && t.UsuarioCreacionId == usuarioId);

            if(tarea is null)
            {
                return NotFound();
            }

            var pasos = await context.Pasos.Where(x => x.TareaId == tareaId).ToListAsync();

            var pasosIds = pasos.Select(x => x.Id);

            var idsPasosNoPertenecenALaTarea = ids.Except(pasosIds).ToList();

            if (idsPasosNoPertenecenALaTarea.Any())
            {
                return BadRequest("No todos los pasos estan presentes");
            }

            var pasosDiccionario = pasos.ToDictionary(p => p.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var pasoId = ids[i];
                var paso = pasosDiccionario[pasoId];
                paso.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();

        }

    }
}
