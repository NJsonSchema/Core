//-----------------------------------------------------------------------
// <copyright file="DateTimeFormatValidator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace NJsonSchema.Validation.FormatValidators
{
    /// <summary>Validator for DateTime format.</summary>
    public class DateTimeFormatValidator : IFormatValidator
    {
        private readonly string[] _acceptableFormats =
        [
            "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
            "yyyy-MM-dd' 'HH:mm:ss.FFFFFFFK",
            "yyyy-MM-dd'T'HH:mm:ssK",
            "yyyy-MM-dd' 'HH:mm:ssK",
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd' 'HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm",
            "yyyy-MM-dd' 'HH:mm",
            "yyyy-MM-dd'T'HH",
            "yyyy-MM-dd' 'HH",
            "yyyy-MM-dd",
            "yyyy-MM-dd'Z'",
            "yyyyMMdd",
            "yyyy-MM",
            "yyyy"
        ];

        /// <summary>Gets the format attribute's value.</summary>
        public string Format { get; } = JsonFormatStrings.DateTime;

        /// <summary>Gets the validation error kind.</summary>
        public ValidationErrorKind ValidationErrorKind { get; } = ValidationErrorKind.DateTimeExpected;

        /// <summary>Validates if a string is valid DateTime.</summary>
        /// <param name="value">String value.</param>
        /// <param name="tokenType">Type of token holding the value.</param>
        /// <returns></returns>
        public bool IsValid(string value, JTokenType tokenType)
        {
            return tokenType == JTokenType.Date 
                || DateTimeOffset.TryParseExact(value, _acceptableFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeResult);
        }
    }
}
