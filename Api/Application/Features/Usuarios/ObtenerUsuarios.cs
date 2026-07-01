using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios;

public sealed record ObtenerUsuariosQuery(int PageNumber, int PageSize)
    : IRequest<ObtenerUsuariosResponse>;

public sealed record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed record ObtenerUsuariosResponse(
    IReadOnlyList<UsuarioDto> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed class ObtenerUsuariosQueryValidator : AbstractValidator<ObtenerUsuariosQuery>
{
    public ObtenerUsuariosQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThan(0).WithMessage("El numero de pagina debe ser mayor a cero.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50).WithMessage("El tamano de pagina debe estar entre 1 y 50.");
    }
}

public sealed class ObtenerUsuariosQueryHandler(ApplicationDbContext context)
    : IRequestHandler<ObtenerUsuariosQuery, ObtenerUsuariosResponse>
{
    public async Task<ObtenerUsuariosResponse> Handle(
        ObtenerUsuariosQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var usuarios = await context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = usuarios
            .Select(usuario => new UsuarioDto(
                usuario.Id.Value,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email.Value))
            .ToList();

        return new ObtenerUsuariosResponse(
            items,
            request.PageNumber,
            request.PageSize,
            totalItems,
            totalPages);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ObtenerUsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarios = await sender.Send(
                new ObtenerUsuariosQuery(pageNumber, pageSize),
                cancellationToken);

            return Ok(usuarios);
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
