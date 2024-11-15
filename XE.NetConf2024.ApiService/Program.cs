using Microsoft.OpenApi.Any;
using Scalar.AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // HACK: Remove the servers from the document because the app run in a container.
        document.Servers.Clear();
        return Task.CompletedTask;
    });

    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Todo))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["title"] = new OpenApiString("A sample title"),
                ["description"] = new OpenApiString("A long description"),
                ["createdOn"] = new OpenApiDateTime(DateTime.Now)
            };
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // GET /openapi/v1.json
    app.MapOpenApi();

    // GET /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });

    // GET /scalar/v1
    app.MapScalarApiReference();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/todos", (Todo todo) => { });

app.MapDefaultEndpoints();

//if(app.Environment.IsDevelopment())
//{
//    app.MapGet("/", () => Results.LocalRedirect("/swagger"));
//}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Todo
{
    public int Id { get; init; }
    public required string Title { get; init; }
    [DefaultValue("A new todo")]
    public required string Description { get; init; }
    [Required]
    public DateTime CreatedOn { get; init; }
}
