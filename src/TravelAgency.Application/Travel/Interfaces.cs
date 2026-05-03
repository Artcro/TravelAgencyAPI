using TravelAgency.Application.DTOs.Travel;

namespace TravelAgency.Application.Travel;

public interface ITripSearchService
{
    Task<TripSearchResponse> SearchAsync(TripSearchRequest request, Guid? userId, CancellationToken cancellationToken);
    Task<TripSearchResponse?> GetSearchByIdAsync(Guid searchId, Guid? userId, CancellationToken cancellationToken);
}

public interface ISavedTripService
{
    Task<SavedTripResponse> SaveAsync(SaveTripRequest request, Guid? userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<object>> ListAsync(Guid? userId, CancellationToken cancellationToken);
    Task<object?> GetByIdAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid tripId, Guid? userId, CancellationToken cancellationToken);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}


public interface IFrontendTravelTicketService
{
    Task<IReadOnlyList<FrontendTravelTicketDto>> SearchAsync(FrontendTravelTicketSearchRequest request, CancellationToken cancellationToken);
}
