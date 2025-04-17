using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Foto : CamposControl
    {
        public int IdFoto { get; set; }

        public int IdUsuario { get; set; }

        public string UrlFoto { get; set; }

        public string Descripcion { get; set; }

        public string Ubicacion { get; set; }
        public DateTime FechaSubida { get; set; }

    }
}
