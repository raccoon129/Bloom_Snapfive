using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Ajuste_Usuario:CamposControl
    {
        public int IdAjuste { get; set; }

        public int IdUsuario { get; set; }

        public bool? ModoAhorroDatos { get; set; }

        public string? OtrosAjustes { get; set; }
    }
}
