using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class CharExtensions
    {
        public static bool IsEnglishVowel(this char c)
        {
            var lowerCase = Char.ToLower(c);

            if ( lowerCase < 'a' || lowerCase > 'z' )
            {
                return false;
            }

            return ( 
                lowerCase == 'a' || 
                lowerCase == 'e' || 
                lowerCase == 'i' || 
                lowerCase == 'o' || 
                lowerCase == 'u' );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsEnglishConsonant(this char c)
        {
            var lowerCase = Char.ToLower(c);

            if ( lowerCase < 'a' || lowerCase > 'z' )
            {
                return false;
            }

            return (
                lowerCase != 'a' &&
                lowerCase != 'e' &&
                lowerCase != 'i' &&
                lowerCase != 'o' &&
                lowerCase != 'u');
        }
    }
}
