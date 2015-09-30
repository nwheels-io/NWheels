using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class UriExtensions
    {
        public static Uri EnsureTrailingSlash(this Uri uri)
        {
            var uriString = uri.ToString();

            if ( uriString.EndsWith("/") )
            {
                return uri;
            }

            return new Uri(uriString + "/");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string EnsureTrailingSlash(this string url)
        {
            if ( url.EndsWith("/") )
            {
                return url;
            }

            return url + "/";
        }
    }
}
