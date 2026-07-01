using Api.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios;

public sealed record ObtenerUsuariosQuery : IRequest<IReadOnlyList<UsuarioDto>>;

public sealed record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed class ObtenerUsuariosQueryHandler(ApplicationDbContext context)
    : IRequestHandler<ObtenerUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    public async Task<IReadOnlyList<UsuarioDto>> Handle(
        ObtenerUsuariosQuery request,
        CancellationToken cancellationToken)
    {
        var usuarios = await context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .ThenBy(usuario => usuario.Apellido)
            .ToListAsync(cancellationToken);

        return usuarios
            .Select(usuario => new UsuarioDto(
                usuario.Id.Value,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Email.Value))
            .ToList();
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ObtenerUsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var usuarios = await sender.Send(new ObtenerUsuariosQuery(), cancellationToken);

        return Ok(usuarios);
    }
}
