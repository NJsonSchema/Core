using System;
using System.Threading.Tasks;
using Xunit;

namespace NJsonSchema.CodeGeneration.CSharp.Tests;

public class ResolveExistingTypeTests
{
    [Fact]
    public async Task String_schema_can_use_existing_type()
    {
        var json = @"{
            ""type"": ""object"",
            ""properties"": {
                ""ip"": {
                    ""type"": ""string"",
                    ""pattern"": ""^\\d{1,3}(\\.\\d{1,3}){3}$"",
                    ""x-cSharpExistingType"": ""System.Net.IPAddress""
                }
            }
        }";
        
        var schema = await JsonSchema.FromJsonAsync(json);

        // Act
        var code = new CSharpGenerator(schema).GenerateFile("MyClass");

        //// Act
        Assert.Contains("public System.Net.IPAddress Ip { get; set; }", code);
    }

    [Fact]
    public async Task String_schema_can_use_existing_type_in_definition()
    {
        var json = @"{
            ""type"": ""object"",
            ""properties"": {
                ""ip"": { ""$ref"": ""#/definitions/ip"" }
            },
            ""definitions"": {
                ""ip"": {
                    ""type"": ""string"",
                    ""pattern"": ""^\\d{1,3}(\\.\\d{1,3}){3}$"",
                    ""x-cSharpExistingType"": ""System.Net.IPAddress""
                }
            }
        }";
        
        var schema = await JsonSchema.FromJsonAsync(json);

        // Act
        var code = new CSharpGenerator(schema).GenerateFile("MyClass");

        //// Act
        Assert.Contains("public System.Net.IPAddress Ip { get; set; }", code);
    }
}
