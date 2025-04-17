using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Favorito : CamposControl
    {
        public int IdFavorito { get; set; }

        public int IdFoto { get; set; }
        public int IdUsuario { get; set; }

        public DateTime FechaFavorito { get; set; }
    }
}
