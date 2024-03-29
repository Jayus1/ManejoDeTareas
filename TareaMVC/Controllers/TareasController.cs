﻿ using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareaMVC.Entidades;
using TareaMVC.Models;
using TareaMVC.Servicios;

namespace TareaMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IMapper mapper;

        public TareasController(ApplicationDbContext context, 
            IServicioUsuario servicioUsuario, IMapper mapper)
        {
            this.context = context;
            this.servicioUsuario = servicioUsuario;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<List<TareaDTO>> Get()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tarea = await context.Tareas
                .Where(t=> t.UsuarioCreacionId== usuarioId)
                .OrderBy(t=> t.Orden)
                .ProjectTo<TareaDTO>(mapper.ConfigurationProvider)
                .ToListAsync();
            return tarea;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var existenTareas = await context.Tareas.AnyAsync(x => x.UsuarioCreacionId == usuarioId);

            var ordenMayor = 0;

            if (existenTareas)
            {
                ordenMayor = await context.Tareas.Where(x => x.UsuarioCreacionId == usuarioId)
                    .Select(x => x.Orden).MaxAsync();
            }

            var tarea = new Tarea
            {
                Titulo = titulo,
                UsuarioCreacionId = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                Orden= ordenMayor + 1
            };

            context.Tareas.Add(tarea);
            await context.SaveChangesAsync();

            return tarea; 
        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var tareas= await context.Tareas
                .Where(t=> t.UsuarioCreacionId==usuarioId)
                .ToListAsync();

            var tareasId = tareas.Select(t => t.Id);
            var idsTareasNoPertenecenAlUsuario = ids.Except(tareasId).ToList();

            if (idsTareasNoPertenecenAlUsuario.Any())
            {
                return Forbid();
            }

            var tareasDiccionario = tareas.ToDictionary(x => x.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i + 1;
            }
            
            await context.SaveChangesAsync();

            return Ok();

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var tarea= await context.Tareas
                .Include(t=> t.Pasos.OrderBy(p=>p.Orden)) 
                .Include(t=>t.ArchivoAdjuntos.OrderBy(a=>a.Orden))
                .FirstOrDefaultAsync(t=> t.Id == id && 
                t.UsuarioCreacionId== usuarioId);

            if (tarea is null)
                return NotFound();

            return tarea;
        }

        [HttpPut ("{id:int}")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDTO)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tarea = await context.Tareas
                .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioId);

            if(tarea is null)
            {
                return NotFound();
            }

            tarea.Titulo = tareaEditarDTO.Titulo;
            tarea.Descripcion = tareaEditarDTO.Descripcion;

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var tarea= await context.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioCreacionId == usuarioId);

            if(tarea is null) 
            {
                return NotFound();
            }

            context.Remove(tarea);
            await context.SaveChangesAsync();

            return Ok();
        }
        
    }
}
