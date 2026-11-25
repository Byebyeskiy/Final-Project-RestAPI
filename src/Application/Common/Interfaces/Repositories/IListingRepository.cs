using Domain.Listings;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IListingRepository
{
    Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken);
    Task<Option<Listing>> GetByTitle(string title, CancellationToken cancellationToken);
    Task<Listing> Add(Listing listing, CancellationToken cancellationToken);
    Task<Listing> Update(Listing listing, CancellationToken cancellationToken);
    Task Delete(Listing listing, CancellationToken cancellationToken);
}
