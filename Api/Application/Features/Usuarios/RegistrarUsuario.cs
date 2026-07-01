using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Application.Features.Usuarios;

public sealed record RegistrarUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed record RegistrarUsuarioCommand(string Nombre, string Apellido, string Email)
    : IRequest<RegistrarUsuarioResponse>;

public sealed record RegistrarUsuarioResponse(Guid Id, string Nombre, string Apellido, string Email);

public sealed class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
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

public sealed class RegistrarUsuarioCommandHandler(ApplicationDbContext context)
    : IRequestHandler<RegistrarUsuarioCommand, RegistrarUsuarioResponse>
{
    public async Task<RegistrarUsuarioResponse> Handle(
        RegistrarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = new Usuario(
            UsuarioId.From(Guid.NewGuid()),
            request.Nombre,
            request.Apellido,
            Email.From(request.Email));

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);

        return new RegistrarUsuarioResponse(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class RegistrarUsuarioController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar(
        RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await sender.Send(
                new RegistrarUsuarioCommand(request.Nombre, request.Apellido, request.Email),
                cancellationToken);

            return Created($"/api/usuarios/{response.Id}", response);
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
