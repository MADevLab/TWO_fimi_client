using System;
using System.Collections.Generic;
using System.Globalization;

namespace TWO_fimi_client
{  
    class MainProcess:Logging
    {
        private string reqFromFile = string.Empty;
        private string nextChallenge;
        public static int SessionId { get; private set; }
        public static string CryptPass { get; private set; }
        public static string SavePoint { get; private set; }

        private readonly RequestBuilder requestBuilder = new RequestBuilder();
        private readonly Serializer serializer = new Serializer();        
        private readonly Cryptographic cryptographic = new Cryptographic();
        private readonly TransactionalOperating transactionalMode = new TransactionalOperating();

        readonly List<string> OperationList = new List<string>()
        {   
            //Заменить на список ENUM!
            {"\n\nOperation item list:\n"},
            {"* I  - Session initialization request (InitSessionRq)\n"},
            {"* 1  - Send '505 -  Get Card Statement (GetCardStatementRq)'  request from file"},
            {"* 2  - Send '511  - Get Card Information (GetCardInfoRq)'  request from file"},
            {"* 3  - Send '514  - Set Card Limits (SetCardLimitsRq)'  request from file"},
            {"* 4  - Send '525  - Create account (CreateAccountRq)'  request from file"},
            {"* 5  - Send '528  - Account Debit  (AccDebitRq)'  request from file"},
            {"* 6  - Send '529  - Account Credit (AccCreditRq)' request from file"},
            {"* 7  - Send '536  - Create Virtual Card (CreateVCardRq)' request from file"},
            {"* 8  - Send '550  - POS Request (POSRequestRq)' request from file"},            
            {"* 9  - Send '564  - Get CVV (GetCVVRq)' request from file"},
            {"* 10 - Send '571  - Get Transactions Information (GetTransInfoRq)' request from file"},
            {"* 11 - Send '1502 - Create Person (CreatePersonRq)' request from file"},
            {"* 12 - Send '1536 - Create Issuer Objects (CreateIssuerObjectsRq)' request from file"},
            {"* 13 - Send '1542 - Update Person (UpdatePersonRq)' request from file"},
            {"* 14 - Send '1569 - Delete Card (DeleteCardRq)' request from file"},
            {"* 15 - Send '1571 - Delete Account (DeleteAccountRq)' request from file"},
            {"* 16 - Send '1572 - Delete Person (DeletePersonRq)' request from file"},
            {"* 17 - Send '1577 - Add Card CMS Abonent (AddCMSAbonentRq)' request from file"},
            {"* 18 - Send '1580 - Set Card 3-D Secure Authentication (SetCard3DSecureAuthenticationRq)' request from file\n"},
            {"* L  - Logoff transaction (LogoffRq)\n"},
            {"* B  - Begin transaction (BeginTransactionRq)"},
            {"* C  - Commit transaction (CommitRq)"},
            {"* R  - RollBack transaction (RollBackRq)"},
            {"* 0  - Exit"}
        };

        public MainProcess()
        {
            SessionId = 0;
            CryptPass = string.Empty;
            SavePoint = string.Empty;
        }

        private void GetOperationList()
        {
            if ((string.IsNullOrEmpty(SessionId.ToString()))||(string.IsNullOrEmpty(nextChallenge))||(string.IsNullOrEmpty(CryptPass)))
            {
                SetColor(string.Format("\n\n\nWarning! Session initialization required (InitSessionRq)!\nSessionId: {0}\nnextChallenge: {1}\nCryptPassword: {2}\n",
                    SessionId.ToString(), nextChallenge, CryptPass), ConsoleColor.Yellow);
            }
            foreach (string menuLine in OperationList)
            {
                Console.WriteLine(menuLine);
            }
        }
        
