using Application.Listings.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class ListingErrorFactory
{
    public static ObjectResult Create(ListingException exception)
    {
        var status = exception switch
        {
            ListingAlreadyExistsException or ListingAlreadyModeratedException => StatusCodes.Status409Conflict,
            ListingNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = exception.Message
        }) { StatusCode = status };
    }
}
