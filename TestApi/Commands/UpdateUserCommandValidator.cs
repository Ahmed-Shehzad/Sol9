﻿// <auto-generated />
using System.Threading;
using System.Threading.Tasks;
using Intercessor.Abstractions;
using FluentValidation;

namespace TestApi.Commands;

public class UpdateUserCommandValidator: AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull();
    }
}