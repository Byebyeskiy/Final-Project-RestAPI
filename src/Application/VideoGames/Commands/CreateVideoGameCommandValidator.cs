using FluentValidation;

namespace Application.VideoGames.Commands;

public sealed class CreateVideoGameCommandValidator : AbstractValidator<CreateVideoGameCommand>
{
    public CreateVideoGameCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(255);
        RuleFor(command => command.Price).InclusiveBetween(0.01m, 1_000_000m);
        RuleFor(command => command.ReleaseDate).NotEmpty();
        RuleFor(command => command.Platform).IsInEnum();
    }
}
