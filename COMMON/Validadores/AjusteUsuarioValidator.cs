using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class AjusteUsuarioValidator:AbstractValidator<Ajuste_Usuario>
    {
        public AjusteUsuarioValidator()
        {
            // Constructor
            RuleFor(x => x.IdUsuario)
    .NotEmpty().WithMessage("El ID del usuario es obligatorio")
    .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");

            RuleFor(x => x.OtrosAjustes)
    .MaximumLength(4000).WithMessage("La longitud máxima para otros ajustes es de 4000 caracteres");
        
    }
    }
}
