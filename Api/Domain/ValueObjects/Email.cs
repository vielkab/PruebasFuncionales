using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Contains('@')
            ? Validation.Ok
            : Validation.Invalid("El correo no tiene un formato valido.");
    }
}
