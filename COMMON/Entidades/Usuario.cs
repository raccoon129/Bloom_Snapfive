using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Usuario : CamposControl
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; }
        public string? Biografia { get; set; }
        public string? Email { get; set; }

        public string Telefono { get; set; }

        public string? Pais { get; set; }

        public string? FotoPerfil { get; set; }

        public string Estado { get; set; }

        public DateTime UltimaConexion { get; set; }

        public string PinContacto { get; set; }

        public DateTime FechaCreacion { get; set; }

    }
}
