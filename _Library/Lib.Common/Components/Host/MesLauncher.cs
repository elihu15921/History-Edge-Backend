using Lib.Common.Components.Agreements;
using Lib.Common.Components.Func;
using Lib.Common.Components.Models;
using Lib.Common.Manager;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Lib.Common.Components.Func.ExtensionTools;

namespace Lib.Common.Components.Host
{
    public class MesLauncher : BaseStationFactory
    {
        public override async Task SendAsync(PayloadRoot root, HostChannel channel)
        {
            try
            {
                if (channel == HostChannel.Undefined || root.Row.Count == 0) return;

                FoundationProvider provider = new();
                GlobalVariables variables = new();

                XElement xHost = new("host");
                xHost.Add(new XAttribute("prod", provider.FoundationBasic.Eai.Host.Name));
                xHost.Add(new XAttribute("ver", provider.FoundationBasic.Eai.Host.Version));
                xHost.Add(new XAttribute("ip", GeneralTools.GetLocalIP()));
                xHost.Add(new XAttribute("id", provider.FoundationBasic.Eai.Host.Id));
                xHost.Add(new XAttribute("acct", provider.FoundationBasic.Eai.Host.Account));
                xHost.Add(new XAttribute("lang", provider.FoundationBasic.Eai.Host.Language));
                xHost.Add(new XAttribute("timestamp", DateTime.Now.ToString(("yyyyMMddHHmmssfff"))));

                XElement service = new("service");
                service.Add(new XAttribute("prod", provider.FoundationBasic.Eai.Service.Name));
                service.Add(new XAttribute("name", channel switch
                {
                    HostChannel.Status => "change.machine.status.process",
                    HostChannel.Parameter => "parameter.check.process",
                    HostChannel.Production => "production.edc.process",
                    HostChannel.IntegrateSignal => "have.not.define.name.RFID",
                    _ => ""
                }));
                service.Add(new XAttribute("srvver", provider.FoundationBasic.Eai.Service.Srvver));
                service.Add(new XAttribute("ip", provider.FoundationBasic.Eai.Service.Ip));
                service.Add(new XAttribute("id", provider.FoundationBasic.Eai.Service.Id));

                XElement xRoot = new XElement("request");
                xRoot.Add(new XAttribute("key", (xHost.ToString() + service.ToString()).ToMD5()));
                xRoot.Add(new XAttribute("type", "sync"));
                xRoot.Add(xHost);
                xRoot.Add(service);

                XElement xPayload = new("payload");

                XElement xParam = new("param");
                xParam.Add(new XAttribute("key", "std_data"));
                xParam.Add(new XAttribute("type", "xml"));
                xPayload.Add(xParam);

                XElement xDataRequest = new("data_request");
                xParam.Add(xDataRequest);

                XElement xDatainfo = new("datainfo");
                xDataRequest.Add(xDatainfo);

                XElement xParameter = new("parameter");
                xParameter.Add(new XAttribute("key", channel switch
                {
                    HostChannel.Status => "change_machine_status",
                    HostChannel.Parameter => "parameter_check",
                    HostChannel.Production => "production_edc",
                    HostChannel.IntegrateSignal => "have_not_define_name_RFID",
                    _ => ""
                }));
                xParameter.Add(new XAttribute("type", "data"));

                xDatainfo.Add(xParameter);

                XElement xData = new("data");
                xData.Add(new XAttribute("name", channel switch
                {
                    HostChannel.Status => "change_machine_status",
                    HostChannel.Parameter => "parameter_check",
                    HostChannel.Production => "production_edc",
                    HostChannel.IntegrateSignal => "have_not_define_name_RFID",
                    _ => ""
                }));

                xParameter.Add(xData);

                int iKey = 1;
                root.Row.ForEach(c =>
                {

                    XElement xRow = new("row");
                    xRow.Add(new XAttribute("seq", iKey++));

                    XElement xMachineNo = new("field", root.MachineNo);
                    xMachineNo.Add(new XAttribute("name", "machine_no"));
                    xMachineNo.Add(new XAttribute("type", "string"));
                    xRow.Add(xMachineNo);

                    switch (channel)
                    {
                        case HostChannel.Status:
                            XElement xStatus = new("field");
                            xStatus.Add(new XAttribute("name", "machine_status"));
                            xStatus.Add(new XAttribute("type", "numeric"));
                            xStatus.Add(c.AttribValue);
                            xRow.Add(xStatus);

                            XElement xRemark = new("field");
                            xRemark.Add(new XAttribute("name", "remark"));
                            xRemark.Add(new XAttribute("type", "string"));
                            xRemark.Add("");
                            xRow.Add(xRemark);
                            break;

                        case HostChannel.Parameter:
                            XElement xParameterNo = new("field");
                            xParameterNo.Add(new XAttribute("name", "attrib_no"));
                            xParameterNo.Add(new XAttribute("type", "string"));
                            xParameterNo.Add(c.AttribNo);
                            xRow.Add(xParameterNo);

                            XElement xParameterValue = new("field");
                            xParameterValue.Add(new XAttribute("name", "attrib_value"));
                            xParameterValue.Add(new XAttribute("type", "string"));
                            xParameterValue.Add(c.AttribValue);
                            xRow.Add(xParameterValue);
                            break;

                        case HostChannel.Production:
                            XElement xPlotNo = new("field");
                            xPlotNo.Add(new XAttribute("name", "plot_no"));
                            xPlotNo.Add(new XAttribute("type", "string"));
                            xPlotNo.Add("");
                            xRow.Add(xPlotNo);

                            XElement xCarrierNo = new("field");
                            xCarrierNo.Add(new XAttribute("name", "carrier_no"));
                            xCarrierNo.Add(new XAttribute("type", "string"));
                            xCarrierNo.Add("");
                            xRow.Add(xCarrierNo);

                            XElement xProductionNo = new("field");
                            xProductionNo.Add(new XAttribute("name", "attrib_no"));
                            xProductionNo.Add(new XAttribute("type", "string"));
                            xProductionNo.Add(c.AttribNo);
                            xRow.Add(xProductionNo);

                            XElement xProductionValue = new("field");
                            xProductionValue.Add(new XAttribute("name", "attrib_value"));
                            xProductionValue.Add(new XAttribute("type", "string"));
                            xProductionValue.Add(c.AttribValue);
                            xRow.Add(xProductionValue);
                            break;
                        //TODO:
                        case HostChannel.IntegrateSignal:
                            XElement xIntegrateSignal = new("field");
                            xIntegrateSignal.Add(new XAttribute("name", "attrib_no"));
                            xIntegrateSignal.Add(new XAttribute("type", "string"));
                            xIntegrateSignal.Add(c.AttribNo);
                            xRow.Add(xIntegrateSignal);

                            XElement xSignalValue = new("field");
                            xSignalValue.Add(new XAttribute("name", "attrib_value"));
                            xSignalValue.Add(new XAttribute("type", "string"));
                            xSignalValue.Add(c.AttribValue);
                            xRow.Add(xSignalValue);
                            break;
                    };

                    XElement xReport = new("field");
                    xReport.Add(new XAttribute("name", "report_datetime"));
                    xReport.Add(new XAttribute("type", "date"));
                    xReport.Add(variables.NowTime);
                    xRow.Add(xReport);

                    xData.Add(xRow);
                });

                xRoot.Add(xPayload);

                var request = new MesServiceSOAP.wsEAISoapClient(MesServiceSOAP.wsEAISoapClient.EndpointConfiguration.wsEAISoap, GlobalVariables.MesUrl);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [sMMP => sMES] Request:\r\n" + xRoot.ToString());
                var response = await request.invokeSrvAsync(xRoot.ToString());
                string result = response.Body.invokeSrvResult;

                Console.WriteLine(result);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [sMES => sMMP] Response:\r\n" + result);
            }
            catch (Exception e)
            {
                Console.WriteLine("sMMP => " + e.Message + "\n" + e.StackTrace);
            }
            finally
            { 
                GC.Collect();
            }
        }
    }
}
