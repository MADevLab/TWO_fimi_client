using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWO_fimi_client
{
    class TransactionalOperating:Logging
    {
        private readonly RequestBuilder requestBuilder = new RequestBuilder();

        public void Begin (int pSessionId, string pCryptPassword, string pSavePoint)
        {
            BeginTransactionRp1 beginTransactionRp1 = (BeginTransactionRp1)requestBuilder.GenerateRequest("BeginTransactionRq", pSessionId, pCryptPassword, pSavePoint);
            if (beginTransactionRp1.Response.Response == 1) Console.Write("*Transaction checkpoint: "); SetColor("Successfully", ConsoleColor.Green);
        }
        public void Rollback(int pSessionId, string pCryptPassword, string pSavePoint)
        {
            RollBackRp1 rollBackRp1 = (RollBackRp1)requestBuilder.GenerateRequest("Rollback", pSessionId, pCryptPassword, pSavePoint);            
            if (rollBackRp1.Response.Response == 1) Console.Write(string.Format("*Rollback transaction for SessionId:: '{0}' / SavePoint:: '{1}' : ",
                        pSessionId, pSavePoint)); SetColor("Successfully", ConsoleColor.Green);
        }
        public void Commit (int pSessionId, string pCryptPassword)
        {
            CommitRp1 commitRp1 = (CommitRp1)requestBuilder.GenerateRequest("Commit", pSessionId, pCryptPassword);
            if (commitRp1.Response.Response == 1) Console.Write("*Commit transaction: "); SetColor("Successfully", ConsoleColor.Green);
        }
    }
}
