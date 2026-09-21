using Application.VideoGames.Services.Abstract;
using Application.VideoGames.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ConfigureApplicationServices
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IVideoGameService, VideoGameService>();
    }
}