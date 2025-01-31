namespace NJsonSchema
{
    /// <summary>
    /// Settings for generating sample json schema
    /// </summary>
    public class SampleJsonSchemaGeneratorSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to generate optional properties (default: <see cref="SchemaType.JsonSchema"/>).
        /// </summary>
        public SchemaType SchemaType { get; set; } = SchemaType.JsonSchema;
    }
}
