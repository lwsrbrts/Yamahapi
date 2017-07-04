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
    public class VolumeController : Controller
    {

        private IConfiguration Configuration { get; set; }

        public VolumeController(IConfiguration configuration)
        {            
            Configuration = configuration;
        }

        // Set the volume.
        [HttpPut("{vol}")]
        public async Task<IActionResult> SetVolume(double vol)
        {
            // Ensure the volume is within an acceptable range.
            if (vol < 20.0 || vol > 80.0) {
                return BadRequest("The volume specified must be between 20 and 80");
            }
            // Multiply the value by 10 to get the decibel value accepted by the receiver.
            vol = vol * 10;

            using (var client = new HttpClient())
            {
                try
                {
                    var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                    client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Volume><Lvl><Val>-{vol}</Val><Exp>1</Exp><Unit>dB</Unit></Lvl></Volume></Main_Zone></YAMAHA_AV>"));
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
