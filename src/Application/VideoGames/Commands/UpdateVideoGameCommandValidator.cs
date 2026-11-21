using FluentValidation;

namespace Application.VideoGames.Commands;

public sealed class UpdateVideoGameCommandValidator : AbstractValidator<UpdateVideoGameCommand>
{
    public UpdateVideoGameCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(255);
        RuleFor(command => command.Price).InclusiveBetween(0.01m, 1_000_000m);
        RuleFor(command => command.ReleaseDate).NotEmpty();
        RuleFor(command => command.Platform).IsInEnum();
    }
}
