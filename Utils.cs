using System;
using System.IO;
using System.Linq;
using System.Text;

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
                
                Encoding encoding = null;
                if (bom.Length >= 4)
                {
                    byte b0 = bom[0];
                    byte b1 = bom[1];
                    byte b2 = bom[2];
                    byte b3 = bom[3];
                    if (b0 == 0xef && b1 == 0xbb && b2 == 0xbf)
                        encoding = Encoding.UTF8;
                    else if (b0 == 0xff && b1 == 0xfe)
                        encoding =  Encoding.Unicode;
                    else if (b0 == 0xfe && b1 == 0xff)
                        encoding =  Encoding.BigEndianUnicode;
                    else if (b0 == 0x00 && b1 == 0x00 && b2 == 0xfe && b3 == 0xff)
                        encoding =  Encoding.UTF32;
                }

                if (encoding != null)
                    return encoding;
                
                byte[] buffer = new byte[4096];
                reader.Seek(0, SeekOrigin.Begin);
                int bytesRead = reader.Read(buffer, 0, buffer.Length);

                var win1251 = Encoding.GetEncoding(1251);
                var win1252 = Encoding.GetEncoding(1252);

                string text1251 = win1251.GetString(buffer, 0, bytesRead);
                string text1252 = win1252.GetString(buffer, 0, bytesRead);

                int cyrillicScore = text1251.Count(c => c >= '\u0400' && c <= '\u04FF');
                int latinScore = text1252.Count(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));

                return cyrillicScore > latinScore ? win1251 : win1252;
            }
        }
    }
}