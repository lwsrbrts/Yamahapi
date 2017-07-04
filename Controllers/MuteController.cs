using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Yamahapi.Controllers
{
    [Route("api/[controller]")]
    public class MuteController : Controller
    {

        private IConfiguration Configuration { get; set; }

        public MuteController(IConfiguration configuration)
        {            
            Configuration = configuration;

            //Configuration.GetValue<string>("Receiver:ReceiverIP");
        }


        // Power on the receiver.
        [HttpPut]
        public async Task<IActionResult> MuteOn()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                    client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent("<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Volume><Mute>On</Mute></Volume></Main_Zone></YAMAHA_AV>"));
                    response.EnsureSuccessStatusCode();
                    return Accepted();

                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Dodgy request: {httpRequestException.Message}");
                }
            }
        }

        // Power off the receiver.
        [HttpDelete]
        public async Task<IActionResult> MuteOff()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                    client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent("<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Volume><Mute>Off</Mute></Volume></Main_Zone></YAMAHA_AV>"));
                    response.EnsureSuccessStatusCode();
                    return Accepted();

                }
                catch (HttpRequestException httpRequestException)
                {
                    return BadRequest($"Dodgy request: {httpRequestException.Message}");
                }
            }
        }

    }
}
