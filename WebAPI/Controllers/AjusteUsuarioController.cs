﻿using COMMON.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AjusteUsuarioController:GenericController<ajuste_usuario>
    {
        public AjusteUsuarioController() : base (Parametros.FabricaRepository.AjustesUsuarioRepository())
        {
            
        }
    }
}
