using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Usuarios.AnyAsync())
        {
            return;
        }

        Randomizer.Seed = new Random(2026);

        var usuarios = new Faker<Usuario>("es")
            .CustomInstantiator(faker =>
            {
                var nombre = faker.Name.FirstName();
                var apellido = faker.Name.LastName();
                var email = faker.Internet.Email(nombre, apellido).ToLowerInvariant();

                return new Usuario(
                    UsuarioId.From(Guid.NewGuid()),
                    nombre,
                    apellido,
                    Email.From(email));
            })
            .Generate(15);

        context.Usuarios.AddRange(usuarios);
        await context.SaveChangesAsync();
    }
}
