namespace OrderManagement.Application.DTOs;

public class ColorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class CreateColorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class UpdateColorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}



