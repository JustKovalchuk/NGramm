using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace NGramm
{
    public class Utils
    {
        public static bool IsVariableChar(char ch)
        {
            return ch == '_' || char.IsLetterOrDigit(ch);
        }
        
        public static bool IsDigit(char ch)
        {
            return char.IsDigit(ch);
        }
        
        public static Encoding GetEncoding(string filename)
        {
            using (var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] bom = new byte[4];
                reader.Read(bom, 0, 4);
                
                if (bom.Length >= 4)
                {
                    byte b0 = bom[0];
                    byte b1 = bom[1];
                    byte b2 = bom[2];
                    byte b3 = bom[3];

                    if (b0 == 0xef && b1 == 0xbb && b2 == 0xbf)
                        return Encoding.UTF8;
                    else if (b0 == 0xff && b1 == 0xfe)
                        return Encoding.Unicode;
                    else if (b0 == 0xfe && b1 == 0xff)
                        return Encoding.BigEndianUnicode;
                    else if (b0 == 0x00 && b1 == 0x00 && b2 == 0xfe && b3 == 0xff)
                        return Encoding.UTF32;
                }

                return Encoding.UTF8;
            }
        }
    }
}