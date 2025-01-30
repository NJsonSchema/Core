using Xunit;

namespace NJsonSchema.CodeGeneration.CSharp.Tests
{
    public class ValidationAttributesTests
    {
        [Fact]
        public async Task When_string_property_has_maxlength_then_stringlength_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/string50'
                    }
                },
                'definitions': {
                    'string50': {
                        'type': 'string',
                        'maxLength': 50
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].MaxLength);
            Assert.Equal(50, schema.Properties["value"].ActualSchema.MaxLength);

            Assert.Contains("[System.ComponentModel.DataAnnotations.StringLength(50)]\n" +
                            "        public string Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_string_property_has_minlength_then_stringlength_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/string40'
                    }
                },
                'definitions': {
                    'string40': {
                        'type': 'string',
                        'minLength': 40
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].MinLength);
            Assert.Equal(40, schema.Properties["value"].ActualSchema.MinLength);

            Assert.Contains("[System.ComponentModel.DataAnnotations.StringLength(int.MaxValue, MinimumLength = 40)]\n" +
                            "        public string Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_int_property_has_maximum_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int20'
                    }
                },
                'definitions': {
                    'int20': {
                        'type': 'integer',
                        'format': 'int32',
                        'maximum': 20
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Maximum);
            Assert.Equal(20, schema.Properties["value"].ActualSchema.Maximum);

            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(int.MinValue, 20)]\n" +
                            "        public int Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_int32_property_has_minimum_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int10'
                    }
                },
                'definitions': {
                    'int10': {
                        'type': 'integer',
                        'format': 'int32',
                        'minimum': 10
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(10, schema.Properties["value"].ActualSchema.Minimum);

            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(10, int.MaxValue)]\n" +
                            "        public int Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_int64_property_has_minimum_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int10'
                    }
                },
                'definitions': {
                    'int10': {
                        'type': 'integer',
                        'format': 'int64',
                        'minimum': 10
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(10, schema.Properties["value"].ActualSchema.Minimum);

            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(10L, long.MaxValue)]\n" +
                            "        public long Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_integer_property_has_minimum_and_maximum_that_are_int64_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int10000000000'
                    }
                },
                'definitions': {
                    'int10000000000': {
                        'type': 'integer',
                        'minimum': -10000000000,
                        'maximum': 10000000000,
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(-10000000000m, schema.Properties["value"].ActualSchema.Minimum);
            Assert.Equal(10000000000m, schema.Properties["value"].ActualSchema.Maximum);

            // expect the integer to be converted to an int64
            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(-10000000000L, 10000000000L)]\n" +
                            "        public long Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_integer_property_has_minimum_and_maximum_with_exclusive_true_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int100000000'
                    }
                },
                'definitions': {
                    'int100000000': {
                        'type': 'integer',
                        'minimum': -100000000,
                        'exclusiveMinimum': true,
                        'maximum': 100000000,
                        'exclusiveMaximum': true,
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(-100000000m, schema.Properties["value"].ActualSchema.Minimum);
            Assert.Equal(100000000m, schema.Properties["value"].ActualSchema.Maximum);
            Assert.True(schema.Properties["value"].ActualSchema.IsExclusiveMaximum);
            Assert.True(schema.Properties["value"].ActualSchema.IsExclusiveMinimum);

            // expect the integer to be converted to an int64
            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(-99999999, 99999999)]\n" +
                            "        public int Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_number_property_has_minimum_and_maximum_with_exclusive_true_and_multipleof_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/number'
                    }
                },
                'definitions': {
                    'number': {
                        'type': 'number',
                        'multipleOf': '0.0001',
                        'minimum': -100000000.5,
                        'exclusiveMinimum': true,
                        'maximum': 100000000.5,
                        'exclusiveMaximum': true,
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(-100000000.5m, schema.Properties["value"].ActualSchema.Minimum);
            Assert.Equal(100000000.5m, schema.Properties["value"].ActualSchema.Maximum);
            Assert.True(schema.Properties["value"].ActualSchema.IsExclusiveMaximum);
            Assert.True(schema.Properties["value"].ActualSchema.IsExclusiveMinimum);

            // expect the integer to be converted to an int64
            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(-100000000.4999D, 100000000.4999D)]\n" +
                            "        public double Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_number_property_has_minimum_and_maximum_that_are_double_then_range_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/int10000000000'
                    }
                },
                'definitions': {
                    'int10000000000': {
                        'type': 'number',
                        'minimum': -10000000000,
                        'maximum': 10000000000,
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Minimum);
            Assert.Equal(-10000000000m, schema.Properties["value"].ActualSchema.Minimum);
            Assert.Equal(10000000000m, schema.Properties["value"].ActualSchema.Maximum);

            // expect the integer to be converted to an int64
            Assert.Contains("[System.ComponentModel.DataAnnotations.Range(-10000000000D, 10000000000D)]\n" +
                            "        public double Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_string_property_has_pattern_then_regularexpression_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string regularExpression = "[a-zA-Z0-9]{5,56}";
            const string json = @"{
                        'type': 'object',
                        'required': [ 'value' ],
                        'properties': {
                            'value': {
                                '$ref': '#/definitions/stringPatterned'
                            }
                        },
                'definitions': {
                    'stringPatterned': {
                        'type': 'string',
                        'pattern': '" + regularExpression + @"'
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Null(schema.Properties["value"].Pattern);
            Assert.Equal(regularExpression, schema.Properties["value"].ActualSchema.Pattern);

            Assert.Contains("[System.ComponentModel.DataAnnotations.RegularExpression(@\"" + regularExpression + "\")]\n" +
                            "        public string Value { get; set; }\n", code);
        }

        [Fact]
        public async Task When_array_property_has_maxitems_then_maxlength_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/array10'
                    }
                },
                'definitions': {
                    'array10': {
                        'type': 'array',
                        'items': {
                           'type': 'string'
                        },
                        'maxItems': 10
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
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Equal(0, schema.Properties["value"].MaxItems);
            Assert.Equal(10, schema.Properties["value"].ActualSchema.MaxItems);

            Assert.Contains("[System.ComponentModel.DataAnnotations.MaxLength(10)]\n" +
                            "        public Array10 Value { get; set; } = new Array10();\n", code);
        }

        [Fact]
        public async Task When_array_property_has_minitems_then_minlength_attribute_is_rendered_in_Swagger_mode()
        {
            // Arrange
            const string json = @"{
                'type': 'object',
                'required': [ 'value' ],
                'properties': {
                    'value': {
                        '$ref': '#/definitions/array10'
                    }
                },
                'definitions': {
                    'array10': {
                        'type': 'array',
                        'items': {
                           'type': 'string'
                        },
                        'minItems': 10
                    }
                }
            }";
            var schema = await JsonSchema.FromJsonAsync(json);

            // Act
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
                SchemaType = SchemaType.Swagger2,
                InlineNamedArrays = true
            });
            var code = generator.GenerateFile("Message");

            // Assert
            Assert.Equal(0, schema.Properties["value"].MinItems);
            Assert.Equal(10, schema.Properties["value"].ActualSchema.MinItems);

            Assert.Contains("[System.ComponentModel.DataAnnotations.MinLength(10)]\n" +
                            "        public System.Collections.Generic.ICollection<string> Value { get; set; } = new System.Collections.ObjectModel.Collection<string>();\n", code);
        }
    }
}
