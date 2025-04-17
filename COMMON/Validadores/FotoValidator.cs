using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class FotoValidator:AbstractValidator<Foto>
    {
        public FotoValidator()
        {
            RuleFor(x => x.IdUsuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");

            //No porque si no contiene foto se vuelve una publicación normal que hay que ver como se podría integrar.
            //Quizá podría ser que la foto se genere como un estado de WhatsApp.
            //RuleFor(x => x.UrlFoto)
            //    .NotEmpty().WithMessage("La URL de la foto es obligatoria");

            RuleFor(x => x.Descripcion)
                .MaximumLength(1000).WithMessage("La descripción no puede exceder los 1000 caracteres");

            //Con la coordenadas en una sola cadena. Con regex se podría obtener latitud y longitud.

            //RuleFor(x => x.Ubicacion)
            //    .MaximumLength(255).WithMessage("La ubicación no puede exceder los 255 caracteres");
        }
    }
}
