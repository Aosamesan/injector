using System;
using System.Text;

namespace Injector.Helpers
{
    public static class NamingHelper
    {
        public static string ConvertToCamelCase(string name)
        {
            var stringBuilder = new StringBuilder();
            var capitalizeFlag = false;

            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    stringBuilder.Append(capitalizeFlag ? char.ToUpper(ch) : char.ToLower(ch));
                }
                capitalizeFlag = !char.IsLetter(ch);
            }

            return stringBuilder.ToString();
        }
    }
}