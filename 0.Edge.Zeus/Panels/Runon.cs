using Edge.Zeus.Controllers;
using Edge.Zeus.Models;
using Lib.Common.Components.Agreements;
using Lib.Common.Manager;
using Lib.Common.Components.Func;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using SoapCore;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using Quartz;
using Quartz.Impl;

namespace Edge.Zeus.Panels
{
    public class Runon
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.TryAddSingleton<ISOAP, WebController>();
            services.AddSoapExceptionTransformer((e) => e.Message);
            services.AddSoapCore();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "sMMP.Edge", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Add GC Collection
            app.UseGCMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "sMMP.Edge v1"));
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSoapEndpoint<ISOAP>(Config.GetValue<string>("Server:WebService:Path"), new BasicHttpBinding(), SoapSerializer.DataContractSerializer);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapPost(Config.GetValue<string>("Server:WebApi:Path"), async context =>
                {
                    using StreamReader streamReader = new(context.Request.Body);

                    string sFileName = Config.GetValue<string>("Server:Protector");

                    bool reboot = File.Exists(AppDomain.CurrentDomain.BaseDirectory + sFileName);

                    string value = await streamReader.ReadToEndAsync();

                    string result;

                    if (GlobalApproach.LocalBuilder(new FoundationWriter(), context.Connection.RemoteIpAddress, value))
                    {
                        result = JsonConvert.SerializeObject(new ResponseRoot()
                        {
                            IsOk = "1",
                            Result = new()
                            {
                                ErrorCode = "",
                                Exception = new()
                                {
                                    Message = reboot.ToString()
                                }
                            }
                        });
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(new ResponseRoot()
                        {
                            IsOk = "0",
                            Result = new()
                            {
                                ErrorCode = "100",
                                Exception = new()
                                {
                                    Message = "Write failed"
                                }
                            }
                        });
                    }

                    await context.Response.WriteAsync(result);

                    FoundationProvider.ReadDocument();

                    if (!reboot) return;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = sFileName,
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                    });
                });
            });
        }

        public Runon(IConfiguration configuration)
        {
            Config = configuration;
        }

        public IConfiguration Config { get; }
    }
}