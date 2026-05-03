namespace TravelAgency.Application.DTOs.Travel;

using System.Text.Json.Serialization;

public sealed class TravelTicketSearchRequest
{
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    public DateOnly? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; }
    public int Infants { get; set; }
    public string Currency { get; set; } = "BRL";
    public string TravelClass { get; set; } = "ECONOMY";
    public int MaxResults { get; set; } = 10;
}

public sealed class FrontendTravelTicketSearchRequest
{
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateOnly DepartureDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    public DateOnly? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int Children { get; set; }
    public int Infants { get; set; }
    public string Currency { get; set; } = "BRL";
    public string TravelClass { get; set; } = "ECONOMY";
    public int MaxResults { get; set; } = 10;
}

public sealed class FrontendTravelTicketDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("ciaAerea")]
    public string CiaAerea { get; set; } = "";
    [JsonPropertyName("horaPartidaIda")]
    public string HoraPartidaIda { get; set; } = "";
    [JsonPropertyName("aeroPartidaIda")]
    public string AeroPartidaIda { get; set; } = "";
    [JsonPropertyName("dataPartidaIda")]
    public string DataPartidaIda { get; set; } = "";
    [JsonPropertyName("horaChegadaIda")]
    public string HoraChegadaIda { get; set; } = "";
    [JsonPropertyName("aeroChegadaIda")]
    public string AeroChegadaIda { get; set; } = "";
    [JsonPropertyName("dataChegadaIda")]
    public string DataChegadaIda { get; set; } = "";
    [JsonPropertyName("horaPartidaVolta")]
    public string? HoraPartidaVolta { get; set; }
    [JsonPropertyName("aeroPartidaVolta")]
    public string? AeroPartidaVolta { get; set; }
    [JsonPropertyName("dataPartidaVolta")]
    public string? DataPartidaVolta { get; set; }
    [JsonPropertyName("horaChegadaVolta")]
    public string? HoraChegadaVolta { get; set; }
    [JsonPropertyName("aeroChegadaVolta")]
    public string? AeroChegadaVolta { get; set; }
    [JsonPropertyName("dataChegadaVolta")]
    public string? DataChegadaVolta { get; set; }
    [JsonPropertyName("paradas")]
    public int Paradas { get; set; }
    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }
}
