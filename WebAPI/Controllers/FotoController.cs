using COMMON.Entidades;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;


namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FotoController : GenericController<foto>
    {
        public FotoController() : base(Parametros.FabricaRepository.FotoRepository())
        {
        }
    }
}
