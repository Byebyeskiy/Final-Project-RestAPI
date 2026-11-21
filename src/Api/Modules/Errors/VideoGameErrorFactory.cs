using Application.VideoGames.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class VideoGameErrorFactory
{
    public static ObjectResult Create(VideoGameException exception)
    {
        var status = exception switch
        {
            VideoGameAlreadyExistsException => StatusCodes.Status409Conflict,
            VideoGameNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = exception.Message
        }) { StatusCode = status };
    }
}
