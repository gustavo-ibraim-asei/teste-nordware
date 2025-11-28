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
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));

        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Neighborhood cannot be empty", nameof(neighborhood));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));

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


