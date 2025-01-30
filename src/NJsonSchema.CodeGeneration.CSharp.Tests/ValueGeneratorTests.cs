using System.ComponentModel.DataAnnotations;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.NewtonsoftJson.Generation;
using Xunit;

namespace NJsonSchema.CodeGeneration.Tests.CSharp
{
    public class ValueGeneratorTests
    {
        public class RangeClass
        {
            [Range(2, Double.MaxValue)]
            public int Foo { get; set; }
        }

        [Fact]
        public void When_schema_contains_range_then_code_is_correctly_generated()
        {
            // Arrange
            var schema = NewtonsoftJsonSchemaGenerator.FromType<RangeClass>();

            // Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                SchemaType = SchemaType.Swagger2
            });
            var code = generator.GenerateFile("MyClass");

            // Assert
            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(2, int.MaxValue)]", code);
        }

        [Theory]
        [InlineData("integer", JsonFormatStrings.Integer, "1, int.MaxValue")]
        [InlineData("integer", JsonFormatStrings.Long, "1L, long.MaxValue")]
        [InlineData("integer", JsonFormatStrings.ULong, "1UL, ulong.MaxValue")]
        [InlineData("number", JsonFormatStrings.Float, "1F, float.MaxValue")]
        [InlineData("number", JsonFormatStrings.Double, "1D, double.MaxValue")]
        [InlineData("number", JsonFormatStrings.Decimal, "1M, decimal.MaxValue")]
        public async Task When_schema_contains_range_and_format_then_code_is_correctly_generated(string propertyType,
            string propertyFormat, string expectedRange)
        {
            /// Arrange
            var json = $$"""
                         {
                             "type": "object",
                             "properties": {
                                "pageSize": {
                                 	"type": "{{propertyType}}",
                                 	"format": "{{propertyFormat}}",
                                 	"minimum": 1
                                },
                                 }
                             }
                         """;
            var schema = await JsonSchema.FromJsonAsync(json);

            //// Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                SchemaType = SchemaType.Swagger2
            });
            var code = generator.GenerateFile("MyClass");

            //// Assert
            Assert.Contains($"[System.ComponentModel.DataAnnotations.Range({expectedRange})]", code);
        }

        [Fact]
        public async Task When_property_is_integer_and_no_format_is_available_then_default_value_is_int32()
        {
            // Arrange
            var json = @"{
                ""type"": ""object"",
                ""properties"": {
	                ""pageSize"": {
		                ""type"": ""integer"",
		                ""default"": 10,
		                ""minimum"": 1
	                },
	                ""pagingSize"": {
		                ""type"": ""integer"",
		                ""default"": 5,
		                ""minimum"": 1
	                }
                }
            }";
            var schema = await JsonSchema.FromJsonAsync(json);

            // Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                SchemaType = SchemaType.Swagger2
            });
            var code = generator.GenerateFile("MyClass");

            // Assert
            Assert.Contains("public int? PageSize { get; set; } = 10;", code);
        }

        [Fact]
        public async Task When_property_is_string_and_format_is_date_time_then_assign_default_value()
        {
            // Arrange
            var json = @"{
                ""type"": ""object"",
                ""properties"": {
	                ""dateTime"": {
		                ""type"": ""string"",
		                ""format"": ""date-time"",
		                ""default"": ""31.12.9999 23:59:59""
	                }
                }
            }";
            var schema = await JsonSchema.FromJsonAsync(json);

            // Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                SchemaType = SchemaType.Swagger2,
                DateTimeType = "System.DateTime"
            });
            var code = generator.GenerateFile("MyClass");

            // Assert
            Assert.Contains("public System.DateTime? DateTime { get; set; } = System.DateTime.Parse(\"31.12.9999 23:59:59\");", code);
        }
    }
}
