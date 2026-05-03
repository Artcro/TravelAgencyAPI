using System.Text.Json;
using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Tests.Travel;

public partial class ApiContractCleanupTests
{
    [Fact]
    public void Frontend_Dto_Uses_Exact_Portuguese_Json_Property_Names()
    {
        var item = new FrontendTravelTicketDto();
        var json = JsonSerializer.Serialize(item);

        Assert.Contains("\"ciaAerea\"", json);
        Assert.Contains("\"horaPartidaIda\"", json);
        Assert.Contains("\"dataChegadaVolta\"", json);
        Assert.Contains("\"paradas\"", json);
        Assert.Contains("\"valor\"", json);
    }

    [Fact]
    public void Frontend_Dto_Does_Not_Expose_English_Ticket_Field_Names()
    {
        var json = JsonSerializer.Serialize(new FrontendTravelTicketDto());

        Assert.DoesNotContain("provider", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("providerOfferId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("airlineCode", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("departureAirportCode", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("arrivalAirportCode", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stops", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("price", json, StringComparison.OrdinalIgnoreCase);
    }

}

public partial class ApiContractCleanupTests
{
    [Fact]
    public void Legacy_Travel_Tickets_Controller_File_Is_Removed()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        Assert.False(File.Exists(Path.Combine(root, "src", "TravelAgency.Api", "Controllers", "TravelTicketsController.cs")));
    }

    [Fact]
    public void Trips_Controller_File_Still_Exists()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        Assert.True(File.Exists(Path.Combine(root, "src", "TravelAgency.Api", "Controllers", "TripsController.cs")));
    }
}
