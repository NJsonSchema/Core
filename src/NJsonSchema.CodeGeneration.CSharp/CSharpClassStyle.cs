//-----------------------------------------------------------------------
// <copyright file="CSharpClassStyle.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.ComponentModel;

namespace NJsonSchema.CodeGeneration.CSharp
{
    /// <summary>The CSharp styles.</summary>
    public enum CSharpClassStyle
    {
        /// <summary>Generates POCOs (Plain Old C# Objects).</summary>
        Poco,
        
        /// <summary>Generates classes implementing the <see cref="INotifyPropertyChanged"/> interface.</summary>
        Inpc,

        /// <summary>Generates classes implementing the Prism base class.</summary>
        Prism,

        /// <summary>Generates Records - read only POCOs (Plain Old C# Objects).</summary>
        Record,

        /// <summary>Generates C# Records.</summary>
        CSharpRecord
    }
}