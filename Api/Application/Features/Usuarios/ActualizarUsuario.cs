using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios;

public sealed record ActualizarUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed record ActualizarUsuarioCommand(Guid Id, string Nombre, string Apellido, string Email)
    : IRequest<UsuarioDto?>;

public sealed class ActualizarUsuarioCommandValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("El id del usuario es requerido.");

        RuleFor(command => command.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(command => command.Apellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El correo es requerido.")
            .EmailAddress().WithMessage("El correo no tiene un formato valido.")
            .MaximumLength(256).WithMessage("El correo no puede superar los 256 caracteres.");
    }
}

public sealed class ActualizarUsuarioCommandHandler(ApplicationDbContext context)
    : IRequestHandler<ActualizarUsuarioCommand, UsuarioDto?>
{
    public async Task<UsuarioDto?> Handle(
        ActualizarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioId.From(request.Id);

        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        usuario.Actualizar(request.Nombre, request.Apellido, Email.From(request.Email));

        await context.SaveChangesAsync(cancellationToken);

        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ActualizarUsuarioController(ISender sender) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid id,
        ActualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await sender.Send(
                new ActualizarUsuarioCommand(id, request.Nombre, request.Apellido, request.Email),
                cancellationToken);

            return usuario is null ? NotFound() : Ok(usuario);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return BadRequest(new ValidationProblemDetails(errors));
        }
    }
}