        public void Processing()
        {
            try
            {
            SelectOperationPoint:
                GetOperationList();
                Console.Write("\nSelect the key for the action from item list: ");
                switch (Console.ReadLine().ToUpper())
                {
                    case "0"://Exit
                        Environment.Exit(0);
                        break;
                    case "I"://InitSessionRq
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'InitSessionRq':",
                            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    InitSessionRp1 initSessionRp = (InitSessionRp1)requestBuilder.GenerateRequest("InitSessionRq");
                    SessionId = initSessionRp.Response.Id;
                    nextChallenge = initSessionRp.Response.NextChallenge;
                    Console.Write("*Session Id: ");
                    SetColor(SessionId.ToString(), ConsoleColor.Green);

                    Console.Write("*Next Challenge: ");
                    SetColor(nextChallenge, ConsoleColor.Green);
                    CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                        nextChallenge);

                Sendlogon:
                        Console.Write("\n\nSend Logon request? [Y/N] ");
                        switch (Console.ReadLine().ToUpper())
                        {
                            case "Y"://LogonRq
                                Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'LogonRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                                LogonRp1 logonRp = (LogonRp1)requestBuilder.GenerateRequest("LogonRq", SessionId, CryptPass);
                                nextChallenge = logonRp.Response.NextChallenge;
                                Console.Write("*Next Challenge: ");
                                SetColor(nextChallenge, ConsoleColor.Green);
                                CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                                    nextChallenge);                                

                                goto SelectOperationPoint;
                            case "N"://OtherRq                                
                                goto SelectOperationPoint;
                            default:
                                SetColor("Key entered is wrong!", ConsoleColor.Red);
                                goto Sendlogon;
                        }
                    case "1":
                        reqFromFile = requestBuilder.GetReqFromFile("GetCardStatementRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        GetCardStatementRq1 getCardStatementRq1 = new GetCardStatementRq1();
                        getCardStatementRq1 = (GetCardStatementRq1)new Serializer().Deserialize(reqFromFile, getCardStatementRq1, typeof(GetCardStatementRq1));
                        getCardStatementRq1.Request.Clerk = DefaultValuesList.Clerk;
                        getCardStatementRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        getCardStatementRq1.Request.SessionSpecified = true;
                        getCardStatementRq1.Request.Session = SessionId;
                        getCardStatementRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'GetCardStatementRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        GetCardStatementRp1 getCardStatementRp1 = new GetCardStatementRp1();
                        getCardStatementRp1 = (GetCardStatementRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(getCardStatementRq1, typeof(GetCardStatementRq1)))),
                                getCardStatementRp1, typeof(GetCardStatementRp1));

                        nextChallenge = getCardStatementRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        //Console.Write("*Type: ");
                        //SetColor(Convert.ToString(getCardInfoRp1.Response.Type), ConsoleColor.Green);
                        //Console.Write("*Card: ");
                        //SetColor(string.Format("{0}-{1}", getCardInfoRp1.Response.FoundPAN, getCardInfoRp1.Response.FoundMBR), ConsoleColor.Green);
                        //Console.Write("*CardUID: ");
                        //SetColor(getCardInfoRp1.Response.CardUID, ConsoleColor.Green);
                        //Console.Write("*Exp. Date: ");
                        //SetColor(getCardInfoRp1.Response.ExpDate.ToString("MM'/'yy"), ConsoleColor.Green);
                        //Console.Write("*Status: ");
                        //SetColor(Convert.ToString(getCardInfoRp1.Response.Status), ConsoleColor.Green);


                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "2":
                        reqFromFile = requestBuilder.GetReqFromFile("GetCardInfoRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        GetCardInfoRq1 getCardInfoRq1 = new GetCardInfoRq1();
                        getCardInfoRq1 = (GetCardInfoRq1)new Serializer().Deserialize(reqFromFile, getCardInfoRq1, typeof(GetCardInfoRq1));
                        getCardInfoRq1.Request.Clerk = DefaultValuesList.Clerk;
                        getCardInfoRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        getCardInfoRq1.Request.SessionSpecified = true;
                        getCardInfoRq1.Request.Session = SessionId;
                        getCardInfoRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'GetCardInfoRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        GetCardInfoRp1 getCardInfoRp1 = new GetCardInfoRp1();
                        getCardInfoRp1 = (GetCardInfoRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(getCardInfoRq1, typeof(GetCardInfoRq1)))),
                                getCardInfoRp1, typeof(GetCardInfoRp1));

