using Api.Dtos;
using Application.VideoGames.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("videogames")]
[ApiController]
public class VideoGamesController(IVideoGameService videoGameService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VideoGameDto>> GetVideoGame(Guid id,
        CancellationToken cancellationToken)
    {
        var videoGame = await videoGameService.GetVideoGame(id, cancellationToken);
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
        var videoGames = await videoGameService.GetVideoGames(cancellationToken);
        return Ok(videoGames.Select(VideoGameDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<VideoGameDto>> CreateVideoGame(
        [FromBody] CreateVideoGameDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var videoGame = await videoGameService.Add(request.Title, request.Price,
                request.ReleaseDate, request.Platform, cancellationToken);
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
            var videoGame = await videoGameService.Update(id, request.Title, request.Price,
                request.ReleaseDate, request.Platform, cancellationToken);
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
        var deleted = await videoGameService.Delete(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}