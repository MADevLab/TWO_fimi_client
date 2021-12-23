using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWO_fimi_client
{
    class Logging
    {
        public void ResultPrint(bool success)
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[OK]\n");
                //WriteLog("[OK]");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[FAILED]\n");
                Console.ResetColor();
            }
        }
        public void PrintExceptionEvent(string pClassName, string pExceptionType, string pMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nWarning! Exception of class: {0}\nExceptionType: {1}\nMessage: {2}\n", pClassName, pExceptionType, pMessage);
            Console.ResetColor();
        }

        public void SetColor (string pMessage, ConsoleColor pCololorIndex)
        {
            Console.ForegroundColor = pCololorIndex;
            Console.WriteLine(pMessage);
            Console.ResetColor();
        }
    }
}
