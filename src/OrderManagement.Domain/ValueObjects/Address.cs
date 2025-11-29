namespace OrderManagement.Domain.ValueObjects;

public class Address
{
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;

    private Address() { } // EF Core

    public Address(string street, string number, string neighborhood, string city, string state, string zipCode, string? complement = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("A rua não pode ser vazia", nameof(street));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("O número não pode ser vazio", nameof(number));

        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("O bairro não pode ser vazio", nameof(neighborhood));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("A cidade não pode ser vazia", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("O estado não pode ser vazio", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("O CEP não pode ser vazio", nameof(zipCode));

        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public override string ToString()
    {
        return $"{Street}, {Number} - {Complement ?? ""} - {Neighborhood}, {City}/{State} - {ZipCode}";
    }
}





