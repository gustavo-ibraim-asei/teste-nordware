namespace OrderManagement.Application.DTOs;

public class SizeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class CreateSizeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class UpdateSizeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}



