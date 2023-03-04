using Microsoft.AspNetCore.Mvc.Rendering;

namespace TareaMVC.Servicios
{
    public class Constante
    {
        public const string RolAdmin = "admin";

        public static readonly SelectListItem[] CulturasIASoportadas = new SelectListItem[]
        {
            new SelectListItem{Value="en",Text="English"},
            new SelectListItem{Value="es",Text="Español"}
       };

    }
}
