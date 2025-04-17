using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class ajuste_usuario:CamposControl
    {
        public int id_ajuste { get; set; }

        public int id_usuario { get; set; }

        public bool? modo_ahorro_datos { get; set; }

        public string? otros_ajustes { get; set; }
        public DateTime? fecha_actualizacion_ajuste { get; set; }
    }
}
