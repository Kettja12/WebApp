using System;

namespace Services.Extenders
{
    public static class StringExtensions
    {
        public static string RandomString(int stringlen)
        {
            Random rand = new Random();
            int randValue;
            string str = "";
            char letter;
            for (int i = 0; i < stringlen; i++)
            {
                randValue = rand.Next(0, 26);
                letter = Convert.ToChar(randValue + 65);
                str = str + letter;
            }
            return str;
        }
    }
}
