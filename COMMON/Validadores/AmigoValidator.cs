﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COMMON.Entidades;
using FluentValidation;

namespace COMMON.Validadores
{
    public class AmigoValidator:AbstractValidator<Amigo>
    {
        public AmigoValidator()
        {
            RuleFor(x => x.IdUsuario)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio")
                .GreaterThan(0).WithMessage("El ID del usuario debe ser mayor que 0");

            RuleFor(x => x.IdAmigoUsuario)
                .NotEmpty().WithMessage("El ID del amigo es obligatorio")
                .GreaterThan(0).WithMessage("El ID del amigo debe ser mayor que 0")
                .NotEqual(x => x.IdUsuario).WithMessage("El usuario y el amigo no pueden ser el mismo");

            RuleFor(x => x.Estado)
                .NotEmpty().WithMessage("El estado es obligatorio")
                .Must(estado => estado == "pendiente" || estado == "aceptado" || estado == "rechazado")
                .WithMessage("El estado debe ser 'pendiente', 'aceptado' o 'rechazado'");

            RuleFor(x => x.FechaAceptacion)
                .Must((amigo, fechaAceptacion) => !fechaAceptacion.HasValue || amigo.Estado == "aceptado")
                .WithMessage("La fecha de aceptación solo debe establecerse cuando el estado es 'aceptado'");
        }
    }
}
