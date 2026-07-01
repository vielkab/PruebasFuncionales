using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios;

public sealed record ObtenerUsuarioPorIdQuery(Guid Id) : IRequest<UsuarioDto?>;

public sealed class ObtenerUsuarioPorIdQueryValidator : AbstractValidator<ObtenerUsuarioPorIdQuery>
{
    public ObtenerUsuarioPorIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty().WithMessage("El id del usuario es requerido.");
    }
}

public sealed class ObtenerUsuarioPorIdQueryHandler(ApplicationDbContext context)
    : IRequestHandler<ObtenerUsuarioPorIdQuery, UsuarioDto?>
{
    public async Task<UsuarioDto?> Handle(
        ObtenerUsuarioPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioId.From(request.Id);

        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ObtenerUsuarioPorIdController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await sender.Send(new ObtenerUsuarioPorIdQuery(id), cancellationToken);

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
