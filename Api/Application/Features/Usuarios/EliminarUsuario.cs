using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios;

public sealed record EliminarUsuarioCommand(Guid Id) : IRequest<bool>;

public sealed class EliminarUsuarioCommandValidator : AbstractValidator<EliminarUsuarioCommand>
{
    public EliminarUsuarioCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("El id del usuario es requerido.");
    }
}

public sealed class EliminarUsuarioCommandHandler(ApplicationDbContext context)
    : IRequestHandler<EliminarUsuarioCommand, bool>
{
    public async Task<bool> Handle(
        EliminarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioId.From(request.Id);

        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return false;
        }

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class EliminarUsuarioController(ISender sender) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await sender.Send(new EliminarUsuarioCommand(id), cancellationToken);

            return eliminado ? NoContent() : NotFound();
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
