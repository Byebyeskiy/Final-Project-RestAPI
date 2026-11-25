using Domain.Listings;
using FluentValidation;

namespace Application.Listings.Commands;

public sealed class ModerateListingCommandValidator : AbstractValidator<ModerateListingCommand>
{
    public ModerateListingCommandValidator()
    {
        RuleFor(command => command.Status).IsInEnum().NotEqual(ListingStatus.Pending);

        RuleFor(command => command.RejectionReason).NotEmpty().MaximumLength(500)
            .When(command => command.Status == ListingStatus.Rejected);
        RuleFor(command => command.RejectionReason).Null()
            .When(command => command.Status == ListingStatus.Approved);
    }
}
