using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Listings.Commands;
using Domain.Listings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("listings")]
[ApiController]
public class ListingsController(ISender sender, IListingQueries queries) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingDto>> GetListing(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.GetById(new ListingId(id), cancellationToken);
        return result.Match<ActionResult<ListingDto>>(
            game => ListingDto.FromDomainModel(game),
            () => NotFound());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetListings(CancellationToken cancellationToken)
    {
        var games = await queries.GetAll(cancellationToken);
        return Ok(games.Select(ListingDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ListingDto>> CreateListing(
        [FromBody] CreateListingDto request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateListingCommand(
            request.Title, request.Price, request.AvailableSince, request.Category), cancellationToken);

        return result.Match<ActionResult<ListingDto>>(
            game =>
            {
                var dto = ListingDto.FromDomainModel(game);
                return CreatedAtAction(nameof(GetListing), new { id = dto.Id }, dto);
            },
            error => ListingErrorFactory.Create(error));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingDto>> UpdateListing(
        Guid id, [FromBody] UpdateListingDto request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateListingCommand(
            new ListingId(id), request.Title, request.Price, request.AvailableSince, request.Category),
            cancellationToken);

        return result.Match<ActionResult<ListingDto>>(
            game => Ok(ListingDto.FromDomainModel(game)),
            error => ListingErrorFactory.Create(error));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteListing(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteListingCommand(new ListingId(id)), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), ListingErrorFactory.Create);
    }
}
