using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.Converters;
using Xunit;

namespace NJsonSchema.CodeGeneration.Tests.CSharp
{
    public class InheritanceTests
    {
        public class MyContainer
        {
            public EmptyClassInheritingDictionary CustomDictionary { get; set; }
        }

        /// <summary>
        /// Foobar.
        /// </summary>
        public sealed class EmptyClassInheritingDictionary : Dictionary<string, object>
        {
        }

        [Fact]
        public async Task When_empty_class_inherits_from_dictionary_then_allOf_inheritance_still_works()
        {
            //// Arrange
            var schema = JsonSchema.FromType<MyContainer>();
            var data = schema.ToJson();

            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings());

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            var dschema = schema.Definitions["EmptyClassInheritingDictionary"];

            Assert.Equal(0, dschema.AllOf.Count);
            Assert.True(dschema.IsDictionary);
            Assert.Contains("Foobar.", data);
            Assert.Contains("Foobar.", code);

            Assert.DoesNotContain("class CustomDictionary :", code);
            Assert.Contains("public EmptyClassInheritingDictionary CustomDictionary", code);
            Assert.Contains("public partial class EmptyClassInheritingDictionary : System.Collections.Generic.Dictionary<string, object>", code);
        }

        [KnownType(typeof(MyException))]
        [JsonConverter(typeof(JsonInheritanceConverter), "kind")]
        public class ExceptionBase : Exception
        {
            public string Foo { get; set; }
        }

        /// <summary>
        /// Foobar.
        /// </summary>
        public class MyException : ExceptionBase
        {
            public string Bar { get; set; }
        }

        public class ExceptionContainer
        {
            public ExceptionBase Exception { get; set; }
        }

        [Fact]
        public async Task When_class_with_discriminator_has_base_class_then_csharp_is_generated_correctly()
        {
            //// Arrange
            var schema = JsonSchema.FromType<ExceptionContainer>();
            var data = schema.ToJson();

            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("Foobar.", data);
            Assert.Contains("Foobar.", code);

            Assert.Contains("class ExceptionBase : Exception", code);
            Assert.Contains("class MyException : ExceptionBase", code);
        }

        [Fact]
        public async Task When_property_references_any_schema_with_inheritance_then_property_type_is_correct()
        {
            //// Arrange
            var json = @"{
    ""type"": ""object"",
    ""properties"": {
        ""dog"": {
            ""$ref"": ""#/definitions/Dog""
        }
    },
    ""definitions"": {
        ""Pet"": {
            ""type"": ""object"",
            ""properties"": {
                ""name"": {
                    ""type"": ""string""
                }
            }
        },
        ""Dog"": {
            ""title"": ""Dog"",
            ""description"": """",
            ""allOf"": [
                {
                    ""$ref"": ""#/definitions/Pet""
                },
                {
                    ""type"": ""object""
                }
            ]
        }
    }
}";
            var schema = await JsonSchema.FromJsonAsync(json);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Poco });

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("public Dog Dog { get; set; }", code);
        }

        [Fact]
        public async Task When_definitions_inherit_from_root_schema()
        {
            //// Arrange
            var path = GetTestDirectory() + "/References/Animal.json";

            //// Act
            var schema = await JsonSchema.FromFileAsync(path);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings { ClassStyle = CSharpClassStyle.Record });

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("public abstract partial class Animal", code);
            Assert.Contains("public partial class Cat : Animal", code);
            Assert.Contains("public partial class PersianCat : Cat", code);
            Assert.Contains("[JsonInheritanceAttribute(\"Cat\", typeof(Cat))]", code);
            Assert.Contains("[JsonInheritanceAttribute(\"PersianCat\", typeof(PersianCat))]", code);
        }

        [Fact]
        public async Task When_definition_inherits_parameters_from_base_come_first()
        {
            //// Arrange
            var path = GetTestDirectory() + "/References/Car.json";

            //// Act
            var schema = await JsonSchema.FromFileAsync(path);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Record,
                SortConstructorParameters = false,
            });

            //// Act
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("public partial class Car : Vehicle", code);
            Assert.Contains("Vehicle(string @id)", code);
            Assert.Contains("Car(string @id, int @wheels)", code);
            Assert.Contains("base(id)", code);
        }

        private string GetTestDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }
    }
}
