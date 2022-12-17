using Edge.Zeus.Models;
using Lib.Common.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Edge.Zeus.Controllers
{
    [ApiController]
    public class EqxController : ControllerBase
    {
        [HttpPost("Default")]
        [Produces("application/json")]
        [Route("[controller]/[action]")]
        public async System.Threading.Tasks.Task<string> BatomAsync([FromBody] BatomRoot req)
        {
            string result;

            try
            {
                result = JsonConvert.SerializeObject(new ResponseRoot()
                {
                    IsOk = "1"
                });

                GlobalVariables globally = new();

                var builder = new EaiServiceSOAP.IntegrationEntryClient(EaiServiceSOAP.IntegrationEntryClient.EndpointConfiguration.IntegrationEntry, GlobalVariables.EaiUrl);
                await builder.invokeSrvAsync(null);
            }
            catch (Exception e)
            {
                result = JsonConvert.SerializeObject(new ResponseRoot()
                {
                    IsOk = "0",
                    Result = new()
                    {
                        ErrorCode = "",
                        Exception = new()
                        {
                            Message = e.Message
                        }
                    }
                });
            }

            return result;
        }

        private readonly ILogger<EqxController> Logger;

        public EqxController(ILogger<EqxController> logger)
        {
            Logger = logger;
        }
    }
}
