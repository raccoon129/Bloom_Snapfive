using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class RuidoAlertaValidator:AbstractValidator<Ruido_Alerta>
    {
        public RuidoAlertaValidator()
        {
            RuleFor(x => x.IdUsuarioOrigen)
                .NotEmpty().WithMessage("El ID del usuario origen es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario origen debe ser mayor que 0");

            RuleFor(x => x.IdUsuarioDestino)
                .NotEmpty().WithMessage("El ID del usuario destino es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario destino debe ser mayor que 0")
                .NotEqual(x => x.IdUsuarioOrigen).WithMessage("El usuario origen y destino no pueden ser el mismo");

            //Para que sea algo breve
            RuleFor(x => x.ComentarioAlerta)
                .MaximumLength(500).WithMessage("El comentario de la alerta no puede exceder los 500 caracteres");
        }
    }
}
