using Domain.Listings;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IListingQueries
{
    Task<IReadOnlyList<Listing>> GetAll(CancellationToken cancellationToken);
    Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken);
}
