using AutoMapper;
using TareaMVC.Entidades;
using TareaMVC.Models;

namespace TareaMVC.Servicios
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Tarea,TareaDTO>();
        }
    }
}
