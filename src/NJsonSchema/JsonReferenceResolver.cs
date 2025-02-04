//-----------------------------------------------------------------------
// <copyright file="JsonReferenceResolver.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema.Infrastructure;
using NJsonSchema.References;

namespace NJsonSchema
{
    /// <summary>Resolves JSON Pointer references.</summary>
    public class JsonReferenceResolver
    {
        private readonly JsonSchemaAppender _schemaAppender;
        private readonly Dictionary<string, IJsonReference> _resolvedObjects = [];

        /// <summary>Initializes a new instance of the <see cref="JsonReferenceResolver"/> class.</summary>
        /// <param name="schemaAppender">The schema appender.</param>
        public JsonReferenceResolver(JsonSchemaAppender schemaAppender)
        {
            _schemaAppender = schemaAppender;
        }

        /// <summary>Creates the factory to be used in the FromJsonAsync method.</summary>
        /// <param name="typeNameGenerator">The type name generator.</param>
        /// <returns>The factory.</returns>
        public static Func<JsonSchema, JsonReferenceResolver> CreateJsonReferenceResolverFactory(ITypeNameGenerator typeNameGenerator)
        {
            JsonReferenceResolver ReferenceResolverFactory(JsonSchema schema)
            {
                return new JsonReferenceResolver(new JsonSchemaAppender(schema, typeNameGenerator));
            }

            return ReferenceResolverFactory;
        }

        /// <summary>Adds a document reference.</summary>
        /// <param name="documentPath">The document path.</param>
        /// <param name="schema">The referenced schema.</param>
        public void AddDocumentReference(string documentPath, IJsonReference schema)
        {
            _resolvedObjects[documentPath.Contains("://") ? documentPath : Path.GetFullPath(documentPath)] = schema;
        }

