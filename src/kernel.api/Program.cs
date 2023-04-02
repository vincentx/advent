using Advent.Kernel;
using Advent.Kernel.Factory;
using Advent.Kernel.WebApi;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile("advent.json", true).Build();

var builder = WebApplication.CreateBuilder(args);

var skills = config.GetSection("Skills").Get<string[]>() ?? new[] { "./skills" };

foreach (var folder in skills)
{
    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
}

builder.Services.AddConsoleLogger(config);
builder.Services.AddSemanticKernelFactory(config);

builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(int.Parse(config["Port"] ?? "5000")); });

var app = builder.Build();


app.MapGet("/api/skills", ([FromServices] SemanticKernelFactory factory, HttpRequest request) =>
    request.TryGetKernel(factory, out var kernel)
        ? Results.Ok(
            new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["skills"] = (from function in kernel!.ToSkills()
                        select new Dictionary<string, object>
                        {
                            ["skill"] = function.SkillName,
                            ["function"] = function.Name,
                            ["_links"] = new Dictionary<string, object>
                            {
                                ["self"] = new Dictionary<string, object>
                                {
                                    ["href"] = ($"/api/skills/{function.SkillName}/{function.Name}".ToLower())
                                }
                            }
                        })
                    .ToList()
            })
        : Results.BadRequest("API config is not valid"));

app.MapGet("/api/skills/{skill}/{function}",
    (string skill, string function, SemanticKernelFactory factory, HttpRequest request) =>
        request.TryGetKernel(factory, out var kernel)
            ? kernel!.Skills.HasFunction(skill, function)
                ? Results.Ok(kernel.Skills.GetFunction(skill, function).Describe())
                : Results.NotFound()
            : Results.BadRequest("API config is not valid"));

app.MapPost("/api/asks",
    async ([FromQuery(Name = "iterations")] int? iterations, Message message,
            SemanticKernelFactory factory, IPlanExecutor planExecutor, HttpRequest request) =>
        request.TryGetKernel(factory, out var kernel, message.Skills)
            ? (message.Pipeline == null || message.Pipeline.Count == 0
                ? await planExecutor.Execute(kernel!, message, iterations ?? 10)
                : await kernel!.InvokePipedFunctions(message)).ToResult(message.Skills)
            : Results.BadRequest("API config is not valid"));


app.UseExceptionHandler(handler =>
    handler.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()!.Error;
        switch (exception)
        {
            case KernelException { ErrorCode: KernelException.ErrorCodes.FunctionNotAvailable } kernelException:
                await Results.BadRequest(kernelException.Message).ExecuteAsync(context);
                break;
            default:
                await Results.Problem(exception.Message).ExecuteAsync(context);
                break;
        }
    }));

app.Run();

public partial class Program
{
}