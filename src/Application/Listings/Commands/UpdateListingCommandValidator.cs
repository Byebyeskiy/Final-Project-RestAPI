using FluentValidation;

namespace Application.Listings.Commands;

public sealed class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    public UpdateListingCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(255);
        RuleFor(command => command.Price).InclusiveBetween(0.01m, 1_000_000m);
        RuleFor(command => command.AvailableSince).NotEmpty();
        RuleFor(command => command.Category).IsInEnum();
    }
}
