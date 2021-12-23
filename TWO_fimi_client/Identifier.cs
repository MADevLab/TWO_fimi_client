using System;

namespace TWO_fimi_client
{
    class Identifier:Logging
    {
        private string savePoint = string.Empty;
        private readonly Random random = new Random();        

        private string GetRandomASCII(int pRndDigit)
        {
            string rndASCII = string.Empty;
            if (pRndDigit == 1) rndASCII = Convert.ToChar(random.Next(48, 57)/*0-9*/).ToString();
            if (pRndDigit == 2) rndASCII = Convert.ToChar(random.Next(65, 90)/*A-Z*/).ToString();
            if (pRndDigit == 3) rndASCII = Convert.ToChar(random.Next(97, 122)/*a-z*/).ToString();
            return rndASCII;
        }

        public string GetOracleId ()
        {
            savePoint += GetRandomASCII(random.Next(2,4)) + "_";
            for (int i = 1; i <= 28; i++)
            {
                savePoint += GetRandomASCII(random.Next(1, 4));
            }
            Console.WriteLine($"\n{"[{0}]",-7}Calculating the Oracle Identifier...",
                                        DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            Console.WriteLine($"{"[{0}]",-7}StatusDescription: [OK]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            Console.Write("\n*OracleID: ");
            SetColor(savePoint, ConsoleColor.Green);
            return savePoint;
        }
    }
}