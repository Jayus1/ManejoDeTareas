using Microsoft.AspNetCore.Mvc;
using TareaMVC.Entidades;
using TareaMVC.Servicios;

namespace TareaMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuario servicioUsuario;

        public TareasController(ApplicationDbContext context, IServicioUsuario servicioUsuario)
        {
            this.context = context;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpPost]
        public async Task<IActionResult<Tarea>> Post()
        {
            return;
        }
    }
}
