using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.VideoGames.Commands;
using Domain.VideoGames;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("videogames")]
[ApiController]
public class VideoGamesController(ISender sender, IVideoGameQueries queries) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VideoGameDto>> GetVideoGame(Guid id, CancellationToken cancellationToken)
    {
        var result = await queries.GetById(new VideoGameId(id), cancellationToken);
        return result.Match<ActionResult<VideoGameDto>>(
            game => VideoGameDto.FromDomainModel(game),
            () => NotFound());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VideoGameDto>>> GetVideoGames(CancellationToken cancellationToken)
    {
        var games = await queries.GetAll(cancellationToken);
        return Ok(games.Select(VideoGameDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<VideoGameDto>> CreateVideoGame(
        [FromBody] CreateVideoGameDto request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateVideoGameCommand(
            request.Title, request.Price, request.ReleaseDate, request.Platform), cancellationToken);

        return result.Match<ActionResult<VideoGameDto>>(
            game =>
            {
                var dto = VideoGameDto.FromDomainModel(game);
                return CreatedAtAction(nameof(GetVideoGame), new { id = dto.Id }, dto);
            },
            error => VideoGameErrorFactory.Create(error));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VideoGameDto>> UpdateVideoGame(
        Guid id, [FromBody] UpdateVideoGameDto request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateVideoGameCommand(
            new VideoGameId(id), request.Title, request.Price, request.ReleaseDate, request.Platform),
            cancellationToken);

        return result.Match<ActionResult<VideoGameDto>>(
            game => Ok(VideoGameDto.FromDomainModel(game)),
            error => VideoGameErrorFactory.Create(error));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVideoGame(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteVideoGameCommand(new VideoGameId(id)), cancellationToken);
        return result.Match<IActionResult>(_ => NoContent(), VideoGameErrorFactory.Create);
    }
}