                        nextChallenge = getCardInfoRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*Type: ");
                        SetColor(Convert.ToString(getCardInfoRp1.Response.Type), ConsoleColor.Green);
                        Console.Write("*Card: ");
                        SetColor(string.Format("{0}-{1}", getCardInfoRp1.Response.FoundPAN, getCardInfoRp1.Response.FoundMBR), ConsoleColor.Green);
                        Console.Write("*CardUID: ");
                        SetColor(getCardInfoRp1.Response.CardUID, ConsoleColor.Green);
                        Console.Write("*Exp. Date: ");
                        SetColor(getCardInfoRp1.Response.ExpDate.ToString("MM'/'yy"), ConsoleColor.Green);                        
                        Console.Write("*Status: ");
                        SetColor(Convert.ToString(getCardInfoRp1.Response.Status), ConsoleColor.Green);
                        

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "3":
                        reqFromFile = requestBuilder.GetReqFromFile("SetCardLimitsRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        SetCardLimitsRq1 setCardLimitsRq1 = new SetCardLimitsRq1();
                        setCardLimitsRq1 = (SetCardLimitsRq1)new Serializer().Deserialize(reqFromFile, setCardLimitsRq1, typeof(SetCardLimitsRq1));
                        setCardLimitsRq1.Request.Clerk = DefaultValuesList.Clerk;
                        setCardLimitsRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        setCardLimitsRq1.Request.SessionSpecified = true;
                        setCardLimitsRq1.Request.Session = SessionId;
                        setCardLimitsRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'SetCardLimitsRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        SetCardLimitsRp1 setCardLimitsRp1 = new SetCardLimitsRp1();
                        setCardLimitsRp1 = (SetCardLimitsRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(setCardLimitsRq1, typeof(SetCardLimitsRq1)))),
                                setCardLimitsRp1, typeof(SetCardLimitsRp1));

                        nextChallenge = setCardLimitsRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "4":
                        reqFromFile = requestBuilder.GetReqFromFile("CreateAccountRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        CreateAccountRq1 createAccountRq1 = new CreateAccountRq1();
                        createAccountRq1 = (CreateAccountRq1)new Serializer().Deserialize(reqFromFile, createAccountRq1, typeof(CreateAccountRq1));
                        createAccountRq1.Request.Clerk = DefaultValuesList.Clerk;
                        createAccountRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        createAccountRq1.Request.SessionSpecified = true;
                        createAccountRq1.Request.Session = SessionId;
                        createAccountRq1.Request.Password = CryptPass;                        

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'CreateAccountRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        CreateAccountRp1 createAccountRp1 = new CreateAccountRp1();
                        createAccountRp1 = (CreateAccountRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(createAccountRq1, typeof(CreateAccountRq1)))),
                                createAccountRp1, typeof(CreateAccountRp1));                                                        

                        nextChallenge = createAccountRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "5":
                        reqFromFile = requestBuilder.GetReqFromFile("AccDebitRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        AcctDebitRq1 acctDebitRq1 = new AcctDebitRq1();
                        acctDebitRq1 = (AcctDebitRq1)new Serializer().Deserialize(reqFromFile, acctDebitRq1, typeof(AcctDebitRq1));
                        acctDebitRq1.Request.Clerk = DefaultValuesList.Clerk;
                        acctDebitRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        acctDebitRq1.Request.SessionSpecified = true;
                        acctDebitRq1.Request.Session = SessionId;
                        acctDebitRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'AccDebitRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        AcctDebitRp1 acctDebitRp1 = new AcctDebitRp1();
                        acctDebitRp1 = (AcctDebitRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(acctDebitRq1, typeof(AcctDebitRq1)))),
                                acctDebitRp1, typeof(AcctDebitRp1));

                        nextChallenge = acctDebitRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*ApprovalCode: ");
                        SetColor(acctDebitRp1.Response.ApprovalCode, ConsoleColor.Green);
                        Console.Write("*AvailBalance: ");
                        SetColor(acctDebitRp1.Response.AvailBalance.ToString(), ConsoleColor.Green);
                        Console.Write("*LedgerBalance: ");
                        SetColor(acctDebitRp1.Response.LedgerBalance.ToString(), ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "6":
                        reqFromFile = requestBuilder.GetReqFromFile("AccCreditRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;                        

                        AcctCreditRq1 acctCreditRq1 = new AcctCreditRq1();
                        acctCreditRq1 = (AcctCreditRq1)new Serializer().Deserialize(reqFromFile, acctCreditRq1, typeof(AcctCreditRq1));
                        acctCreditRq1.Request.Clerk = DefaultValuesList.Clerk;
                        acctCreditRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        acctCreditRq1.Request.SessionSpecified = true;
                        acctCreditRq1.Request.Session = SessionId;
                        acctCreditRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'AccCreditRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        AcctCreditRp acctCreditRp = new AcctCreditRp();
                        acctCreditRp = (AcctCreditRp)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(acctCreditRq1, typeof(AcctCreditRq1)))),
                                acctCreditRp, typeof(AcctCreditRp));

                        nextChallenge = acctCreditRp.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*ApprovalCode: ");
                        SetColor(acctCreditRp.Response.ApprovalCode, ConsoleColor.Green);
                        Console.Write("*AvailBalance: ");
                        SetColor(acctCreditRp.Response.AvailBalance.ToString(), ConsoleColor.Green);
                        Console.Write("*LedgerBalance: ");
                        SetColor(acctCreditRp.Response.LedgerBalance.ToString(), ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "7":

                        reqFromFile = requestBuilder.GetReqFromFile("CreateVCardRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;
                        
                        CreateVCardRq1 createVCardRq1 = new CreateVCardRq1();
                        createVCardRq1 = (CreateVCardRq1)new Serializer().Deserialize(reqFromFile, createVCardRq1, typeof(CreateVCardRq1));
                        createVCardRq1.Request.Clerk = DefaultValuesList.Clerk;
                        createVCardRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        createVCardRq1.Request.SessionSpecified = true;
                        createVCardRq1.Request.Session = SessionId;
                        createVCardRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'CreateVCardRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));                      
                        CreateVCardRp1 createVCardRp1 = new CreateVCardRp1();
                        createVCardRp1 = (CreateVCardRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(createVCardRq1, typeof(CreateVCardRq1)))),
                                createVCardRp1, typeof(CreateVCardRp1));

                        nextChallenge = createVCardRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*Card: ");
                        SetColor(string.Format("{0}-{1}", createVCardRp1.Response.PAN, createVCardRp1.Response.MBR), ConsoleColor.Green);
                        Console.Write("*CardUID: ");
                        SetColor(createVCardRp1.Response.CardUID, ConsoleColor.Green);
                        Console.Write("*Exp. Date: ");
                        SetColor(createVCardRp1.Response.Expiration.ToString("MM'/'yy"), ConsoleColor.Green);
                        Console.Write("*Account: ");
                        SetColor(createVCardRp1.Response.Account, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "8":
                        reqFromFile = requestBuilder.GetReqFromFile("POSRequestRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        POSRequestRq1 pPOSRequestRq1 = new POSRequestRq1();
                        pPOSRequestRq1 = (POSRequestRq1)new Serializer().Deserialize(reqFromFile, pPOSRequestRq1, typeof(POSRequestRq1));
                        pPOSRequestRq1.Request.Clerk = DefaultValuesList.Clerk;
                        pPOSRequestRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        pPOSRequestRq1.Request.SessionSpecified = true;
                        pPOSRequestRq1.Request.Session = SessionId;
                        pPOSRequestRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'POSRequestRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        POSRequestRp1 pPOSRequestRp1 = new POSRequestRp1();
                        pPOSRequestRp1 = (POSRequestRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(pPOSRequestRq1, typeof(POSRequestRq1)))),
                                pPOSRequestRp1, typeof(POSRequestRp1));

                        nextChallenge = pPOSRequestRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*ApprovalCode: ");
                        SetColor(pPOSRequestRp1.Response.ApprovalCode, ConsoleColor.Green);
                        Console.Write("*AvailBalance: ");
                        SetColor(pPOSRequestRp1.Response.AvailBalance.ToString(), ConsoleColor.Green);
                        Console.Write("*LedgerBalance: ");
                        SetColor(pPOSRequestRp1.Response.LedgerBalance.ToString(), ConsoleColor.Green);
                        Console.Write("*AuthRespCode: ");
                        SetColor(pPOSRequestRp1.Response.AuthRespCode.ToString(), ConsoleColor.Green);
                        

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "9":

                        reqFromFile = requestBuilder.GetReqFromFile("GetCVVRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        GetCVVRq1 getCVVRq1 = new GetCVVRq1();
                        getCVVRq1 = (GetCVVRq1)new Serializer().Deserialize(reqFromFile, getCVVRq1, typeof(GetCVVRq1));
                        getCVVRq1.Request.Clerk = DefaultValuesList.Clerk;
                        getCVVRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        getCVVRq1.Request.SessionSpecified = true;
                        getCVVRq1.Request.Session = SessionId;
                        getCVVRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'GetCVVRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));                        
                        GetCVVRp1 getCVVRp1 = new GetCVVRp1();
                        getCVVRp1 = (GetCVVRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(getCVVRq1, typeof(GetCVVRq1)))),
                            getCVVRp1, typeof(GetCVVRp1));

                        nextChallenge = getCVVRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        if (getCVVRq1.Request.IsCVV2 == 1)
                            Console.Write("*CVV2: ");
                        else
                            Console.Write("*CVV: ");
                        SetColor(getCVVRp1.Response.StrCVV, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "10":
                        reqFromFile = requestBuilder.GetReqFromFile("GetTransInfoRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        GetTransInfoRq1 getTransInfoRq1 = new GetTransInfoRq1();
                        getTransInfoRq1 = (GetTransInfoRq1)new Serializer().Deserialize(reqFromFile, getTransInfoRq1, typeof(GetTransInfoRq1));
                        getTransInfoRq1.Request.Clerk = DefaultValuesList.Clerk;
                        getTransInfoRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        getTransInfoRq1.Request.SessionSpecified = true;
                        getTransInfoRq1.Request.Session = SessionId;
                        getTransInfoRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'GetTransInfoRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        GetTransInfoRp1 getTransInfoRp1 = new GetTransInfoRp1();
                        getTransInfoRp1 = (GetTransInfoRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(getTransInfoRq1, typeof(GetTransInfoRq1)))),
                                getTransInfoRp1, typeof(GetTransInfoRp1));

                        nextChallenge = getTransInfoRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*MaskBalances: ");
                        SetColor(getTransInfoRp1.Response.MaskBalances.ToString(), ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "11":
                        reqFromFile = requestBuilder.GetReqFromFile("CreatePersonRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        CreatePersonRq1 createPersonRq1 = new CreatePersonRq1();
                        createPersonRq1 = (CreatePersonRq1)new Serializer().Deserialize(reqFromFile, createPersonRq1, typeof(CreatePersonRq1));
                        createPersonRq1.Request.Clerk = DefaultValuesList.Clerk;
                        createPersonRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        createPersonRq1.Request.SessionSpecified = true;
                        createPersonRq1.Request.Session = SessionId;
                        createPersonRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'CreatePersonRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        CreatePersonRp1 createPersonRp1 = new CreatePersonRp1();
                        createPersonRp1 = (CreatePersonRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(createPersonRq1, typeof(CreatePersonRq1)))),
                                createPersonRp1, typeof(CreatePersonRp1));

                        nextChallenge = createPersonRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*PersonId: ");
                        SetColor(createPersonRp1.Response.PersonId.ToString(), ConsoleColor.Green);

                        Console.Write("*PersonExtId: ");
                        SetColor(createPersonRp1.Response.PersonExtId, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;                    
                    case "12":
                        reqFromFile = requestBuilder.GetReqFromFile("CreateIssuerObjectsRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        CreateIssuerObjectsRq1 createIssuerObjectsRq1 = new CreateIssuerObjectsRq1();
                        createIssuerObjectsRq1 = (CreateIssuerObjectsRq1)new Serializer().Deserialize(reqFromFile, createIssuerObjectsRq1, typeof(CreateIssuerObjectsRq1));
                        createIssuerObjectsRq1.Request.Clerk = DefaultValuesList.Clerk;
                        createIssuerObjectsRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        createIssuerObjectsRq1.Request.SessionSpecified = true;
                        createIssuerObjectsRq1.Request.Session = SessionId;
                        createIssuerObjectsRq1.Request.Password = CryptPass;
                        
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'CreateIssuerObjectsRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        CreateIssuerObjectsRp1 createIssuerObjectsRp1 = new CreateIssuerObjectsRp1();
                        createIssuerObjectsRp1 = (CreateIssuerObjectsRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(createIssuerObjectsRq1, typeof(CreateIssuerObjectsRq1)))),
                                createIssuerObjectsRp1, typeof(CreateIssuerObjectsRp1));

                        nextChallenge = createIssuerObjectsRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        Console.Write("*ApprovalCode: ");
                        SetColor(createIssuerObjectsRp1.Response.ApprovalCode, ConsoleColor.Green);
                        Console.Write("*CardExpiration: ");
                        SetColor(createIssuerObjectsRp1.Response.CardExpiration.ToString("MM'/'yy"), ConsoleColor.Green);
                        Console.Write("*CardLimitCurrency: ");
                        SetColor(createIssuerObjectsRp1.Response.CardLimitCurrency.ToString(), ConsoleColor.Green);
                        Console.Write("*CardUID: ");
                        SetColor(createIssuerObjectsRp1.Response.CardUID, ConsoleColor.Green);
                        Console.Write("*PAN: ");
                        SetColor(createIssuerObjectsRp1.Response.PAN, ConsoleColor.Green);
                        Console.Write("*MBR: ");
                        SetColor(createIssuerObjectsRp1.Response.MBR.ToString(), ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "13":
                        reqFromFile = requestBuilder.GetReqFromFile("UpdatePersonRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        UpdatePersonRq1 updatePersonRq1 = new UpdatePersonRq1();
                        updatePersonRq1 = (UpdatePersonRq1)new Serializer().Deserialize(reqFromFile, updatePersonRq1, typeof(UpdatePersonRq1));
                        updatePersonRq1.Request.Clerk = DefaultValuesList.Clerk;
                        updatePersonRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        updatePersonRq1.Request.SessionSpecified = true;
                        updatePersonRq1.Request.Session = SessionId;
                        updatePersonRq1.Request.Password = CryptPass;
                        
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'UpdatePersonRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

                        UpdatePersonRp1 updatePersonRp1 = new UpdatePersonRp1();

                        updatePersonRp1 = (UpdatePersonRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(updatePersonRq1, typeof(UpdatePersonRq1)))),
                                updatePersonRp1, typeof(UpdatePersonRp1));

                        nextChallenge = updatePersonRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "14":
                        reqFromFile = requestBuilder.GetReqFromFile("DeleteCardRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        DeleteCardRq1 deleteCardRq1 = new DeleteCardRq1();
                        deleteCardRq1 = (DeleteCardRq1)new Serializer().Deserialize(reqFromFile, deleteCardRq1, typeof(DeleteCardRq1));
                        deleteCardRq1.Request.Clerk = DefaultValuesList.Clerk;
                        deleteCardRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        deleteCardRq1.Request.SessionSpecified = true;
                        deleteCardRq1.Request.Session = SessionId;
                        deleteCardRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'DeleteCardRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

                        DeleteCardRp1 deleteCardRp1 = new DeleteCardRp1();
                        deleteCardRp1 = (DeleteCardRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(deleteCardRq1, typeof(DeleteCardRq1)))),
                                deleteCardRp1, typeof(DeleteCardRp1));

                        nextChallenge = deleteCardRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);
                        
                        goto SelectOperationPoint;
                    case "15":
                        reqFromFile = requestBuilder.GetReqFromFile("DeleteAccountRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        DeleteAccountRq1 deleteAccountRq1 = new DeleteAccountRq1();
                        deleteAccountRq1 = (DeleteAccountRq1)new Serializer().Deserialize(reqFromFile, deleteAccountRq1, typeof(DeleteAccountRq1));
                        deleteAccountRq1.Request.Clerk = DefaultValuesList.Clerk;
                        deleteAccountRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        deleteAccountRq1.Request.SessionSpecified = true;
                        deleteAccountRq1.Request.Session = SessionId;
                        deleteAccountRq1.Request.Password = CryptPass;                        
                        
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'DeleteAccountRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

                        DeleteAccountRp1 deleteAccountRp1 = new DeleteAccountRp1();
                        deleteAccountRp1 = (DeleteAccountRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(deleteAccountRq1, typeof(DeleteAccountRq1)))),
                                deleteAccountRp1, typeof(DeleteAccountRp1));

                        nextChallenge = deleteAccountRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "16":
                        reqFromFile = requestBuilder.GetReqFromFile("DeletePersonRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;
                        
                        DeletePersonRq1 deletePersonRq1 = new DeletePersonRq1();
                        deletePersonRq1 = (DeletePersonRq1)new Serializer().Deserialize(reqFromFile, deletePersonRq1, typeof(DeletePersonRq1));
                        deletePersonRq1.Request.Clerk = DefaultValuesList.Clerk;
                        deletePersonRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        deletePersonRq1.Request.SessionSpecified = true;
                        deletePersonRq1.Request.Session = SessionId;
                        deletePersonRq1.Request.Password = CryptPass;                                                

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'DeletePersonRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        DeletePersonRp1 deletePersonRp1 = new DeletePersonRp1();
                        deletePersonRp1 = (DeletePersonRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(deletePersonRq1, typeof(DeletePersonRq1)))),
                                deletePersonRp1, typeof(DeletePersonRp1));

                        nextChallenge = deletePersonRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;

                    case "17":
                        reqFromFile = requestBuilder.GetReqFromFile("AddCMSAbonentRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        AddCMSAbonentRq1 addCMSAbonentRq1 = new AddCMSAbonentRq1();

                        addCMSAbonentRq1 = (AddCMSAbonentRq1)new Serializer().Deserialize(reqFromFile, addCMSAbonentRq1, typeof(AddCMSAbonentRq1));
                        addCMSAbonentRq1.Request.Clerk = DefaultValuesList.Clerk;
                        addCMSAbonentRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        addCMSAbonentRq1.Request.SessionSpecified = true;
                        addCMSAbonentRq1.Request.Session = SessionId;
                        addCMSAbonentRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'AddCMSAbonentRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        AddCMSAbonentRp1 addCMSAbonentRp1 = new AddCMSAbonentRp1();
                        addCMSAbonentRp1 = (AddCMSAbonentRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(addCMSAbonentRq1, typeof(AddCMSAbonentRq1)))),
                                addCMSAbonentRp1, typeof(AddCMSAbonentRp1));

                        nextChallenge = addCMSAbonentRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;

                    case "18":
                        reqFromFile = requestBuilder.GetReqFromFile("SetCard3DSecureAuthenticationRq");
                        if (string.IsNullOrEmpty(reqFromFile)) goto SelectOperationPoint;

                        SetCard3DSecureAuthenticationRq1 setCard3DSecureAuthenticationRq1 = new SetCard3DSecureAuthenticationRq1();

                        setCard3DSecureAuthenticationRq1 = (SetCard3DSecureAuthenticationRq1)new Serializer().Deserialize(reqFromFile, setCard3DSecureAuthenticationRq1, typeof(SetCard3DSecureAuthenticationRq1));
                        setCard3DSecureAuthenticationRq1.Request.Clerk = DefaultValuesList.Clerk;
                        setCard3DSecureAuthenticationRq1.Request.Ver = Convert.ToDecimal(DefaultValuesList.FIMIVer, CultureInfo.InvariantCulture);
                        setCard3DSecureAuthenticationRq1.Request.SessionSpecified = true;
                        setCard3DSecureAuthenticationRq1.Request.Session = SessionId;
                        setCard3DSecureAuthenticationRq1.Request.Password = CryptPass;

                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'SetCard3DSecureAuthenticationRq':",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        SetCard3DSecureAuthenticationRp1 setCard3DSecureAuthenticationRp1 = new SetCard3DSecureAuthenticationRp1();
                        setCard3DSecureAuthenticationRp1 = (SetCard3DSecureAuthenticationRp1)serializer.Deserialize(
                            new WebTools().SendRequest(serializer.Wrap2SOAP(serializer.Serialize(setCard3DSecureAuthenticationRq1, typeof(SetCard3DSecureAuthenticationRq1)))),
                                setCard3DSecureAuthenticationRp1, typeof(SetCard3DSecureAuthenticationRp1));

                        nextChallenge = setCard3DSecureAuthenticationRp1.Response.NextChallenge;
                        Console.Write("*Next Challenge: ");
                        SetColor(nextChallenge, ConsoleColor.Green);

                        CryptPass = cryptographic.GetCryptPassword(cryptographic.GetPasswordHash(DefaultValuesList.ClerkPassword.ToUpper()),
                            nextChallenge);

                        goto SelectOperationPoint;
                    case "L"://Logoff Transaction
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'LogoffRq':",
                                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        LogoffRp1 logoffRp1 = (LogoffRp1)requestBuilder.GenerateRequest("LogoffRq", SessionId);
                        goto SelectOperationPoint;
                    case "B"://Begin Transaction
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'BeginTransactionRq':",
                                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        SavePoint = new Identifier().GetOracleId();
                        transactionalMode.Begin(SessionId, CryptPass, SavePoint);
                        goto SelectOperationPoint;
                    case "C"://Commit
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'CommitRq':",
                                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        transactionalMode.Commit(SessionId, CryptPass);
                        goto SelectOperationPoint;
                    case "R"://Rollback
                        Console.WriteLine($"\n{"[{0}]",-7}Generated request of operation type 'RollbackRq':",
                               DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        transactionalMode.Rollback(SessionId, CryptPass, SavePoint);
                        goto SelectOperationPoint;
                    default:
                        SetColor("Key entered is wrong!", ConsoleColor.Red);
                        goto SelectOperationPoint;
                }
        }
            catch (Exception ex)
            {
                PrintExceptionEvent(GetType().FullName, ex.GetType().Name, ex.Message);
        new MainProcess().Processing();
    }
}        
    }
}