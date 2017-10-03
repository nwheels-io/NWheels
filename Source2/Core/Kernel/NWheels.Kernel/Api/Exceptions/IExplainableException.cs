using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Exceptions
{
    /// <summary>
    /// Designates exceptions that have explanation page on NWheels web site.
    /// </summary>
    public interface IExplainableException
    {
        /// <summary>
        ///     Returns path and query of web page URL that explains this exception.
        /// </summary>
        /// <remarks>
        ///     The path and query part should be in the format:
        ///     type_full_name?reason=error_code&amp;param1=value1&amp;param2=value2
        /// </remarks>
        /// <returns>
        ///     Path and query part of explanation page URL
        /// </returns>
        string ExplanationQuery { get; }
    }
}
