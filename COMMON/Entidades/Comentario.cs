using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Comentario: CamposControl
    {
        public int IdComentario { get; set; }

        public int IdFoto { get; set; }
        public int IdUsuario { get; set; }
        public string Contenido { get; set; }

        public DateTime FechaComentario { get; set; }
    }
}
