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
    [Route("api/inputs")] // Had to force this because it said Inputs controller was already in use...it isn't unless hidden from me but hey, what do I know?
    public class Inputs2Controller : Controller
    {

        private IConfiguration Configuration { get; set; }

        public Inputs2Controller(IConfiguration configuration)
        {            
            Configuration = configuration;
        }

        // Get a list of valid input names from the receiver.
        [HttpGet]
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

    }
}
