using Lib.Common.Components.Agreements;
using Lib.Common.Manager.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static Lib.Common.Manager.GlobalVariables;

namespace Lib.Common.Manager
{
    public class FoundationWriter : JsonWriterFactory
    {
        public override bool Write(IPAddress serverAddress, string request)
        {
            try
            {
                using StreamWriter stream = File.CreateText(FoundationDocument);
                using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true,
                    Encoding = new UTF8Encoding(false)
                });

                writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");

                GlobalVariables globally = new();
                XNamespace ns = Foundation;
                writer.WriteStartElement("Foundation", ns.NamespaceName);

                FoundationRoot rootBox = new();

                if (request != null) rootBox = JToken.Parse(request).ToObject<FoundationRoot>();

                writer.WriteStartElement("Edge");
                writer.WriteAttributeString("Name", rootBox.Edge.Name);
                writer.WriteAttributeString("Version", rootBox.Edge.Version);
                writer.WriteEndElement();

                writer.WriteStartElement("Server");
                writer.WriteAttributeString("Name", rootBox.Server.Name);
                writer.WriteAttributeString("Disabled", rootBox.Disabled == true ? "1" : "0");
                writer.WriteAttributeString("UpdateTime", globally.NowTime);

                string address = serverAddress.ToString();
                int nn = address.Split(':').Length;
                if (address.Split(':').Length == 1) address = ":::";

                writer.WriteStartElement("URL");
                writer.WriteString("http://" + address.Split(':')[3]);
                writer.WriteEndElement();

                writer.WriteStartElement("MqttPort");
                writer.WriteString(rootBox.Server.MqttPort.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Eai");
                writer.WriteAttributeString("Version", rootBox.Eai.Version);

                writer.WriteStartElement("Host");
                writer.WriteAttributeString("Name", rootBox.Eai.Host.Name);
                writer.WriteAttributeString("Version", rootBox.Eai.Host.Version);
                writer.WriteAttributeString("Id", rootBox.Eai.Host.Id);
                writer.WriteAttributeString("Language", rootBox.Eai.Host.Language);

                writer.WriteStartElement("Account");
                writer.WriteString(rootBox.Eai.Host.Account);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("Service");
                writer.WriteAttributeString("Name", rootBox.Eai.Service.Name);
                writer.WriteAttributeString("Srvver", rootBox.Eai.Service.Srvver);
                writer.WriteAttributeString("Id", rootBox.Eai.Service.Id);

                writer.WriteStartElement("Ip");
                writer.WriteString(rootBox.Eai.Service.Ip);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();

                #region ModbusTcp

                writer.WriteStartElement(nameof(Communication.ModbusTcp));

                rootBox.ModbusTCP.ForEach(c =>
                {
                    writer.WriteStartElement("Machine");
                    writer.WriteAttributeString("Name", c.MachineNo);
                    writer.WriteAttributeString("Production", c.Production == true ? "1" : "0");
                    writer.WriteAttributeString("Disabled", c.Disabled == true ? "1" : "0");

                    writer.WriteStartElement("Version");
                    writer.WriteString(c.Version);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Address");
                    writer.WriteString(c.Address);
                    writer.WriteEndElement();

                    writer.WriteStartElement("CollectionMode");
                    writer.WriteString(c.CollectionMode);
                    writer.WriteEndElement();

                    writer.WriteStartElement("CommandMode");
                    writer.WriteString(c.CommandMode);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Quartz");
                    writer.WriteString(c.Quartz);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Port");
                    writer.WriteString(c.Port.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("Row");

                    c.Map.ToList().ForEach(c =>
                    {
                        writer.WriteStartElement("Parameter");
                        writer.WriteAttributeString("Disabled", c.Disabled == true ? "1" : "0");
                        writer.WriteAttributeString("Channel", c.Channel);
                        writer.WriteAttributeString("FunctionCode", c.FunctionCode.ToString());
                        writer.WriteAttributeString("SlaveAddress", c.SlaveAddress.ToString());
                        writer.WriteAttributeString("StartAddress", c.StartAddress.ToString());
                        writer.WriteAttributeString("OffSet", c.Offset.ToString());

                        c.NumberOfPoints.ToList().ForEach(c =>
                        {
                            writer.WriteStartElement("Element");
                            writer.WriteAttributeString("Id", c.PointNo.ToString());
                            writer.WriteAttributeString("AttribName", c.AttribName);
                            //Add By YanHao - 20210325
                            writer.WriteAttributeString("AttribType", c.AttribType);
                            writer.WriteEndElement();
                        });

                        writer.WriteEndElement();
                    });

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                });

                writer.WriteEndElement();

                #endregion

                #region OpcUa

                //writer.WriteStartElement(nameof(Communication.OpcUa));

                //rootBox.OpcUa.ForEach(c =>
                //{
                //    writer.WriteEndElement();

                //    writer.WriteEndElement();
                //    writer.WriteEndElement();
                //});

                //writer.WriteEndElement();

                #endregion

                #region WebApi

                //writer.WriteStartElement(nameof(Communication.WebApi));

                //rootBox.WebApi.ForEach(c =>
                //{
                //    writer.WriteEndElement();

                //    writer.WriteEndElement();
                //    writer.WriteEndElement();
                //});

                //writer.WriteEndElement();

                #endregion

                writer.WriteEndElement();
                writer.WriteEndDocument();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("sMMP => " + e.Message + "\n" + e.StackTrace);

                return false;
            }
        }
    }
}