//-----------------------------------------------------------------------
// <copyright file="JsonPathUtilities.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace NJsonSchema
{
    /// <summary>Utilities to work with JSON paths.</summary>
    public static class JsonPathUtilities
    {
        /// <summary>Gets the JSON path of the given object.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="searchedObject">The object to search.</param>
        /// <returns>The path or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not find the JSON path of a child object.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rootObject"/> is <see langword="null"/></exception>
        public static string? GetJsonPath(object rootObject, object searchedObject)
        {
            // TODO: Remove this overload?
            return GetJsonPath(rootObject, searchedObject, new DefaultContractResolver());
        }

        /// <summary>Gets the JSON path of the given object.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="searchedObject">The object to search.</param>
        /// <param name="contractResolver">The contract resolver.</param>
        /// <returns>The path or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not find the JSON path of a child object.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rootObject"/> is <see langword="null"/></exception>
        public static string? GetJsonPath(object rootObject, object searchedObject, IContractResolver contractResolver)
        {
            return GetJsonPaths(rootObject, [searchedObject], contractResolver)[searchedObject];
        }

        /// <summary>Gets the JSON path of the given object.</summary>
        /// <param name="rootObject">The root object.</param>
        /// <param name="searchedObjects">The objects to search.</param>
        /// <param name="contractResolver">The contract resolver.</param>
        /// <returns>The path or <c>null</c> when the object could not be found.</returns>
        /// <exception cref="InvalidOperationException">Could not find the JSON path of a child object.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="rootObject"/> is <see langword="null"/></exception>
        public static IReadOnlyDictionary<object, string?> GetJsonPaths(object rootObject,
            IEnumerable<object> searchedObjects, IContractResolver contractResolver)
        {
            if (rootObject == null)
            {
                throw new ArgumentNullException(nameof(rootObject));
            }

            var mappings = searchedObjects.ToDictionary(o => o, o => (string?)null);
            FindJsonPaths(rootObject, mappings, "#", [], contractResolver);

            if (mappings.Any(p => p.Value == null))
            {
                var errorItems = mappings.Where(p => p.Value == null).Select(x => x.Key.GetType().FullName);
                throw new InvalidOperationException("Could not find the JSON path of a referenced schema: " +
                                                     string.Join(",", errorItems) +
                                                    ". Manually referenced schemas must be added to the " +
                                                    "'Definitions' of a parent schema.");
            }

            return mappings;
        }

        private static bool FindJsonPaths(object obj, Dictionary<object, string?> searchedObjects,
            string basePath, HashSet<object> checkedObjects, IContractResolver contractResolver)
        {
            if (obj == null)
            {
                return false;
            }

            var type = obj.GetType();
            if (type == typeof(string)
                || type.IsPrimitive
                || type.IsEnum
                || type == typeof(JValue)
                || checkedObjects.Contains(obj)
            )
            {
                // no need to inspect
                return false;
            }

            if (searchedObjects.ContainsKey(obj))
            {
                searchedObjects[obj] = basePath;
                if (searchedObjects.All(static p => p.Value != null))
                {
                    return true;
                }
            }

            checkedObjects.Add(obj);

            var pathAndSeparator = basePath + "/";
            if (obj is IDictionary dictionary)
            {
                foreach (DictionaryEntry pair in dictionary)
                {
                    if (pair.Value != null && 
                        FindJsonPaths(pair.Value, searchedObjects, pathAndSeparator + pair.Key, checkedObjects, contractResolver))
                    {
                        return true;
                    }
                }
            }
            else if (obj is IList list)
            {
                for (var i = 0; i < list.Count; ++i)
                {
                    var item = list[i];
                    if (item != null && 
                        FindJsonPaths(item, searchedObjects, pathAndSeparator + i, checkedObjects, contractResolver))
                    {
                        return true;
                    }
                }
            }
            else if (obj is IEnumerable enumerable)
            {
                var i = 0;
                foreach (var item in enumerable)
                {
                    if (item != null &&
                        FindJsonPaths(item, searchedObjects, pathAndSeparator + i, checkedObjects, contractResolver))
                    {
                        return true;
                    }
                    i++;
                }
            }
            else
            {
                if (contractResolver.ResolveContract(type) is JsonObjectContract contract)
                {
                    foreach (var jsonProperty in contract.Properties)
                    {
                        if (jsonProperty.Ignored)
                        {
                            continue;
                        }

                        var value = jsonProperty.ValueProvider?.GetValue(obj);
                        if (value != null &&
                            FindJsonPaths(value, searchedObjects, pathAndSeparator + jsonProperty.PropertyName, checkedObjects, contractResolver))
                        {
                            return true;
                        }
                    }

                    if (obj is IJsonExtensionObject)
                    {
                        var extensionDataProperty = type.GetRuntimeProperty(nameof(IJsonExtensionObject.ExtensionData));
                        if (extensionDataProperty != null)
                        {
                            var value = extensionDataProperty.GetValue(obj);
                            if (value != null && 
                                FindJsonPaths(value, searchedObjects, basePath, checkedObjects, contractResolver))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
