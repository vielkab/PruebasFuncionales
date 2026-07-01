using System.Net.Http.Json;
using Shouldly;

namespace Api.Tests;

public sealed class UsuarioFeaturesTests : IntegrationTestBase
{
    [Test]
    public async Task ObtenerUsuarios_DebeRetornarUsuariosPaginados()
    {
        using var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=5");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ObtenerUsuariosResponse>();
        content.ShouldNotBeNull();
        content.PageNumber.ShouldBe(1);
        content.PageSize.ShouldBe(5);
        content.Items.Count.ShouldBeLessThanOrEqualTo(5);
        content.TotalItems.ShouldBeGreaterThanOrEqualTo(content.Items.Count);
    }

    [Test]
    public async Task RegistrarUsuario_DebeCrearUsuario()
    {
        var request = new RegistrarUsuarioRequest(
            "Carlos",
            "Mendoza",
            $"carlos.mendoza.{Guid.NewGuid():N}@example.com");

        using var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        content.ShouldNotBeNull();
        content.Id.ShouldNotBe(Guid.Empty);
        content.Nombre.ShouldBe(request.Nombre);
        content.Apellido.ShouldBe(request.Apellido);
        content.Email.ShouldBe(request.Email);
    }

    [Test]
    public async Task ObtenerUsuarioPorId_DebeRetornarUsuarioExistente()
    {
        var usuarios = await ObtenerPrimeraPaginaAsync();
        var usuario = usuarios.Items.First();

        using var response = await Client.GetAsync($"/api/usuarios/{usuario.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        content.ShouldNotBeNull();
        content.Id.ShouldBe(usuario.Id);
    }

    [Test]
    public async Task ObtenerUsuarioPorId_DebeRetornarNotFound_CuandoNoExiste()
    {
        using var response = await Client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ActualizarUsuario_DebeModificarUsuarioExistente()
    {
        var usuario = await RegistrarUsuarioAsync();
        var request = new ActualizarUsuarioRequest(
            "Daniela",
            "Zambrano",
            $"daniela.zambrano.{Guid.NewGuid():N}@example.com");

        using var response = await Client.PutAsJsonAsync($"/api/usuarios/{usuario.Id}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        content.ShouldNotBeNull();
        content.Id.ShouldBe(usuario.Id);
        content.Nombre.ShouldBe(request.Nombre);
        content.Apellido.ShouldBe(request.Apellido);
        content.Email.ShouldBe(request.Email);
    }

    [Test]
    public async Task ActualizarUsuario_DebeRetornarNotFound_CuandoNoExiste()
    {
        var request = new ActualizarUsuarioRequest(
            "Daniela",
            "Zambrano",
            $"daniela.zambrano.{Guid.NewGuid():N}@example.com");

        using var response = await Client.PutAsJsonAsync($"/api/usuarios/{Guid.NewGuid()}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task EliminarUsuario_DebeEliminarUsuarioExistente()
    {
        var usuario = await RegistrarUsuarioAsync();

        using var response = await Client.DeleteAsync($"/api/usuarios/{usuario.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var obtenerResponse = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
        obtenerResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task EliminarUsuario_DebeRetornarNotFound_CuandoNoExiste()
    {
        using var response = await Client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<ObtenerUsuariosResponse> ObtenerPrimeraPaginaAsync()
    {
        using var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=5");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ObtenerUsuariosResponse>();
        content.ShouldNotBeNull();
        content.Items.ShouldNotBeEmpty();

        return content;
    }

    private async Task<UsuarioDto> RegistrarUsuarioAsync()
    {
        var request = new RegistrarUsuarioRequest(
            "Carlos",
            "Mendoza",
            $"carlos.mendoza.{Guid.NewGuid():N}@example.com");

        using var response = await Client.PostAsJsonAsync("/api/usuarios", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<UsuarioDto>();
        content.ShouldNotBeNull();

        return content;
    }

    private sealed record RegistrarUsuarioRequest(string Nombre, string Apellido, string Email);

    private sealed record ActualizarUsuarioRequest(string Nombre, string Apellido, string Email);

    private sealed record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

    private sealed record ObtenerUsuariosResponse(
        IReadOnlyList<UsuarioDto> Items,
        int PageNumber,
        int PageSize,
        int TotalItems,
        int TotalPages);
}
