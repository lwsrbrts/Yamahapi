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
    public class InputController : Controller
    {

        private IConfiguration Configuration { get; set; }

        public InputController(IConfiguration configuration)
        {            
            Configuration = configuration;
        }

        // Get a list of valid input names from the receiver.
        public async Task<string[]> GetInputs()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                    client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent("<YAMAHA_AV cmd=\"GET\"><System><Config>GetParam</Config></System></YAMAHA_AV>"));

                    var bytes = await response.Content.ReadAsByteArrayAsync();
                  
                    var XmlDoc = XDocument.Parse(Encoding.UTF8.GetString(bytes));

                    var Inputs = XmlDoc.Root.Descendants("Input").Descendants();

                    List<string> InputList = new List<string>();
                    foreach (XElement input in Inputs) {
                        InputList.Add(input.Name.ToString().Replace("_", ""));
                    }
                    return InputList.ToArray();

                }
                catch (HttpRequestException httpRequestException)
                {
                    return new string[] {httpRequestException.Message};
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    return new string[] {invalidOperationException.Message};
                }
            }
        }

        // GET api/input - gets the current input from the receiver.
        [HttpGet]
        public async Task<string> GetInput()
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
                    
                    return XmlDoc.Root.Element("Main_Zone").Element("Basic_Status").Element("Input").Element("Input_Sel").Value;

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

        // Sets the input - assuming it's valid.
        [HttpPut("{input}")]
        public async Task<IActionResult> SetInput(string input)
        {
            // Ensure the input is possible
            List<string> checkValues = new List<string> { "HDMI1", "HDMI2", "HDMI3", "HDMI4", "AV4" };

            var Inputs = await GetInputs();

            if (Inputs.Contains(input.ToUpper())) {

                using (var client = new HttpClient())
                {
                    try
                    {
                        var ReceiverIP = Configuration.GetValue<string>("Receiver:ReceiverIP");
                        client.BaseAddress = new Uri($"http://{ReceiverIP}/");
                        var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Input><Input_Sel>{input.ToUpper()}</Input_Sel></Input></Main_Zone></YAMAHA_AV>"));
                        response.EnsureSuccessStatusCode();
                        return Accepted();

                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        return BadRequest($"Dodgy request: {httpRequestException.Message}");
                    }
                }
            }
            else return BadRequest("Invalid input selected.");
        }


    }
}
