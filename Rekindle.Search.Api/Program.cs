using System.Text.Json;
using System.Text.Json.Serialization;
using Rekindle.Exceptions.Api;
using Rekindle.Search.Api.Routes;
using Rekindle.Search.Application;
using Rekindle.Search.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => { o.SwaggerEndpoint("/openapi/v1.json", "Rekindle Search API"); });
});

app.UseExceptionHandlingMiddleware();

app.MapSearchEndpoints();
//app.UseHttpsRedirection();

app.Run();