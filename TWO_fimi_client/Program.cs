using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace TWO_fimi_client
{
    class DefaultValuesList
    {        
        public static string WorkingDirectory { get; private set; }
        public static string FIMIDriver_EP { get; private set; }
        public static string Clerk { get; private set; }
        public static string ClerkPassword { get; private set; }        
        public static string FIMIVer { get; private set; }
        public static string NeedDicts { get; private set; }
        public static string NeedDictsSpecified { get; private set; }
        public static string AllVendorsSpecified { get; private set; }
        public static string AvoidSessionSpecified { get; private set; }
        public static string Force { get; private set; }        
        public DefaultValuesList()
        {            
            WorkingDirectory = ConfigurationManager.AppSettings.Get("WorkingDirectory");            
            Force = ConfigurationManager.AppSettings.Get("Force");
            FIMIDriver_EP = ConfigurationManager.AppSettings.Get("FIMIDriver_EP");
            Clerk = ConfigurationManager.AppSettings.Get("Clerk");
            ClerkPassword = ConfigurationManager.AppSettings.Get("ClerkPassword");
            FIMIVer = ConfigurationManager.AppSettings.Get("FIMIVer");
            NeedDicts = ConfigurationManager.AppSettings.Get("NeedDicts");
            NeedDictsSpecified = ConfigurationManager.AppSettings.Get("NeedDictsSpecified");
            AllVendorsSpecified = ConfigurationManager.AppSettings.Get("AllVendorsSpecified");
            AvoidSessionSpecified = ConfigurationManager.AppSettings.Get("AvoidSessionSpecified");
        }

        public void Read_cfg()
        {
            Type typeOfDefaultValuesList = typeof(DefaultValuesList);
            PropertyInfo[] DefaultValuesListProperties = typeOfDefaultValuesList.GetProperties();

            Console.WriteLine($"\n{"[{0}]",-7}Reading settings from app.cfg....\n", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (PropertyInfo property in DefaultValuesListProperties)
            {
                switch (property.Name)
                {
                    case "WorkingDirectory":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        string path = property.GetValue(this, null).ToString();
                        if (!Directory.Exists(property.GetValue(this, null).ToString()))
                        {
                            new Logging().SetColor("Could not find a part of the Req file path...", ConsoleColor.Red);
                            Console.ReadLine();
                            return;
                        }
                        break;
                    case "FIMIDriver_EP":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "Clerk":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "ClerkPassword":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "FIMIVer":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "NeedDicts":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "NeedDictsSpecified":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "AllVendorsSpecified":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "AvoidSessionSpecified":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                    case "Force":
                        Console.WriteLine(String.Format("# Field name: '{0}' => '{1}'", property.Name, property.GetValue(this, null)));
                        break;
                }
            }
            Console.ResetColor();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
/*
            string req = string.Empty;
            using (var sr = new StreamReader("C:\\Program Files\\PROJECT\\TWO_fimi_client\\TWO_fimi_client\\Debug\\CreatePersonRp.xml"))
            {
                 req = sr.ReadToEnd();
            }

            CreatePersonRp1 createPersonRp1 = new CreatePersonRp1();
            createPersonRp1 = (CreatePersonRp1) new Serializer().Deserialize(req,
                    createPersonRp1, typeof(CreatePersonRp1));
*/

            Console.SetWindowSize(140, 43);
            Console.Title = String.Format("{0} (v.{1})", Assembly.GetEntryAssembly().GetName().Name, Assembly.GetEntryAssembly().GetName().Version);
            new DefaultValuesList().Read_cfg();
            new MainProcess().Processing();
        }
    }
}