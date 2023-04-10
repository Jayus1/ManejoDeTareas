using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TareaMVC.Models;
using TareaMVC.Servicios;

namespace TareaMVC.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext context;

        public UsuariosController
            (UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context) 
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new IdentityUser() { UserName= modelo.Email, Email=modelo.Email };
            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }
        }

        [AllowAnonymous]
        public IActionResult Login(string mensaje =null)
        {
            if (mensaje is null)
            {
                ViewData["Mensaje"] = mensaje;
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await signInManager.PasswordSignInAsync
                (model.Email, model.Password, model.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "El correo o contraseña es incorrecto");
                return View(model);
            } 
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home"); 
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult LoginExterno(string proveedor, string urlRetorno = null)
        {
            var urlRedirection = Url.Action("RegistrarUsuariosExternos", values: new { urlRetorno });
            var propiedad = signInManager.ConfigureExternalAuthenticationProperties(proveedor, urlRedirection);
            return new ChallengeResult(proveedor, propiedad);

        }

        [AllowAnonymous]
        public async Task<IActionResult> RegistrarUsuariosExternos(string urlRetorno = null, string remoteError = null)
        {
            urlRetorno = urlRetorno ?? Url.Content("~/");

            var mensaje = "";

            if (remoteError is not null)
            {
                mensaje = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if(info== null)
            {

                mensaje = $"Error cargando la data del login externo";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }
            var resultadoLoginExterno = await signInManager.ExternalLoginSignInAsync
                (info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            //Ya la cuenta existe
            if (resultadoLoginExterno.Succeeded)
            {
                return LocalRedirect(urlRetorno);
            }

            string email="";

            if (info.Principal.HasClaim(c=>c.Type==ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                mensaje = $"Error leyendo el email del usuario desde el proveedor";
                return RedirectToAction("Login", routeValues: new { mensaje });

            }

            var usuario = new IdentityUser() { Email = email, UserName = email };
            var resultadoCrearUsuarios= await userManager.CreateAsync(usuario);

            if (!resultadoCrearUsuarios.Succeeded)
            {
                mensaje = resultadoCrearUsuarios.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoAgregarLogon = await userManager.AddLoginAsync(usuario,info);
            if (resultadoCrearUsuarios.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true, info.LoginProvider);
                return LocalRedirect(urlRetorno);
            }

            mensaje = "Ha ocurrido un error agregando el login";
            return RedirectToAction("Login", routeValues: new { mensaje });

        }
        [HttpGet]
        [Authorize(Roles =Constante.RolAdmin)]
        public async Task<IActionResult> Listado(string mensaje = null)
        {
            var usuarios = await context.Users.Select(u => new UsuarioViewModel
            {
                Email = u.Email
            }).ToListAsync();

            var modelo = new UsuariosListadosViewModel();
            modelo.Usuarios = usuarios;
            modelo.Mensaje = mensaje;

            return View(modelo);
        }

        [HttpPost]
        [Authorize(Roles = Constante.RolAdmin)]
        public async Task<IActionResult> HacerAdmin(string email)
        {
            var usuario = await context.Users
                .Where(u=>u.Email== email)
                .FirstOrDefaultAsync();

            if(usuario is null)
            {
                return NotFound();
            }

            await userManager.AddToRoleAsync(usuario, Constante.RolAdmin);
            return RedirectToAction("Listado",
                routeValues: new { mensaje = "Rol asignado correctamente a "+ email });
        }

        [HttpPost]
        [Authorize(Roles = Constante.RolAdmin)]
        public async Task<IActionResult> RemoverAdmin(string email)
        {
            var usuario = await context.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (usuario is null)
            {
                return NotFound();
            }

            await userManager.RemoveFromRoleAsync(usuario, Constante.RolAdmin);
            return RedirectToAction("Listado",
                routeValues: new { mensaje = "Rol removido correctamente a " + email });

        }

    }
}
