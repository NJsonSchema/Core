using System.Reflection;
using Xunit;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;

namespace NJsonSchema.Tests.References
{
#pragma warning disable SYSLIB0012

    public class LocalReferencesTests
    {
        [Fact]
        public async Task When_definitions_is_nested_then_refs_work()
        {
            // Arrange
            var json = @"{
	""type"": ""object"", 
	""properties"": {
		""foo"": {
			""$ref"": ""#/definitions/collection/bar""
		}
	},
	""definitions"": {
		""collection"": {
			""bar"": {
				""type"": ""integer""
			}
		}
	}
}";

            // Act
            var schema = await JsonSchema.FromJsonAsync(json);
            var j = schema.ToJson();

            // Assert
            Assert.Equal(JsonObjectType.Integer, schema.Properties["foo"].ActualTypeSchema.Type);
        }

        [Fact]
        public async Task When_schema_references_collection_in_definitions_it_works()
        {
            // Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/schema_with_collection_reference.json";

            // Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            // Assert
            Assert.Equal(JsonObjectType.Integer, schema.Properties["foo"].ActualTypeSchema.Type);
            Assert.Single(schema.Definitions);
            Assert.Equal("./collection.json", schema.Definitions["collection"].DocumentPath);
        }

        [Fact]
        public async Task When_schema_references_external_schema_then_it_is_inlined_with_ToJson()
        {
            // Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/schema_with_reference.json";

            // Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            // Assert
            Assert.True(schema.Definitions.ContainsKey("Animal"));
            Assert.Contains("\"$ref\": \"#/definitions/Animal\"", json);
        }

        [Fact]
        public async Task When_document_has_indirect_external_ref_than_it_is_loaded()
        {
            // Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/schema_with_indirect_reference.json";

            // Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            // Assert
            Assert.Single(schema.Definitions);
            Assert.Equal("FooAnimal", schema.Definitions.Single().Key);
        }

        [Fact]
        public async Task When_document_has_indirect_external_ref_to_a_definition_than_it_is_loaded()
        {
            // Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/schema_with_indirect_subreference.json";

            // Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            // Assert
            Assert.Single(schema.Definitions);
            Assert.Equal("SubAnimal", schema.Definitions.Single().Key);
        }

        [Fact]
        public async Task When_document_has_indirect_external_ref_to_a_schema_with_sibling_then_it_is_loaded()
        {
            //// Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/schema_with_indirect_subreference_sibling.json";

            //// Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            //// Assert
            // TODO
        }

        [Fact]
        public async Task When_reference_is_registered_in_custom_resolver_it_should_not_try_to_access_file()
        {
            // Arrange
            var externalSchema = await JsonSchema.FromJsonAsync(
            @"{
                ""type"": ""object"", 
                ""properties"": {
                    ""foo"": {
                        ""type"": ""string""
                    }
                }
            }");

            Func<JsonSchema, JsonReferenceResolver> factory = schema =>
            {
                var schemaResolver = new JsonSchemaResolver(schema, new NewtonsoftJsonSchemaGeneratorSettings());
                var resolver = new JsonReferenceResolver(schemaResolver);
                resolver.AddDocumentReference("../dir/external.json", externalSchema);
                return resolver;
            };

            string schemaJson =
            @"{
                ""$schema"": ""http://json-schema.org/draft-07/schema#"",
                ""type"": ""object"",
                ""properties"": {
                    ""title"": {
                        ""$ref"": ""../dir/external.json#""
                    }
                }
            }";

            // Act
            var schema = await JsonSchema.FromJsonAsync(schemaJson, ".", factory);

            // Assert
            Assert.NotNull(schema);
        }

        [Theory]
        //[InlineData("b%23r", Skip = "Not working ATM")] // Escaped well-formed JSON Pointer
        [InlineData("b#r")] // Non-escaped ill-formed JSON Pointer
        public async Task When_definitions_have_sharp_in_type_name(string referenceTypeName)
        {
            // Arrange
            var json = $@"{{
	""type"": ""object"", 
	""properties"": {{
		""foo"": {{
			""$ref"": ""#/definitions/{referenceTypeName}""
		}}
	}},
	""definitions"": {{
		""b#r"": {{
			""type"": ""integer""
		}}
	}}
}}";

            // Act
            var schema = await JsonSchema.FromJsonAsync(json);
            var j = schema.ToJson();

            // Assert
            Assert.Equal(JsonObjectType.Integer, schema.Properties["foo"].ActualTypeSchema.Type);
        }

        [Fact]
        public async Task When_schema_references_external_schema_placed_in_directory_with_sharp_in_name()
        {
            // Arrange
            var path = GetTestDirectory() + "/References/LocalReferencesTests/dir_with_#/first.json";

            // Act
            var schema = await JsonSchema.FromFileAsync(path);
            var json = schema.ToJson();

            // Assert
            Assert.Equal(JsonObjectType.Integer, schema.ActualTypeSchema.Type);
        }

        private string GetTestDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("#", "%23");
            var uri = new UriBuilder(codeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }
    }
}
