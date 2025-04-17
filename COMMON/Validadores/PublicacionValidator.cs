using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class PublicacionValidator:AbstractValidator<publicacion>
    {
        public PublicacionValidator()
        {
            RuleFor(x => x.id_usuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");
            RuleFor(x => x.descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres");
            RuleFor(x => x.fecha_publicacion)
                .NotEmpty().WithMessage("La fecha de publicación es obligatoria");
        }
    }
}
