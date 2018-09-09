using System;
using System.Text;

namespace Injector.Helpers
{
    public static class NamingHelper
    {
        // UL => pass, LL => pass, LU => pass, UU => lower, *L => *U
        public static string ConvertToCamelCase(string name)
        {
            var stringBuilder = new StringBuilder();
            bool previousSign = false;
            bool previousUpper = true;

            foreach (char ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    char appendChar = ch;
                    if (previousSign)
                    {
                        appendChar = char.ToUpper(ch);
                    }
                    else
                    {
                        if (previousUpper)
                        {
                            appendChar = char.ToLower(ch);
                        }
                    }

                    stringBuilder.Append(appendChar);
                    previousUpper = char.IsUpper(appendChar);
                    previousSign = false;
                }
                else
                {
                    previousSign = true;
                }
            }

            return stringBuilder.ToString();
        }
    }
}