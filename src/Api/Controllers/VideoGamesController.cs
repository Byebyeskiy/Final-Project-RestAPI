using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.VideoGames.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("videogames")]
[ApiController]
public class VideoGamesController(ISender sender, IVideoGameQueries queries) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VideoGameDto>> GetVideoGame(Guid id,
        CancellationToken cancellationToken)
    {
        var videoGame = await queries.GetById(id, cancellationToken);
        if (videoGame is null)
        {
            return NotFound();
        }

        return VideoGameDto.FromDomainModel(videoGame);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VideoGameDto>>> GetVideoGames(
        CancellationToken cancellationToken)
    {
        var videoGames = await queries.GetAll(cancellationToken);
        return Ok(videoGames.Select(VideoGameDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<VideoGameDto>> CreateVideoGame(
        [FromBody] CreateVideoGameDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var videoGame = await sender.Send(new CreateVideoGameCommand(request.Title, request.Price,
                request.ReleaseDate, request.Platform), cancellationToken);
            var dto = VideoGameDto.FromDomainModel(videoGame);

            return CreatedAtAction(nameof(GetVideoGame), new { id = dto.Id }, dto);
        }
        catch (ArgumentException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VideoGameDto>> UpdateVideoGame(Guid id,
        [FromBody] UpdateVideoGameDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var videoGame = await sender.Send(new UpdateVideoGameCommand(id, request.Title, request.Price,
                request.ReleaseDate, request.Platform), cancellationToken);
            if (videoGame is null)
            {
                return NotFound();
            }

            return Ok(VideoGameDto.FromDomainModel(videoGame));
        }
        catch (ArgumentException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVideoGame(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(new DeleteVideoGameCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
