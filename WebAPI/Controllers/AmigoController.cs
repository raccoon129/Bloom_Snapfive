using COMMON.Entidades;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmigoController:GenericController<amigo>
    {
        public AmigoController() : base(Parametros.FabricaRepository.AmigoRepository())
        {

        }
    }
}
