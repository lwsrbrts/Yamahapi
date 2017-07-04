using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;


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

        // GET api/volume
        [HttpGet]
        public async Task<string> GetVolume()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                    client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent("<YAMAHA_AV cmd=\"GET\"><Main_Zone><Basic_Status>GetParam</Basic_Status></Main_Zone></YAMAHA_AV>"));

                    //var contents = await response.Content.ReadAsStringAsync();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                  
                    var XmlDoc = XDocument.Parse(Encoding.UTF8.GetString(bytes));
                    
                    return XmlDoc.Root.Element("Main_Zone").Element("Basic_Status").Element("Volume").Element("Lvl").Element("Val").Value;

                }
                catch (HttpRequestException httpRequestException)
                {
                    return httpRequestException.Message;
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    return invalidOperationException.Message;
                }
            }
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
