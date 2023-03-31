using System.Text.Json;

namespace Advent.Kernel;

public class MessageTests
{
    [Fact]
    public void should_be_able_to_serialize_end_to_end_message_to_json()
    {
        var message = Message.Ask("my goal", new() { ["PARAM"] = "PARAM_VALUE" });
        var json = JsonSerializer.Serialize(message,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var deserialized = JsonSerializer.Deserialize<Message>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


        Assert.Equal(2, deserialized?.Variables.Count());
        Assert.Contains("INPUT", deserialized.Variables.Select(_ => _.Key));
        Assert.Contains("my goal", deserialized.Variables.Select(_ => _.Value));
        Assert.Contains("PARAM", deserialized.Variables.Select(_ => _.Key));
        Assert.Contains("PARAM_VALUE", deserialized.Variables.Select(_ => _.Value));
        Assert.Equal(1, deserialized.Pipeline?.Count());
        Assert.Equal("PlannerSkill", deserialized.Pipeline?[0].Skill);
        Assert.Equal("ExecutePlan", deserialized.Pipeline?[0].Name);
    }

    [Fact]
    public void should_be_able_to_serialize_chained_message_to_json()
    {
        var message = Message.Ask(new() { ["PARAM"] = "PARAM_VALUE" },
            new Message.FunctionRef()
            {
                Skill = "PlannerSkill", Name = "CreatePlan"
            },
            new Message.FunctionRef()
            {
                Skill = "PlannerSkill", Name = "ExecutePlan"
            });
        var json = JsonSerializer.Serialize(message,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var deserialized = JsonSerializer.Deserialize<Message>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


        Assert.Equal(2, deserialized?.Variables.Count());
        Assert.Contains("PARAM", deserialized.Variables.Select(_ => _.Key));
        Assert.Contains("PARAM_VALUE", deserialized.Variables.Select(_ => _.Value));
        Assert.Contains("INPUT", deserialized.Variables.Select(_ => _.Key));
        Assert.Contains("", deserialized.Variables.Select(_ => _.Value));
        Assert.Equal(2, deserialized.Pipeline?.Count());
        Assert.Equal("PlannerSkill", deserialized.Pipeline?[0].Skill);
        Assert.Equal("CreatePlan", deserialized.Pipeline?[0].Name);
        Assert.Equal("PlannerSkill", deserialized.Pipeline?[1].Skill);
        Assert.Equal("ExecutePlan", deserialized.Pipeline?[1].Name);
    }
}