using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMMON.Entidades
{
    public class Ruido_Alerta : CamposControl
    {
        public int IdAlerta { get; set; }

        public int IdUsuarioOrigen { get; set; }
        public int IdUsuarioDestino { get; set; }

        public DateTime FechaAlerta { get; set; }

        public bool EstadoAlerta { get; set; }

        public string ComentarioAlerta { get; set; }
    }
}
