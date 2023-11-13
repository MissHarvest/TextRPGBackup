using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPG
{
    static public  class Utility
    {
        public static string MatchCharacterLength(string text, int length, int offset = 0)
        {
            int len = Encoding.Default.GetBytes(text).Length;
            int pad = length - len / 3 * 2 - len % 3;

            StringBuilder sb = new StringBuilder(text);
            sb.Append("".PadRight(pad - offset));
            return sb.ToString();
        }
    }
}
