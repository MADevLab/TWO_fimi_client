using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TWO_fimi_client
{
    class WebTools: Logging
    {        
        public string SendRequest(string requestMessage)
        {
            try
            {
                SetColor(String.Format("\n{0}\n\n", requestMessage), ConsoleColor.Cyan);
                Console.WriteLine($"\n{"[{0}]",-7}Trying to send request to host {{1}} ...",
                                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), DefaultValuesList.FIMIDriver_EP);
                string responseFromServer;
                // Create a request using a URL that can receive a post.   
                WebRequest request = WebRequest.Create(DefaultValuesList.FIMIDriver_EP);
                // Set the Method property of the request to POST.  
                request.Method = "POST";


                // Create POST data and convert it to a byte array.  
                //string postData = "This is a test that posts this string to a Web server.";
                byte[] byteArray = Encoding.UTF8.GetBytes(requestMessage);

                // Set the ContentType property of the WebRequest.  
                //request.ContentType = "application/x-www-form-urlencoded";                
                request.ContentType = "application/soap+xml;charset=UTF-8;";

                // Set the ContentLength property of the WebRequest.  
                request.ContentLength = byteArray.Length;

                // Get the request stream.  
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.  
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.  
                dataStream.Close();

                // Get the response.  
                WebResponse response = request.GetResponse();
                // Display the status.  
                Console.WriteLine($"{"[{0}]",-7}StatusDescription: [{{1}}]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                    ((HttpWebResponse)response).StatusDescription);
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                // Get the stream containing content returned by the server.  
                // The using block ensures the stream is automatically closed.
                using (dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Console.WriteLine($"{"[{0}]",-7}Host response:", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    TextFormater(responseFromServer);
                }
                // Close the response.  
                response.Close();
                return responseFromServer;
            }
            catch (WebException we)
            {
                if (we.Response != null)
                {
                    Console.WriteLine($"{"[{0}]",-7}Host response:", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    using (WebResponse response = we.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        SetColor(String.Format("\n# {0}", httpResponse.StatusCode.ToString()), ConsoleColor.Red);
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            TextFormater(reader.ReadToEnd(), ConsoleColor.Red);
                        }
                    }
                    SendRollBack:
                    Console.Write("\n\nSend Rollback request? [Y/N] ");
                    switch (Console.ReadLine().ToUpper())
                    {
                        case "Y"://RollbackRq
                            new TransactionalOperating().Rollback(MainProcess.SessionId, MainProcess.CryptPass, MainProcess.SavePoint);
                            new MainProcess().Processing();
                            break;
                        case "N"://To itemList                                
                            new MainProcess().Processing();
                            break;
                        default:
                            SetColor("Key entered is wrong!", ConsoleColor.Red);
                            goto SendRollBack;
                    }
                }
                else
                {
                    PrintExceptionEvent(GetType().FullName, we.GetType().Name, we.Message);
                    new MainProcess().Processing();
                }
            }
            catch (UriFormatException ue)
            {
                PrintExceptionEvent(GetType().FullName, ue.GetType().Name, ue.Message);
                new MainProcess().Processing();
            }
            catch (Exception ex)
            {
                PrintExceptionEvent(GetType().FullName, ex.GetType().Name, ex.Message);
                new MainProcess().Processing();
            }
            return String.Empty;
        }
            
        public string TextFormater(string pMessage, ConsoleColor pCololorIndex = ConsoleColor.DarkCyan)
        {
            string result = String.Empty;

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(pMessage);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                SetColor(String.Format("\n{0}\n", formattedXml), pCololorIndex);

                result = formattedXml;
            }
            catch (XmlException xmlExc)
            {
                PrintExceptionEvent(GetType().FullName, xmlExc.GetType().Name, xmlExc.Message);
                new MainProcess().Processing();
            }

            mStream.Close();
            writer.Close();

            return result;
        }
    }
}

/*
         public string NewSendRequest(string requestMessage)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings.Get("FIMIDriver_EP"));
                byte[] bytes;
                bytes = Encoding.UTF8.GetBytes(requestMessage);
                request.ProtocolVersion = HttpVersion.Version10;
                request.Accept = "gzip,deflate";
                //request.Connection = "Keep-Alive";
                
                request.ContentType = "application/soap+xml;charset=UTF-8;";
                request.ContentLength = bytes.Length;
                request.Method = "POST";                
                //request.ServicePoint.Expect100Continue = false;


                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return responseStr;
                }             
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
            return null;
        }
        */
