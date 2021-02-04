using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OrganizationPortal
{
    public static class StringExtensions
    {
        /// <summary>
        /// Cut and replace end of string 
        /// </summary>
        /// <param name="charactersCount">Identify characters count for return and format.</param>
        /// <param name="end">String to append in the end of string.</param>
        /// <returns>Return string with replaced end.</returns>
        public static string ReplaceEnd(this string text, int charactersCount, string end = "...")
        {
            if (text.Length <= charactersCount)
                return String.Format("{0}", text);

            return String.Format("{0}{1}", text.Substring(0, charactersCount), end);
        }
    }
}
