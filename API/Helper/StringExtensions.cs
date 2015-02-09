using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security;

namespace API
{

    public static class StringExtensions
    {
        /// <summary>
        /// Returns a Secure string from the source string
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string Source)
        {
            if (string.IsNullOrWhiteSpace(Source))
                return null;
            else
            {
                SecureString Result = new SecureString();
                foreach (char c in Source.ToCharArray())
                    Result.AppendChar(c);
                return Result;
            }
        }
    }
}