        /// <summary>Gets the object from the given JSON path.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="jsonPath">The JSON path.</param>
        /// <param name="targetType">The target type to resolve.</param>
        /// <param name="contractResolver">The contract resolver.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The JSON Schema or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not resolve the JSON path.</exception>
        /// <exception cref="NotSupportedException">Could not resolve the JSON path.</exception>
        public async Task<IJsonReference> ResolveReferenceAsync(object rootObject, string jsonPath, Type targetType,
                IContractResolver contractResolver, CancellationToken cancellationToken = default)
        {
            return await ResolveReferenceAsync(rootObject, jsonPath, targetType, contractResolver, true, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Gets the object from the given JSON path.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="jsonPath">The JSON path.</param>
        /// <param name="targetType">The target type to resolve.</param>
        /// <param name="contractResolver">The contract resolver.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The JSON Schema or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not resolve the JSON path.</exception>
        /// <exception cref="NotSupportedException">Could not resolve the JSON path.</exception>
        public async Task<IJsonReference> ResolveReferenceWithoutAppendAsync(object rootObject, string jsonPath, Type targetType,
                IContractResolver contractResolver, CancellationToken cancellationToken = default)
        {
            return await ResolveReferenceAsync(rootObject, jsonPath, targetType, contractResolver, false, cancellationToken).ConfigureAwait(false);
        }

        private static string UnescapeReferenceSegment(string segment)
        {
            var urlDecoded = WebUtility.UrlDecode(segment);
            return urlDecoded.Replace("~1", "/").Replace("~0", "~");
        }

        /// <summary>Resolves a document reference.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="jsonPath">The JSON path to resolve.</param>
        /// <param name="targetType">The target type to resolve.</param>
        /// <param name="contractResolver">The contract resolver.</param>
        /// <returns>The resolved JSON Schema.</returns>
        /// <exception cref="InvalidOperationException">Could not resolve the JSON path.</exception>
        public virtual IJsonReference ResolveDocumentReference(object rootObject, string jsonPath, Type targetType, IContractResolver contractResolver)
        {
            var allSegments = jsonPath.Split('/').Skip(1).ToList();
            for (var i = 0; i < allSegments.Count; i++)
            {
                allSegments[i] = UnescapeReferenceSegment(allSegments[i]);
            }

            var schema = ResolveDocumentReference(rootObject, allSegments, targetType, contractResolver, [])
                         ?? throw new InvalidOperationException($"Could not resolve the path '{jsonPath}'.");

            return schema;
        }

        /// <summary>Resolves a file reference.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The resolved JSON Schema.</returns>
        /// <exception cref="NotSupportedException">The System.IO.File API is not available on this platform.</exception>
        public virtual async Task<IJsonReference> ResolveFileReferenceAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromFileAsync(filePath, schema => this, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Resolves an URL reference.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="NotSupportedException">The HttpClient.GetAsync API is not available on this platform.</exception>
        public virtual async Task<IJsonReference> ResolveUrlReferenceAsync(string url, CancellationToken cancellationToken = default)
        {
            return await JsonSchema.FromUrlAsync(url, schema => this, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IJsonReference> ResolveReferenceAsync(object rootObject, string jsonPath, Type targetType, IContractResolver contractResolver, bool append, CancellationToken cancellationToken = default)
        {
            if (jsonPath == "#")
            {
                if (rootObject is IJsonReference)
                {
                    return (IJsonReference)rootObject;
                }

                throw new InvalidOperationException("Could not resolve the JSON path '#' because the root object is not a JsonSchema4.");
            }
            else if (jsonPath.StartsWith("#/", StringComparison.Ordinal))
            {
                return ResolveDocumentReference(rootObject, jsonPath, targetType, contractResolver);
            }
            else if (jsonPath.StartsWith("http://", StringComparison.Ordinal) || jsonPath.StartsWith("https://", StringComparison.Ordinal))
            {
                return await ResolveUrlReferenceWithAlreadyResolvedCheckAsync(jsonPath, jsonPath, targetType, contractResolver, append, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var documentPathProvider = rootObject as IDocumentPathProvider;

                var documentPath = documentPathProvider?.DocumentPath;
                if (documentPath != null)
                {
                    if (documentPath.StartsWith("http://", StringComparison.Ordinal) || documentPath.StartsWith("https://", StringComparison.Ordinal))
                    {
                        var url = new Uri(new Uri(documentPath), jsonPath).ToString();
                        return await ResolveUrlReferenceWithAlreadyResolvedCheckAsync(url, jsonPath, targetType, contractResolver, append, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // Split the file path and fragment before concatenating with
                        // document path. If document path have '#' in it, doing this
                        // later would not work.
                        var filePath = ResolveFilePath(documentPath, jsonPath);
                        return await ResolveFileReferenceWithAlreadyResolvedCheckAsync(filePath, targetType, contractResolver, jsonPath, append, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new NotSupportedException("Could not resolve the JSON path '" + jsonPath + "' because no document path is available.");
                }
            }
        }

        /// <summary>Resolves file path.</summary>
        /// <param name="documentPath">The document path.</param>
        /// <param name="jsonPath">The JSON path</param>
        public virtual string ResolveFilePath(string documentPath, string jsonPath)
        {
            var arr = Regex.Split(jsonPath, @"(?=#)");
            return Path.Combine(Path.GetDirectoryName(documentPath)!, arr[0]);
        }

        private async Task<IJsonReference> ResolveFileReferenceWithAlreadyResolvedCheckAsync(string filePath, Type targetType, IContractResolver contractResolver, string jsonPath, bool append, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = Path.GetFullPath(filePath);
                var arr = Regex.Split(jsonPath, @"(?=#)");

                fullPath = DynamicApis.HandleSubdirectoryRelativeReferences(fullPath, jsonPath);

                if (!_resolvedObjects.TryGetValue(fullPath, out IJsonReference? value))
                {
                    value = await ResolveFileReferenceAsync(fullPath, cancellationToken).ConfigureAwait(false);
                    value.DocumentPath = arr[0];
                    _resolvedObjects[fullPath] = value;
                }

                var referencedFile = value;
                var resolvedSchema = arr.Length == 1 ? referencedFile : await ResolveReferenceAsync(referencedFile, arr[1], targetType, contractResolver, cancellationToken).ConfigureAwait(false);
                if (resolvedSchema is JsonSchema && append &&
                    (_schemaAppender.RootObject as JsonSchema)?.Definitions.Values.Contains(referencedFile) != true)
                {
                    var key = jsonPath.Split('/', '\\').Last().Split('.').First();
                    _schemaAppender.AppendSchema((JsonSchema)resolvedSchema, key);
                }

                return resolvedSchema;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Could not resolve the JSON path '" + jsonPath + "' within the file path '" + filePath + "'.", exception);
            }
        }

        private async Task<IJsonReference> ResolveUrlReferenceWithAlreadyResolvedCheckAsync(string fullJsonPath, string jsonPath, Type targetType, IContractResolver contractResolver, bool append, CancellationToken cancellationToken)
        {
            try
            {
                var arr = fullJsonPath.Split('#');
                if (!_resolvedObjects.TryGetValue(arr[0], out IJsonReference? value))
                {
                    var schema = await ResolveUrlReferenceAsync(arr[0], cancellationToken).ConfigureAwait(false);
                    schema.DocumentPath = arr[0];
                    if (schema is JsonSchema && append)
                    {
                        _schemaAppender.AppendSchema((JsonSchema)schema, null);
                    }

                    value = schema;
                    _resolvedObjects[arr[0]] = value;
                }

                var result = value;
                return arr.Length == 1 ? result : await ResolveReferenceAsync(result, "#" + arr[1], targetType, contractResolver, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Could not resolve the JSON path '" + jsonPath + "' with the full JSON path '" + fullJsonPath + "'.", exception);
            }
        }

        private IJsonReference? ResolveDocumentReference(object obj, List<string> segments, Type targetType, IContractResolver contractResolver, HashSet<object> checkedObjects)
        {
            if (obj == null || obj is string || checkedObjects.Contains(obj))
            {
                return null;
            }

            if (obj is IJsonReference reference && reference.Reference != null)
            {
                var result = ResolveDocumentReferenceWithoutDereferencing(reference.Reference, segments, targetType, contractResolver, checkedObjects);
                if (result == null)
                {
                    return ResolveDocumentReferenceWithoutDereferencing(obj, segments, targetType, contractResolver, checkedObjects);
                }
                else
                {
                    return result;
                }
            }

            return ResolveDocumentReferenceWithoutDereferencing(obj, segments, targetType, contractResolver, checkedObjects);
        }

        private IJsonReference? ResolveDocumentReferenceWithoutDereferencing(object obj, List<string> segments, Type targetType, IContractResolver contractResolver, HashSet<object> checkedObjects)
        {
            if (segments.Count == 0)
            {
                if (obj is IDictionary)
                {
                    var settings = new JsonSerializerSettings { ContractResolver = contractResolver };
                    var json = JsonConvert.SerializeObject(obj, settings);
                    return JsonConvert.DeserializeObject(json, targetType, settings) as IJsonReference;
                }
                else
                {
                    return obj as IJsonReference;
                }
            }

            checkedObjects.Add(obj);
            var firstSegment = segments[0];

            if (obj is IDictionary dictionary)
            {
                if (dictionary.Contains(firstSegment))
                {
                    return ResolveDocumentReference(dictionary[firstSegment]!, segments.Skip(1).ToList(), targetType, contractResolver, checkedObjects);
                }
            }
            else if (obj is IEnumerable)
            {
                if (int.TryParse(firstSegment, out var index))
                {
                    var enumerable = ((IEnumerable)obj).Cast<object>().ToArray();
                    if (enumerable.Length > index)
                    {
                        return ResolveDocumentReference(enumerable[index], segments.Skip(1).ToList(), targetType, contractResolver, checkedObjects);
                    }
                }
            }
            else
            {
                var extensionObj = obj as IJsonExtensionObject;
                if (extensionObj?.ExtensionData?.ContainsKey(firstSegment) == true)
                {
                    return ResolveDocumentReference(extensionObj.ExtensionData[firstSegment]!, segments.Skip(1).ToList(), targetType, contractResolver, checkedObjects);
                }

                foreach (var member in obj
                    .GetType()
                    .GetContextualAccessors()
                    .Where(p => !p.IsAttributeDefined<JsonIgnoreAttribute>(true)))
                {
                    var pathSegment = member.GetName();
                    if (pathSegment == firstSegment)
                    {
                        var value = member.GetValue(obj);
                        if (value != null)
                        {
                            return ResolveDocumentReference(value, segments.Skip(1).ToList(), targetType, contractResolver, checkedObjects);
                        }
                    }
                }
            }

            return null;
        }
    }
}