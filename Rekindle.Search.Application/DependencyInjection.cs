using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Images;
using Rekindle.Search.Application.Users.Interfaces;
using Rekindle.Search.Application.Users.Services;

namespace Rekindle.Search.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IImageSearchService, ImageSearchService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}

public interface IApplicationAssemblyMarker
{
}