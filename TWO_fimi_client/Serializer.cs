using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TWO_fimi_client
{
    class Serializer:Logging
    {
        public string Wrap2SOAP(XElement pMessBody)
        {
            Envelope envelop = new Envelope
            {
                Body = string.Empty
            };
            XElement soapEnvelop = Serialize(envelop, typeof(Envelope));
            soapEnvelop.Descendants().SingleOrDefault(p => p.Name.LocalName == "Body").Add(pMessBody);
            return soapEnvelop.ToString();
        }

        public XElement Serialize(object pObject, Type pTypeObject)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(pTypeObject);

            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                };
                XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                xmlSerializer.Serialize(xmlWriter, pObject);

                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
                return XElement.Parse(streamReader.ReadToEnd());
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        public object Deserialize(string pMessage, object pObject, Type pTypeObject)
        {
            try
            {
                XElement xMessBody = XElement.Parse(XElement.Parse(pMessage).Descendants().SingleOrDefault(p => p.Name.LocalName == "Body").FirstNode.ToString());
                XmlSerializer xmlSerializer = new XmlSerializer(pTypeObject);
                return xmlSerializer.Deserialize(xMessBody.CreateReader());
            }
            catch (InvalidOperationException ioe)
            {
                PrintExceptionEvent(GetType().FullName, ioe.GetType().Name, String.Format("{0}: {1}", ioe.Message, ioe.InnerException.Message));
                new MainProcess().Processing();
            }
            return string.Empty;
        }
        /*
        private void Deserialize(string pMessage)
        {
            try
            {
                XElement xElement = XElement.Parse(XElement.Parse(pMessage).Descendants().SingleOrDefault(p => p.Name.LocalName == "Body").FirstNode.ToString());
                //XmlSerializer xmlSerializer = new XmlSerializer(typeof(AcctCreditRq1));
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AcctCreditRp));

                //AcctCreditRq1 acctCreditRq1 = (AcctCreditRq1) xmlSerializer.Deserialize(xElement.CreateReader());
                AcctCreditRp acctCreditRp1 = (AcctCreditRp)xmlSerializer.Deserialize(xElement.CreateReader());
                //return acctCreditRp1;
            }
            catch (InvalidOperationException ioe)
            {
                PrintExceptionEvent(GetType().FullName, ioe.GetType().Name, ioe.Message);
                new MainScreen().Processing();
            }
            //return string.Empty;
        }
        */
    }
}
