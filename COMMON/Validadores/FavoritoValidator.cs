﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class FavoritoValidator:AbstractValidator<favorito>
    {
        public FavoritoValidator()
        {
            RuleFor(x => x.id_foto)
                .NotEmpty().WithMessage("El ID de la foto es obligatorio")
                .GreaterThan(0).WithMessage("El ID de la foto debe ser mayor que 0");

            RuleFor(x => x.id_usuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");
        }
    }
}
