namespace TravelAgency.Application.DTOs.Travel;

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

public sealed class TravelTicketOptionDto
{
    public string Provider { get; set; } = "";
    public string ProviderOfferId { get; set; } = "";
    public string AirlineCode { get; set; } = "";
    public string AirlineName { get; set; } = "";
    public string DepartureAirportCode { get; set; } = "";
    public string DepartureTime { get; set; } = "";
    public DateTime DepartureAt { get; set; }
    public string ArrivalAirportCode { get; set; } = "";
    public string ArrivalTime { get; set; } = "";
    public string ArrivalDate { get; set; } = "";
    public DateTime ArrivalAt { get; set; }
    public int Stops { get; set; }
    public MoneyDto Price { get; set; } = new(0, "BRL");
}

public sealed class TravelTicketSearchResponse
{
    public List<TravelTicketOptionDto> Items { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
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
    public int Id { get; set; }
    public string CiaAerea { get; set; } = "";
    public string HoraPartidaIda { get; set; } = "";
    public string AeroPartidaIda { get; set; } = "";
    public string DataPartidaIda { get; set; } = "";
    public string HoraChegadaIda { get; set; } = "";
    public string AeroChegadaIda { get; set; } = "";
    public string DataChegadaIda { get; set; } = "";
    public string? HoraPartidaVolta { get; set; }
    public string? AeroPartidaVolta { get; set; }
    public string? DataPartidaVolta { get; set; }
    public string? HoraChegadaVolta { get; set; }
    public string? AeroChegadaVolta { get; set; }
    public string? DataChegadaVolta { get; set; }
    public int Paradas { get; set; }
    public decimal Valor { get; set; }
}
