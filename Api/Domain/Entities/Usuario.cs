using Api.Domain.Common;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity<UsuarioId>
{
    private Usuario()
    {
    }

    public Usuario(UsuarioId id, string nombre, string apellido, Email email)
    {
        Id = id;
        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Email = email;
    }

    public string Nombre { get; private set; } = string.Empty;

    public string Apellido { get; private set; } = string.Empty;

    public Email Email { get; private set; }
}
