using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Amigo:CamposControl
    {
        public int IdAmigo { get; set; }

        public int IdUsuario { get; set; }
        public int IdAmigoUsuario { get; set; }

        public string Estado { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaAceptacion { get; set; }

    }
}
