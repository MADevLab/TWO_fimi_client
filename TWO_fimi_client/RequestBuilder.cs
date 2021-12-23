using System;
using System.Configuration;
using System.IO;
using System.Globalization;

namespace TWO_fimi_client
{
    class RequestBuilder:Logging
    {
        WebTools webTools = new WebTools();
        Serializer serializer = new Serializer();
        public object GenerateRequest(string pOperationName, int pSessionId = 0, string pCryptPassword = null, string pSavePoint = null)
        {
            switch (pOperationName)
            {
                case "InitSessionRq":
                    InitSessionRq1 initSessionRq1 = new InitSessionRq1
                    {
                        Request = new InitSessionRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            NeedDicts = Convert.ToByte(DefaultValuesList.NeedDicts),
                            NeedDictsSpecified = Convert.ToBoolean(Convert.ToByte(DefaultValuesList.NeedDictsSpecified)),
                            AllVendorsSpecified = Convert.ToBoolean(Convert.ToByte(DefaultValuesList.AllVendorsSpecified)),
                            AvoidSessionSpecified = Convert.ToBoolean(Convert.ToByte(DefaultValuesList.AvoidSessionSpecified))
                        }
                    };
                    InitSessionRp1 initSessionRp1 = new InitSessionRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(initSessionRq1, typeof(InitSessionRq1)))),
                        initSessionRp1, typeof(InitSessionRp1));
                case "LogonRq":
                    LogonRq1 logonRq1 = new LogonRq1
                    {
                        Request = new LogonRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            SessionSpecified = true,
                            Session = pSessionId,
                            Password = pCryptPassword
                        }
                    };
                    LogonRp1 logonRp1 = new LogonRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(logonRq1, typeof(LogonRq1)))),
                        logonRp1, typeof(LogonRp1)); 
                case "LogoffRq":
                    LogoffRq1 logoffRq1 = new LogoffRq1                    
                    {
                        Request = new LogoffRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            SessionSpecified = true,
                            Session = pSessionId/*,
                            Password = pCryptPassword*/
                        }
                    };
                    LogoffRp1 logoffRp1 = new LogoffRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(logoffRq1, typeof(LogoffRq1)))),
                        logoffRp1, typeof(LogoffRp1));
                case "BeginTransactionRq":
                    BeginTransactionRq1 beginTransactionRq1 = new BeginTransactionRq1
                    {
                        Request = new BeginTransactionRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            SessionSpecified = true,
                            Session = pSessionId,
                            Password = pCryptPassword,
                            Savepoint = pSavePoint,
                            Force = Convert.ToByte(DefaultValuesList.Force)
                        }
                    };
                    BeginTransactionRp1 beginTransactionRp1 = new BeginTransactionRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(beginTransactionRq1, typeof(BeginTransactionRq1)))),
                        beginTransactionRp1, typeof(BeginTransactionRp1));
                case "Commit":
                    CommitRq1 commitRq1 = new CommitRq1
                    {
                        Request = new CommitRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            SessionSpecified = true,
                            Session = pSessionId,
                            Password = pCryptPassword
                        }
                    };
                    CommitRp1 commitRp1 = new CommitRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(commitRq1, typeof(CommitRq1)))),
                        commitRp1, typeof(CommitRp1));
                case "Rollback":
                    RollBackRq1 rollBackRq1 = new RollBackRq1
                    {
                        Request = new RollBackRq
                        {
                            Clerk = DefaultValuesList.Clerk,
                            Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture),
                            SessionSpecified = true,
                            Session = pSessionId,
                            Password = pCryptPassword,
                            Savepoint = pSavePoint
                        }
                    };                    
                    RollBackRp1 rollBackRp1 = new RollBackRp1();
                    return serializer.Deserialize(webTools.SendRequest(serializer.Wrap2SOAP(serializer.Serialize(rollBackRq1, typeof(RollBackRq1)))),
                        rollBackRp1, typeof(RollBackRp1));                
                default:
                    return string.Empty;
            }
        }
        

        public string GetReqFromFile(string pOperName)
        {
            string reqFileName = String.Empty;
            string path = DefaultValuesList.WorkingDirectory;
            try
            {
                EnterFileName:
                Console.Write(String.Format("Please enter a filename for the '{0}' request (Ex. {0}.xml)\nor press the [Q] key to return to the list of operations: ", pOperName));
                reqFileName = Console.ReadLine();

                switch (reqFileName.ToUpper())
                {
                    case "Q":
                        return String.Empty;                        
                    default:
                        Console.WriteLine($"\n{"[{0}]",-7}Checking file exists...: '{{1}}'...",
                                            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), path + reqFileName);

                        if (File.Exists(path + reqFileName))
                        {
                            Console.WriteLine($"{"[{0}]",-7}StatusDescription: [OK]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                            Console.WriteLine($"{"[{0}]",-7}Loading request of operation type '{{1}}' from file: '{{2}}'...",
                                            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), pOperName, reqFileName);
                            using (var sr = new StreamReader(path + reqFileName))
                            {
                                return webTools.TextFormater(sr.ReadToEnd());
                            }
                        }
                        SetColor("Could not find a part of the request file path...", ConsoleColor.Red);
                        goto EnterFileName;
                }
            }
            catch (IOException ioe)
            {
                PrintExceptionEvent(GetType().FullName, ioe.GetType().Name, ioe.Message);
                new MainProcess().Processing();
            }

            return String.Empty;
        }
    }
}
