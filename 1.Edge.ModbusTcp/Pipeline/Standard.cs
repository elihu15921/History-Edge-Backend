using Edge.ModbusTcp.Components;
using Edge.ModbusTcp.Models;
using Lib.Common.Components.Agreements;
using Lib.Common.Components.Models;
using Lib.Common.Manager;
using Lib.Common.Manager.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Modbus.Device;
using Modbus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static Lib.Common.Manager.GlobalVariables;
using Lib.Common.Components.History;
using Serilog.Events;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Collections.Concurrent;

namespace Edge.ModbusTcp.Pipeline
{
    //[DisallowConcurrentExecution]
    internal class Standard : ProtocolFactory, IJob
    {
        public override async Task SendAsync(IConfigurationRoot root)
        {
            ModbusTcpManager.RebootModbusTcp = false;

            int iFrequency = 0;

            YamlBase.Modules.Where(c => c.Launcher == nameof(Communication.ModbusTcp)).Select(c => new
            {
                c.Frequency

            }).ToList().ForEach(c =>
            {
                iFrequency = c.Frequency;
            });

            //List<MachineShell> MachineBoxes = new();

            root.GetSection(nameof(ModbusTcpTitle.MachineBox)).GetChildren().Select(c => new
            {
                machineNo = c.GetValue<string>(nameof(ModbusTcpRoot.MachineNo)),
                production = c.GetValue<bool>(nameof(ModbusTcpRoot.Production)),
                disabled = c.GetValue<bool>(nameof(ModbusTcpRoot.Disabled)),
                vesion = c.GetValue<string>(nameof(ModbusTcpRoot.Version)),
                address = c.GetValue<string>(nameof(ModbusTcpRoot.Address)),
                collectionmode = c.GetValue<string>(nameof(ModbusTcpRoot.CollectionMode)),
                commandmode = c.GetValue<string>(nameof(ModbusTcpRoot.CommandMode)),
                quartz = c.GetValue<string>(nameof(ModbusTcpRoot.Quartz)),
                port = c.GetValue<int>(nameof(ModbusTcpRoot.Port)),
                map = c.GetSection(nameof(ModbusTcpRoot.Map)).GetChildren().Select(c => new
                {
                    disabled = c.GetValue<bool>(nameof(ElementBox.Disabled)),
                    channel = c.GetValue<string>(nameof(ElementBox.Channel)),
                    functionCode = c.GetValue<int>(nameof(ElementBox.FunctionCode)),
                    slaveAddress = c.GetValue<byte>(nameof(ElementBox.SlaveAddress)),
                    startAddress = c.GetValue<ushort>(nameof(ElementBox.StartAddress)),
                    offSet = c.GetValue<ushort>(nameof(ElementBox.Offset)),
                    Points = c.GetSection(nameof(ElementBox.NumberOfPoints)).GetChildren().Select(c => new
                    {
                        pointNo = c.GetValue<int>(nameof(Numberofpoint.PointNo)),
                        attribName = c.GetValue<string>(nameof(Numberofpoint.AttribName)),
                        //Add By YanHao - 20210325
                        attribType = c.GetValue<string>(nameof(Numberofpoint.AttribType))

                    }).ToList()

                }).ToList()

            }).ToList().ForEach(c =>
            {
                if (c.disabled) return;

                List<MessageBox> boxes = new();

                c.map.ForEach(c =>
                {
                    if (c.disabled) return;

                    HostChannel Channel = c.channel switch
                    {
                        nameof(HostChannel.Status) => HostChannel.Status,
                        nameof(HostChannel.Parameter) => HostChannel.Parameter,
                        nameof(HostChannel.Production) => HostChannel.Production,
                        nameof(HostChannel.ProductionCommand) => HostChannel.ProductionCommand,
                        nameof(HostChannel.IntegrateSignal) => HostChannel.IntegrateSignal,
                        _ => HostChannel.Undefined
                    };

                    if (Channel == HostChannel.Undefined) return;

                    List<PickPoint> points = new();

                    c.Points.ForEach(c =>
                    {
                        points.Add(new()
                        {
                            PointNo = c.pointNo,
                            AttribName = c.attribName,
                            //Add By YanHao - 20210325
                            AttribType = c.attribType
                        });
                    });

                    boxes.Add(new()
                    {
                        Channel = Channel,
                        FunctionCode = c.functionCode,
                        SlaveAddress = c.slaveAddress,
                        StartAddress = c.startAddress,
                        Offset = c.offSet,
                        PickPoints = points
                    });
                });

                MachineBoxes.Add(new()
                {
                    MachineNo = c.machineNo,
                    Production = c.production,
                    Vesion = c.vesion,
                    Address = c.address,
                    CollectionMode = c.collectionmode,
                    CommandMode = c.commandmode,
                    Quartz = c.quartz,
                    Port = c.port,
                    MessageBoxes = boxes
                });
            });

            await Task.Run(() =>
            {
                try
                {
                    //Callback += new TimerCallback(Working);
                    //Callback += new TimerCallback(RocketLaunch);

                    //Timer = new Timer(Callback, HostChannel.Production, Timeout.Infinite, iFrequency);
                    //Timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(iFrequency));

                    //202109 - 改用Quartz组件统一进行作业调度
                    CreaterScheduler(iFrequency, MachineBoxes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("sMMP => " + e.Message + "\n" + e.StackTrace);
                }
            });

            //202109已弃用，改用Quartz组件进行任务调度
            void Working(object obj)
            {
                LogBuilder.WriteLog(LogEventLevel.Information, "[P1&P2] [sMMP => MODBUS] Query data task running.");
                MachineBoxes.ForEach(async b =>
                {
                    await Task.Run(() =>
                    {
                        int iPort = b.Port;
                        string sMachineNo = b.MachineNo, sAddress = b.Address;

                        //lock (MachineSwitch) if (!MachineSwitch.ContainsKey(sMachineNo)) MachineSwitch.Add(sMachineNo, true);

                        b.MessageBoxes.ForEach(async c =>
                        {
                            try
                            {
                                if (c.PickPoints.Count == 0) return;

                                TcpClient client = new TcpClient(sAddress, iPort);

                                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                                {
                                    ushort[] result = await master.ReadHoldingRegistersAsync(c.SlaveAddress, c.StartAddress, Convert.ToUInt16(c.Offset));

                                    if (result == null || result.Length == 0) return;

                                    //MachineSwitch[sMachineNo] = true;

                                    c.PickPoints.ForEach(p =>
                                    {
                                        string sKey = sMachineNo + "#" + p.AttribName;
                                        //Add By YanHao - 20210325
                                        //string value = Converter(result, c.AttribType);

                                        float value = 0;
                                        double longValue = 0;
                                        if (p.AttribType.ToLower() == "long")
                                        {
                                            longValue = GetRegistersByNModbus(result, c.StartAddress, p.AttribType);//Convert registers to long
                                        }
                                        else
                                        {
                                            value = GetRegistersByNModbus(result, c.StartAddress)[0];//Convert registers to float
                                            LogBuilder.WriteLog(LogEventLevel.Information, $"[P1&P2] [MODBUS => sMMP] Machine = {sMachineNo}, Address = {c.StartAddress}, Value is {value}.");
                                        }

                                        if (!RowBox.ContainsKey(sKey))
                                        {
                                            //Modify By YanHao - 20210325
                                            if (p.AttribType.ToLower() == "long")
                                            {
                                                RowBox.AddOrUpdate(sKey, longValue.ToString(), (key, value) => longValue.ToString());
                                            }
                                            else RowBox.AddOrUpdate(sKey, value.ToString(), (key, value) => value.ToString());
                                            //RowBox.Add(sKey, result[c.PointNo]);
                                        }
                                        else
                                        {
                                            //Modify By YanHao - 20210325
                                            if (p.AttribType.ToLower() == "long")
                                            {
                                                RowBox[sKey] = longValue.ToString();
                                            }
                                            else RowBox[sKey] = value.ToString();
                                            //RowBox[sKey] = result[c.PointNo];
                                        }
                                    });

                                    //Add By YanHao - 20210325
                                    //Just for test
                                    //await master.WriteSingleRegisterAsync(c.SlaveAddress, 2080, 1);
                                };
                                client.Dispose();
                            }
                            catch (Exception e)
                            {
                                //if (MachineSwitch[sMachineNo] == true)
                                {
                                    //webapi <==

                                    //Console.WriteLine($" No.{sMachineNo} => {e.Message}");
                                    LogBuilder.WriteLog(LogEventLevel.Error, $" No.{sMachineNo} => {e.Message}");
                                }

                                //MachineSwitch[sMachineNo] = false;
                            }
                        });

                    });
                });
            }
            //202109已弃用，改用Quartz组件进行任务调度
            void RocketLaunch(object obj)
            {
                MachineBoxes.ToList().ForEach(async b =>
                {
                    switch (b.CollectionMode)
                    {
                        case "0"://实时采集参数
                            await Task.Run(() =>
                            {
                                bool bProduction = b.Production;
                                string sMachineNo = b.MachineNo, sVersion = b.Vesion;
                                //Production处理
                                List<Parameter> production = new();
                                //Status处理
                                List<Parameter> status = new();
                                //IntegrateSignal处理
                                List<Parameter> signal = new();

                                b.MessageBoxes.ForEach(c =>
                                {
                                    bool shouldPush = false;

                                    c.PickPoints.ForEach(p =>
                                    {
                                        string sKey = sMachineNo + "#" + p.AttribName;

                                        if (RowBox.ContainsKey(sKey))
                                        {
                                            if (!HistoryBox.ContainsKey(sKey))
                                            {
                                                string _temp = Convert.ToString(RowBox[sKey]);
                                                HistoryBox.AddOrUpdate(sKey, _temp, (key, value) => _temp);
                                                shouldPush = true;
                                            }
                                            else
                                            {
                                                if (RowBox[sKey] != HistoryBox[sKey])
                                                {
                                                    Console.WriteLine($"##########Rocket!  {p.AttribName}");
                                                    switch (c.Channel)
                                                    {
                                                        case HostChannel.Production:
                                                            production.Add(new()
                                                            {
                                                                AttribNo = p.AttribName,
                                                                AttribValue = RowBox[sKey]
                                                            });
                                                            break;

                                                        case HostChannel.Status:
                                                            status.Add(new()
                                                            {
                                                                AttribNo = p.AttribName,
                                                                AttribValue = RowBox[sKey]
                                                            });
                                                            break;

                                                        case HostChannel.IntegrateSignal:
                                                            string MoldingCode = RFIDRule(RowBox[sKey]);
                                                            signal.Add(new()
                                                            {
                                                                AttribNo = MoldingCode.Substring(0, 1),
                                                                AttribValue = MoldingCode.Substring(1, 6),
                                                            });
                                                            break;

                                                        default: break;
                                                    }
                                                    HistoryBox[sKey] = RowBox[sKey];
                                                    shouldPush = true;
                                                }
                                            }
                                        }
                                    });
                                    if (shouldPush)
                                    {
                                        GlobalVariables globally = new();

                                        GlobalApproach.PushDataToHost(c.Channel, new()
                                        {
                                            Version = sVersion,
                                            Production = bProduction,
                                            MachineNo = sMachineNo,
                                            ReportDateTime = globally.NowTime,
                                            Row = production
                                        });
                                        production.Clear();

                                        GlobalApproach.PushDataToHost(c.Channel, new()
                                        {
                                            Version = sVersion,
                                            Production = bProduction,
                                            MachineNo = sMachineNo,
                                            ReportDateTime = globally.NowTime,
                                            Row = status
                                        });
                                        status.Clear();

                                        GlobalApproach.PushDataToHost(c.Channel, new()
                                        {
                                            Version = sVersion,
                                            Production = bProduction,
                                            MachineNo = sMachineNo,
                                            ReportDateTime = globally.NowTime,
                                            Row = signal
                                        });
                                        signal.Clear();
                                    }
                                });
                            });
                            break;

                        case "1"://产量变化时采集上报参数

                            //尝试寻找产量参数 - 01
                            bool IsQtyUpdate = false;
                            MessageBox qtybox = b.MessageBoxes.Find(c => c.PickPoints.Exists(p => p.AttribName == "01"));
                            if (qtybox == null)
                            {
                                Console.WriteLine($"Can not found machine counter setting for machine '{b.MachineNo}'!");
                                IsQtyUpdate = false;
                            }
                            else
                            {
                                //取产量参数并对比是否发生变化
                                qtybox.PickPoints.ForEach(p =>
                                {
                                    string sKey = b.MachineNo + "#" + p.AttribName;

                                    if (RowBox.ContainsKey(sKey))
                                    {
                                        if (!HistoryBox.ContainsKey(sKey)) IsQtyUpdate = true;
                                        else
                                        {
                                            if (RowBox[sKey] != HistoryBox[sKey]) IsQtyUpdate = true;
                                            else IsQtyUpdate = false;
                                        }
                                    }
                                });
                            }
                            await Task.Run(() =>
                            {
                                bool bProduction = b.Production;
                                string sMachineNo = b.MachineNo, sVersion = b.Vesion;
                                //Production处理
                                List<Parameter> production = new();
                                //Status处理
                                List<Parameter> status = new();

                                b.MessageBoxes.ForEach(c =>
                                {
                                    bool shouldPush = false;

                                    c.PickPoints.ForEach(p =>
                                    {
                                        string sKey = sMachineNo + "#" + p.AttribName;

                                        if (RowBox.ContainsKey(sKey))
                                        {
                                            if (!HistoryBox.ContainsKey(sKey))
                                            {
                                                string _temp = Convert.ToString(RowBox[sKey]);
                                                HistoryBox.AddOrUpdate(sKey, _temp, (key, value) => _temp);
                                            }
                                            else
                                            {
                                                if (RowBox[sKey] != HistoryBox[sKey])
                                                {
                                                    switch (c.Channel)
                                                    {
                                                        case HostChannel.Production:
                                                            if (!IsQtyUpdate) break;//如果产量无变化，此次不更新上报

                                                            production.Add(new()
                                                            {
                                                                AttribNo = p.AttribName,
                                                                AttribValue = RowBox[sKey]
                                                            });
                                                            HistoryBox[sKey] = RowBox[sKey];
                                                            shouldPush = true;
                                                            break;

                                                        case HostChannel.Status:
                                                            status.Add(new()
                                                            {
                                                                AttribNo = p.AttribName,
                                                                AttribValue = RowBox[sKey]
                                                            });
                                                            HistoryBox[sKey] = RowBox[sKey];
                                                            shouldPush = true;
                                                            break;
                                                        default:
                                                            HistoryBox[sKey] = RowBox[sKey];
                                                            shouldPush = true;
                                                            break;
                                                    }

                                                }
                                            }
                                        }
                                    });

                                    if (shouldPush)
                                    {
                                        GlobalVariables globally = new();

                                        GlobalApproach.PushDataToHost(c.Channel, new()
                                        {
                                            Version = sVersion,
                                            Production = bProduction,
                                            MachineNo = sMachineNo,
                                            ReportDateTime = globally.NowTime,
                                            Row = production
                                        });
                                        production.Clear();

                                        GlobalApproach.PushDataToHost(c.Channel, new()
                                        {
                                            Version = sVersion,
                                            Production = bProduction,
                                            MachineNo = sMachineNo,
                                            ReportDateTime = globally.NowTime,
                                            Row = status
                                        });
                                        status.Clear();
                                    }
                                });
                            });
                            break;
                    }
                });
            }

            async void CreaterScheduler(int interval, List<MachineShell> boxes)
            {
                //创建一个作业调度池
                NameValueCollection initFactoryOptions = new NameValueCollection();
                initFactoryOptions["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
                initFactoryOptions["quartz.threadPool.threadCount"] = "500";

                ISchedulerFactory schedf = new StdSchedulerFactory(initFactoryOptions);
                IScheduler sched = await schedf.GetScheduler();

                //创建出一个具体的作业
                IJobDetail job = JobBuilder.Create<Standard>().WithIdentity("Job", "group").StoreDurably().Build();

                //创建出一个具体的作业
                //IJobDetail job1 = JobBuilder.Create<WriteValue>().WithIdentity("Job1", "group1").StoreDurably().Build();

                //创建自动执行触发器
                ITrigger _Simpletrigger = TriggerBuilder.Create()
                    .WithIdentity("Simple", "group1").WithPriority(0)
                    .ForJob(job)
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(interval).RepeatForever())//每间隔固定时间执行一次
                    .Build();
                //创建定时执行触发器
                List<ITrigger> lstTriggers = new List<ITrigger>();
                
                boxes.ForEach(b=>
                {
                    if (b.CommandMode == "1")
                    {
                        //解析获取定时信息 >> Quartz = "17,29;17,31"
                        foreach (string _cron in b.Quartz.Split(";"))
                        {
                            ITrigger _Crontrigger = TriggerBuilder.Create()
                            .WithIdentity(b.MachineNo + "_Cron", b.MachineNo + "_"+_cron).WithPriority(1)
                            .ForJob(job)
                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(int.Parse(_cron.Split(",")[0]), int.Parse(_cron.Split(",")[1])))//每天0点0分执行一次
                            .Build();

                            lstTriggers.Add(_Crontrigger);
                        }

                        //ITrigger _Crontrigger = TriggerBuilder.Create()
                        //.WithIdentity(b.MachineNo + "_Cron", b.MachineNo + "group2").WithPriority(1)
                        //.ForJob(job)
                        //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(int.Parse(b.Quartz.Split(",")[0]), int.Parse(b.Quartz.Split(",")[1])))//每天0点0分执行一次
                        //.Build();

                        //lstTriggers.Add(_Crontrigger);
                    }
                });
                //ITrigger _Crontrigger = TriggerBuilder.Create()
                //    .WithIdentity("Cron", "group2").WithPriority(1)
                //    .ForJob(job)
                //    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(11, 29))//每天0点0分执行一次
                //    .Build();

                //加入作业调度池中
                await sched.AddJob(job, false);
                lstTriggers.ForEach(async t=>
                {
                    await sched.ScheduleJob(t);
                });
                await sched.ScheduleJob(_Simpletrigger);
                //开始运行
                await sched.Start();
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            switch (context.Trigger.Key.Name)
            {
                case "Simple":
                    LogBuilder.WriteLog(LogEventLevel.Information, "[P1&P2] [sMMP => MODBUS] Query data task running.");
                    //遍历参数
                    await Task.Run(()=>
                    {
                        MachineBoxes.ForEach(async b =>
                        {
                            await Task.Run(() =>
                            {
                                int iPort = b.Port;
                                string sMachineNo = b.MachineNo, sAddress = b.Address;

                                //lock (MachineSwitch) if (!MachineSwitch.ContainsKey(sMachineNo)) MachineSwitch.Add(sMachineNo, true);

                                b.MessageBoxes.ForEach(async c =>
                                {
                                    try
                                    {
                                        if (c.PickPoints.Count == 0) return;
                                        if (c.FunctionCode != 3) return;

                                        //TcpClient client = new TcpClient(sAddress, iPort);
                                        using (TcpClient client = new())
                                        {
                                            if (!client.ConnectAsync(sAddress, iPort).Wait(3000))
                                            {
                                                LogBuilder.WriteLog(LogEventLevel.Error, $" No.{sMachineNo} => {sAddress}:{iPort} 连接超时，请检查网络或端口！");
                                                client.Dispose();
                                                return;
                                            }

                                            using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                                            {
                                                ushort[] result = await master.ReadHoldingRegistersAsync(c.SlaveAddress, c.StartAddress, Convert.ToUInt16(c.Offset));

                                                if (result == null || result.Length == 0) return;

                                                //MachineSwitch[sMachineNo] = true;

                                                c.PickPoints.ForEach(p =>
                                                {
                                                    string sKey = sMachineNo + "#" + p.AttribName;
                                                    //Add By YanHao - 20210325
                                                    //string value = Converter(result, c.AttribType);

                                                    float value = 0;
                                                    double longValue = 0;
                                                    if (p.AttribType.ToLower() == "long")
                                                    {
                                                        longValue = GetRegistersByNModbus(result, c.StartAddress, p.AttribType);//Convert registers to long
                                                    }
                                                    else
                                                    {
                                                        value = GetRegistersByNModbus(result, c.StartAddress)[0];//Convert registers to float
                                                        LogBuilder.WriteLog(LogEventLevel.Information, $"[P1&P2] [MODBUS => sMMP] Machine = {sMachineNo}, Address = {c.StartAddress}, Value is {value}.");
                                                    }

                                                    if (!RowBox.ContainsKey(sKey))
                                                    {
                                                        //Modify By YanHao - 20210325
                                                        if (p.AttribType.ToLower() == "long")
                                                        {
                                                            RowBox.AddOrUpdate(sKey, longValue.ToString(), (key, value) => longValue.ToString());
                                                        }
                                                        else RowBox.AddOrUpdate(sKey, value.ToString(), (key, value) => value.ToString());
                                                        //RowBox.Add(sKey, result[c.PointNo]);
                                                    }
                                                    else
                                                    {
                                                        //Modify By YanHao - 20210325
                                                        if (p.AttribType.ToLower() == "long")
                                                        {
                                                            RowBox[sKey] = longValue.ToString();
                                                        }
                                                        else RowBox[sKey] = value.ToString();
                                                        //RowBox[sKey] = result[c.PointNo];
                                                    }
                                                });

                                                //Add By YanHao - 20210325
                                                //Just for test
                                                //await master.WriteSingleRegisterAsync(c.SlaveAddress, 2080, 1);
                                            };
                                            client.Dispose();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //if (MachineSwitch[sMachineNo] == true)
                                        {
                                            //webapi <==

                                            //Console.WriteLine($" No.{sMachineNo} => {e.Message}");
                                            LogBuilder.WriteLog(LogEventLevel.Error, $" No.{sMachineNo} => {e.Message}");
                                        }

                                        //MachineSwitch[sMachineNo] = false;
                                    }
                                    finally
                                    {
                                        GC.Collect();
                                    }
                                });

                            });
                        });
                        //参数上报
                        MachineBoxes.ToList().ForEach(async b =>
                        {
                            switch (b.CollectionMode)
                            {
                                case "0"://实时采集参数
                                    await Task.Run(() =>
                                    {
                                        bool bProduction = b.Production;
                                        string sMachineNo = b.MachineNo, sVersion = b.Vesion;
                                        //Production处理
                                        List<Parameter> production = new();
                                        //Status处理
                                        List<Parameter> status = new();
                                        //IntegrateSignal处理
                                        List<Parameter> signal = new();

                                        b.MessageBoxes.ForEach(c =>
                                        {
                                            if (c.FunctionCode != 3) return;
                                            bool shouldPush = false;

                                            c.PickPoints.ForEach(p =>
                                            {
                                                string sKey = sMachineNo + "#" + p.AttribName;

                                                if (RowBox.ContainsKey(sKey))
                                                {
                                                    if (!HistoryBox.ContainsKey(sKey))
                                                    {
                                                        string _temp = Convert.ToString(RowBox[sKey]);
                                                        HistoryBox.AddOrUpdate(sKey, _temp, (key, value) => _temp);
                                                        shouldPush = true;
                                                        //TODO: HostChannel.IntegrateSignal初始发送一次
                                                        switch (c.Channel)
                                                        {
                                                            case HostChannel.IntegrateSignal:
                                                                string MoldingCode = RFIDRule(RowBox[sKey]);
                                                                signal.Add(new()
                                                                {
                                                                    AttribNo = MoldingCode.Substring(0, 1),
                                                                    AttribValue = MoldingCode.Substring(1, 6),
                                                                });
                                                                break;
                                                            default: break;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        if (RowBox[sKey] != HistoryBox[sKey])
                                                        {
                                                            Console.WriteLine($"##########Rocket!  {p.AttribName}");
                                                            switch (c.Channel)
                                                            {
                                                                case HostChannel.Production:
                                                                    production.Add(new()
                                                                    {
                                                                        AttribNo = p.AttribName,
                                                                        AttribValue = RowBox[sKey]
                                                                    });
                                                                    break;

                                                                case HostChannel.Status:
                                                                    status.Add(new()
                                                                    {
                                                                        AttribNo = p.AttribName,
                                                                        AttribValue = RowBox[sKey]
                                                                    });
                                                                    break;

                                                                case HostChannel.IntegrateSignal:
                                                                    string MoldingCode = RFIDRule(RowBox[sKey]);
                                                                    signal.Add(new()
                                                                    {
                                                                        AttribNo = MoldingCode.Substring(0, 1),
                                                                        AttribValue = MoldingCode.Substring(1, 6),
                                                                    });
                                                                    break;
                                                                default: break;
                                                            }
                                                            HistoryBox[sKey] = RowBox[sKey];
                                                            shouldPush = true;
                                                        }
                                                    }
                                                }
                                            });
                                            if (shouldPush)
                                            {
                                                GlobalVariables globally = new();

                                                GlobalApproach.PushDataToHost(c.Channel, new()
                                                {
                                                    Version = sVersion,
                                                    Production = bProduction,
                                                    MachineNo = sMachineNo,
                                                    ReportDateTime = globally.NowTime,
                                                    Row = production
                                                });
                                                production.Clear();

                                                GlobalApproach.PushDataToHost(c.Channel, new()
                                                {
                                                    Version = sVersion,
                                                    Production = bProduction,
                                                    MachineNo = sMachineNo,
                                                    ReportDateTime = globally.NowTime,
                                                    Row = status
                                                });
                                                status.Clear();

                                                GlobalApproach.PushDataToHost(c.Channel, new()
                                                {
                                                    Version = sVersion,
                                                    Production = bProduction,
                                                    MachineNo = sMachineNo,
                                                    ReportDateTime = globally.NowTime,
                                                    Row = signal
                                                });
                                                signal.Clear();
                                            }
                                        });
                                    });
                                    break;

                                case "1"://产量变化时采集上报参数

                                    //尝试寻找产量参数 - 01
                                    bool IsQtyUpdate = false;
                                    MessageBox qtybox = b.MessageBoxes.Find(c => c.PickPoints.Exists(p => p.AttribName == "01"));
                                    if (qtybox == null)
                                    {
                                        Console.WriteLine($"Can not found machine counter setting for machine '{b.MachineNo}'!");
                                        IsQtyUpdate = false;
                                    }
                                    else
                                    {
                                        //取产量参数并对比是否发生变化
                                        qtybox.PickPoints.ForEach(p =>
                                        {
                                            string sKey = b.MachineNo + "#" + p.AttribName;

                                            if (RowBox.ContainsKey(sKey))
                                            {
                                                if (!HistoryBox.ContainsKey(sKey)) IsQtyUpdate = true;
                                                else
                                                {
                                                    if (RowBox[sKey] != HistoryBox[sKey]) IsQtyUpdate = true;
                                                    else IsQtyUpdate = false;
                                                }
                                            }
                                        });
                                    }
                                    await Task.Run(() =>
                                    {
                                        bool bProduction = b.Production;
                                        string sMachineNo = b.MachineNo, sVersion = b.Vesion;
                                        //Production处理
                                        List<Parameter> production = new();
                                        //Status处理
                                        List<Parameter> status = new();

                                        b.MessageBoxes.ForEach(c =>
                                        {
                                            if (c.FunctionCode != 3) return;
                                            bool shouldPush = false;

                                            c.PickPoints.ForEach(p =>
                                            {
                                                string sKey = sMachineNo + "#" + p.AttribName;

                                                if (RowBox.ContainsKey(sKey))
                                                {
                                                    if (!HistoryBox.ContainsKey(sKey))
                                                    {
                                                        string _temp = Convert.ToString(RowBox[sKey]);
                                                        HistoryBox.AddOrUpdate(sKey, _temp, (key, value) => _temp);
                                                        shouldPush = true;
                                                    }
                                                    else
                                                    {
                                                        if (RowBox[sKey] != HistoryBox[sKey])
                                                        {
                                                            switch (c.Channel)
                                                            {
                                                                case HostChannel.Production:
                                                                    if (!IsQtyUpdate) break;//如果产量无变化，此次不更新上报

                                                                    production.Add(new()
                                                                    {
                                                                        AttribNo = p.AttribName,
                                                                        AttribValue = RowBox[sKey]
                                                                    });
                                                                    HistoryBox[sKey] = RowBox[sKey];
                                                                    shouldPush = true;
                                                                    break;

                                                                case HostChannel.Status:
                                                                    status.Add(new()
                                                                    {
                                                                        AttribNo = p.AttribName,
                                                                        AttribValue = RowBox[sKey]
                                                                    });
                                                                    HistoryBox[sKey] = RowBox[sKey];
                                                                    shouldPush = true;
                                                                    break;
                                                                default:
                                                                    HistoryBox[sKey] = RowBox[sKey];
                                                                    shouldPush = true;
                                                                    break;
                                                            }

                                                        }
                                                    }
                                                }
                                            });

                                            if (shouldPush)
                                            {
                                                GlobalVariables globally = new();

                                                GlobalApproach.PushDataToHost(c.Channel, new()
                                                {
                                                    Version = sVersion,
                                                    Production = bProduction,
                                                    MachineNo = sMachineNo,
                                                    ReportDateTime = globally.NowTime,
                                                    Row = production
                                                });
                                                production.Clear();

                                                GlobalApproach.PushDataToHost(c.Channel, new()
                                                {
                                                    Version = sVersion,
                                                    Production = bProduction,
                                                    MachineNo = sMachineNo,
                                                    ReportDateTime = globally.NowTime,
                                                    Row = status
                                                });
                                                status.Clear();
                                            }
                                        });
                                    });
                                    break;
                            }
                        });
                    });

                    break;
                default:
                    await Task.Run(async () =>
                    {
                        //定位清零的Modbus点位
                        MachineShell machine = MachineBoxes.Find(m => m.MachineNo == context.Trigger.Key.Name.Split("_")[0]);
                        MessageBox box = machine.MessageBoxes.Find(b => b.PickPoints.Exists(p => p.AttribName == "ResetCounter"));

                        using (TcpClient client = new TcpClient(machine.Address, machine.Port))
                        {
                            using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                            {
                                //Just for test
                                //await master.WriteSingleRegisterAsync(box.SlaveAddress, box.StartAddress, 0);
                                LogBuilder.WriteLog(LogEventLevel.Information, $"[P1&P2] [sMMP => MODBUS] Reset [{box.StartAddress}] task running.");
                                await master.WriteSingleCoilAsync(box.SlaveAddress, box.StartAddress, true);
                                Thread.Sleep(1000);
                                await master.WriteSingleCoilAsync(box.SlaveAddress, box.StartAddress, false);
                                LogBuilder.WriteLog(LogEventLevel.Information, $"[P1&P2] [sMMP => MODBUS] Reset [{box.StartAddress}] task done.");
                            };
                            client.Dispose();
                        };

                        //MachineBoxes.ForEach(async b =>
                        //{

                        //    await Task.Run(() =>
                        //    {
                        //        int iPort = b.Port;
                        //        string sMachineNo = b.MachineNo, sAddress = b.Address;

                        //        lock (MachineSwitch) if (!MachineSwitch.ContainsKey(sMachineNo)) MachineSwitch.Add(sMachineNo, true);

                        //        b.MessageBoxes.ForEach(async c =>
                        //        {
                        //            try
                        //            {
                        //                if (c.PickPoints.Count == 0) return;

                        //                TcpClient client = new TcpClient(sAddress, iPort);

                        //                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                        //                {
                        //                    //Just for test
                        //                    await master.WriteSingleRegisterAsync(c.SlaveAddress, 2914, 0);
                        //                };
                        //                client.Dispose();
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                if (MachineSwitch[sMachineNo] == true)
                        //                {
                        //                    //webapi <==

                        //                    //Console.WriteLine($" No.{sMachineNo} => {e.Message}");
                        //                    LogBuilder.WriteLog(LogEventLevel.Error, $" No.{sMachineNo} => {e.Message}");
                        //                }

                        //                MachineSwitch[sMachineNo] = false;
                        //            }
                        //        });

                        //    });
                        //});
                    });
                    break;

                //case "TEST":
                //default:
                //    return Console.Out.WriteLineAsync($"{DateTime.Now.ToString()}: TEST...");
                    //break;
            }
        }

        private static float[] GetRegistersByNModbus(ushort[] inputs, ushort startAddress)
        {
            float[] floatArray = new float[] { };
            switch (inputs.Length)
            {
                case 1://Int
                    floatArray = new float[] { inputs[0] };
                    break;

                case 2://Float
                    int startReg = int.Parse(startAddress.ToString());
                    List<float> floatList = new();
                    floatList.Add(ModbusUtility.GetSingle(inputs[inputs.Length - 1], inputs[0]));
                    floatArray = floatList.ToArray();
                    break;
            }
            return floatArray;
        }

        //20221020 - 读取RFID时使用，转LONG型
        private static double GetRegistersByNModbus(ushort[] inputs, ushort startAddress, string AttribType)
        {
            double value = 0;
            //Just DEMO Mark
            //List<byte> result = new();
            //result.AddRange(BitConverter.GetBytes(inputs[1]));
            //result.AddRange(BitConverter.GetBytes(inputs[0]));
            //value = BitConverter.ToInt32(result.ToArray(), 0);

            List<byte> result = new();
            result.AddRange(BitConverter.GetBytes(inputs[0]));
            result.AddRange(BitConverter.GetBytes(inputs[1]));
            value = BitConverter.ToInt32(result.ToArray(), 0);
            LogBuilder.WriteLog(LogEventLevel.Information, $"[P1&P2] [MODBUS => sMMP] Address = {startAddress} Value is {value}.");


            return value;
        }
        private static Timer Timer { get; set; }
        private static TimerCallback Callback { get; set; }

        //private static readonly Dictionary<string, bool> MachineSwitch = new();

        private static readonly ConcurrentDictionary<string, dynamic> HistoryBox = new();

        private static readonly ConcurrentDictionary<string, dynamic> RowBox = new();
        public static List<MachineShell> MachineBoxes = new();

        private static string RFIDRule(string _MoldingCode)
        {
            string MoldingCode = "";
            //第一码规则:1 = 入库；2 = 出库
            //第二码规则:1 = M； 2 = Z(后期自定义扩展)
            //第三~七码表示原始编码
            string _First = _MoldingCode.Substring(0, 1);
            string _Second = _MoldingCode.Substring(1, 1) switch
            { 
                "1" => "M",
                "2" => "Z",
               _=> _MoldingCode.Substring(1, 1)
            };
            string _Remian = _MoldingCode.Substring(2, 5);

            MoldingCode = _First + _Second + _Remian;
            LogBuilder.WriteLog(LogEventLevel.Information, $"[sMMP] RFID Code read as '{MoldingCode}'.");
            return MoldingCode;
        }
    }

    internal class WriteValue : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                LogBuilder.WriteLog(LogEventLevel.Information, "[P1&P2] [sMMP => MODBUS] Update data task running.");
                //遍历参数
                MachineBoxes.ForEach(async b =>
                {

                    await Task.Run(() =>
                    {
                        int iPort = b.Port;
                        string sMachineNo = b.MachineNo, sAddress = b.Address;

                        b.MessageBoxes.ForEach(async c =>
                        {
                            try
                            {
                                if (c.PickPoints.Count == 0) return;

                                TcpClient client = new TcpClient(sAddress, iPort);

                                using (ModbusIpMaster master = ModbusIpMaster.CreateIp(client))
                                {
                                    //Just for test
                                    await master.WriteSingleRegisterAsync(c.SlaveAddress, 2914, 0);
                                };
                                client.Dispose();
                            }
                            catch (Exception e)
                            {
                            }
                        });

                    });
                });
            });
        }

        public static List<MachineShell> MachineBoxes = new();
    }
}