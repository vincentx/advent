using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Advent.Kernel.Factory;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel.Api;

public class ApiTests : IClassFixture<ApiFactory<Program>>
{
    private readonly ApiFactory<Program> _factory;

    public ApiTests(ApiFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task should_return_bad_request_if_no_backend_provide_for_list_skills()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/api/skills");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task should_return_skill_list()
    {
        var httpClient = _factory.CreateClient();

        httpClient.DefaultRequestHeaders.Add(Headers.TextCompletionKey, "key");
        httpClient.DefaultRequestHeaders.Add(Headers.EmbeddingKey, "key");
        

        var list = await httpClient.GetFromJsonAsync<SkillFunctionList>("/api/skills");
        Assert.Equal(43, list.Skills.Count);
        var planner = list.Skills.First(_ => _.Function == "CreatePlan");
        Assert.Equal("PlannerSkill", planner.Skill);
        Assert.Equal(1, planner.Links.Count);
        Assert.Equal("/api/skills/plannerskill/createplan", planner.Links["self"].Href);
    }

    [Fact]
    public async Task should_return_bad_request_if_no_backend_provide_for_list_skill_details()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/api/skills/plannerskill/createplan");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task should_return_skill_details()
    {
        var httpClient = _factory.CreateClient();

        httpClient.DefaultRequestHeaders.Add(Headers.TextCompletionKey, "key");
        httpClient.DefaultRequestHeaders.Add(Headers.EmbeddingKey, "key");

        var function = await httpClient.GetFromJsonAsync<FunctionView>("/api/skills/timeskill/timezonename");
        Assert.Equal("TimeZoneName", function.Name);
        Assert.Equal("TimeSkill", function.SkillName);
        Assert.Equal("Get the local time zone name", function.Description);
        Assert.False(function.IsSemantic);
        Assert.False(function.IsAsynchronous);
        Assert.Empty(function.Parameters);
    }

    [Fact]
    public async Task should_run_piped_functions()
    {
        var httpClient = _factory.CreateClient();

        httpClient.DefaultRequestHeaders.Add(Headers.TextCompletionKey, "key");
        httpClient.DefaultRequestHeaders.Add(Headers.EmbeddingKey, "key");

        var response = await httpClient.PostAsJsonAsync("/api/asks", new Message()
        {
            Variables = new Dictionary<string, string>() { ["INPUT"] = " lower " },
            Pipeline = new List<Message.FunctionRef>
            {
                new()
                {
                    Skill = "TextSkill", Name = "Uppercase"
                },
                new()
                {
                    Skill = "TextSkill", Name = "TrimEnd"
                }
            }
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var message = await response.Content.ReadFromJsonAsync<Message>();
        Assert.Equal(" LOWER", message.Variables.ToDictionary(_ => _.Key, _ => _.Value)["INPUT"]);
    }

    [Fact]
    public async Task should_not_run_piped_functions_if_no_backend_provided()
    {
        var httpClient = _factory.CreateClient();

        var response = await httpClient.PostAsJsonAsync("/api/asks", new Message()
        {
            Variables = new Dictionary<string, string>() { ["INPUT"] = " lower " },
            Pipeline = new List<Message.FunctionRef>
            {
                new()
                {
                    Skill = "TextSkill", Name = "Uppercase"
                },
                new()
                {
                    Skill = "TextSkill", Name = "TrimEnd"
                }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task should_not_run_piped_functions_if_function_not_selected()
    {
        var httpClient = _factory.CreateClient();

        httpClient.DefaultRequestHeaders.Add(Headers.TextCompletionKey, "key");

        var response = await httpClient.PostAsJsonAsync("/api/asks", new Message()
        {
            Variables = new Dictionary<string, string>() { ["INPUT"] = " lower " },
            Pipeline = new List<Message.FunctionRef>
            {
                new()
                {
                    Skill = "TextSkill", Name = "Uppercase"
                },
                new()
                {
                    Skill = "TextSkill", Name = "TrimEnd"
                }
            },
            Skills = new List<string> { "OtherSkill" }
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task should_run_end_to_end()
    {
        var httpClient = _factory.CreateClient();

        httpClient.DefaultRequestHeaders.Add(Headers.TextCompletionKey, "key");
        httpClient.DefaultRequestHeaders.Add(Headers.EmbeddingKey, "key");

        var response = await httpClient.PostAsJsonAsync("/api/asks", new Message()
        {
            Variables = new Dictionary<string, string>() { ["INPUT"] = " lower " },
            Pipeline = new List<Message.FunctionRef>()
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(PlanExecutor.Called);
    }
}

public record SkillFunctionList
{
    public IList<SkillFunction> Skills { get; set; }
}

public record SkillFunction
{
    public string Skill { get; set; }
    public string Function { get; set; }

    [JsonPropertyName("_links")] public IDictionary<string, SkillFunctionLink> Links { get; set; }
}

public record SkillFunctionLink
{
    public string Href { get; set; }
}

public class ApiFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Replace(ServiceDescriptor.Singleton<IPlanExecutor, PlanExecutor>());
        });
        return base.CreateHost(builder);
    }
}

class PlanExecutor : IPlanExecutor
{
    public static bool Called = false;

    public Task<SKContext> Execute(IKernel kernel, Message message, int iterations)
    {
        Called = true;
        return Task.FromResult(kernel.CreateNewContext());
    }

    static ContextVariables ToContext(IEnumerable<KeyValuePair<string, string>> variables)
    {
        var context = new ContextVariables();
        foreach (var variable in variables) context[variable.Key] = variable.Value;
        return context;
    }
}