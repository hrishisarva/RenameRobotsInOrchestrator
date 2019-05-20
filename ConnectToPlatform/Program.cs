using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/*
 
Pseudocode:
Prompt user - Script to replace all matching Robot MachineName and MachineId with replacement MachineName and MachineId that administrator has created

Prompt user for old Machine name - set as variable oldMachineName

Prompt user for new Machine name - set as variable newMachineName

Call /odata/Machines
   get Id for newMachineName - set as newMachineId

For every Robot with MachineName = oldMachineName
   Set MachineName = newMachineName
   Set MachineID = newMachineID

https://platform.uipath.com/swagger/ui/index#/
*/

namespace ConnectToPlatform
{

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent iContent)
        {
            try
            {
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, requestUri)
                {
                    Content = iContent
                };

                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await client.SendAsync(request);
                }
                catch (TaskCanceledException e)
                {
                    Console.WriteLine("ERROR: " + e.ToString());
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                throw;
            }
        }
    }

    public class Program
    {

        static void Main(string[] args)
        {

            string tenantName = string.Empty;
            Console.WriteLine("Enter Tenant name : ");
            tenantName = Console.ReadLine();

            string emailId = string.Empty;
            Console.WriteLine("Enter EmailID to login to Orchestrator : ");
            emailId = Console.ReadLine();

            string pwd = string.Empty;
            Console.WriteLine("Enter password to login to orchestrator : ");
            pwd = Console.ReadLine();


            //api call to get auth token 
            var jsonResponse = PostAuthenticateToUiPathPlatform(tenantName, emailId, pwd);

            // Console.WriteLine("Authentication from Orchestrator ");
            dynamic parsedJson = JsonConvert.DeserializeObject(jsonResponse);
            //Console.Write(JsonConvert.SerializeObject(parsedJson, Formatting.Indented));
            //Console.WriteLine("\n\n\n ");
            var token = ExtractAuthenticationKey(jsonResponse);


            /****************************/
            // api to get machines 
            //Console.WriteLine("\n\n\n ");
            var responseFromGetMachines = GetMachines(token);

            // Console.WriteLine("Response from Get Machines Call ");
            parsedJson = JsonConvert.DeserializeObject(responseFromGetMachines.Result);
            // Console.Write(JsonConvert.SerializeObject(parsedJson, Formatting.Indented));

            var machinesInReadableWay = PresentMachinesInAReadableWay(responseFromGetMachines.Result);

            if (machinesInReadableWay.Count <= 0)
            {
                return;
            }

            string oldMachineName = string.Empty;
            long oldMachineId = 0;
            string NewMachineName = string.Empty;
            long NewMachineId = 0;


            var OldMachineDetails = SelectOldMachineDetails();
            var oldM = new Machines();
            foreach (var i in machinesInReadableWay)
            {
                if (i.Identifier == OldMachineDetails)
                {
                    oldM.MachineName = i.MachineName;
                    oldM.MachineId = i.MachineId;
                    oldMachineName = i.MachineName;
                    oldMachineId = i.MachineId;
                    break;
                }
            }
            var NewMachineDetails = SelectNewMachineDetails();

            var newM = new Machines();
            foreach (var i in machinesInReadableWay)
            {
                if (i.Identifier == NewMachineDetails)
                {
                    newM.MachineName = i.MachineName;
                    newM.MachineId = i.MachineId;
                    NewMachineName = i.MachineName;
                    NewMachineId = i.MachineId;
                    break;
                }
            }

            /****************/

            Console.WriteLine("\nOld Machine Name  {0} , Old Machine Id {1} ", oldMachineName, oldMachineId);

            Console.WriteLine("\nNew Machine Name  {0} , New Machine Id {1} ", NewMachineName, NewMachineId);

            /******************/

            //Console.WriteLine("\n\n\n ");
            var responseFromGetRobots = GetRobots(token);
            //Console.WriteLine("Response from Get Robots Call ");
            //parsedJson = JsonConvert.DeserializeObject(responseFromGetRobots.Result);
            //Console.Write(JsonConvert.SerializeObject(parsedJson, Formatting.Indented));

            Console.WriteLine("\nExtracting robots information from GetRobots response");
            var robotsToBeUpdated = GetRobotsDetails(responseFromGetRobots.Result, oldMachineName, oldMachineId);

            if (robotsToBeUpdated.Count > 0)
            {
                Console.WriteLine("\n");
                //UpdateRobotsWithNewMachineInfo(robotsToBeUpdated, token, newMachineName, newMachineId);
                UpdateRobotsWithNewMachineInfo(robotsToBeUpdated, token, NewMachineName, NewMachineId);

            }
            else
            {
                Console.WriteLine("\nNo robots are associated with your selection of Old Machine ");
            }
            Console.WriteLine("Press any key to exit ....");
            Console.ReadLine();
        }

        private static List<Machines> PresentMachinesInAReadableWay(string responseFromGetMachines)
        {

            try
            {
                Console.WriteLine("\nPlease select the identifier ");

                string text = responseFromGetMachines;

                List<Machines> Machines = new List<Machines>();

                var machineDetails = JObject.Parse(text)["value"]
                .ToList();

                int i = 1;

                Console.WriteLine("Identifier\t\tMachine Name\t\t\t MachineId");

                foreach (var token in machineDetails)

                {
                    Machines mm = new Machines();
                    mm.Identifier = i++;
                    mm.MachineName =
                    token.Value<string>("Name");

                    mm.MachineId =
                    token.Value<long>("Id");

                    Console.WriteLine("{0}\t\t{1}\t\t\t{2}", mm.Identifier, mm.MachineName, mm.MachineId);
                    Machines.Add(mm);

                }

                return Machines;

            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                throw;
            }

        }

        public static void UpdateRobotsWithNewMachineInfo(List<Robots> robotsTobeUpdated, string token, string newMachineName, long newMachineId)
        {
            try
            {
                foreach (var robotTobeUpdated in robotsTobeUpdated)
                {
                    string x = UpdateRobotsWithNewMachineName(token, robotTobeUpdated, newMachineName, newMachineId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                throw;
            }
        }

        public static string PostAuthenticateToUiPathPlatform(string tenantName, string emailID, string password)
        {

            Console.WriteLine("Checking your credentials with Orchestrator ...");
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection
                {
                    ["tenancyName"] = tenantName,
                    ["usernameOrEmailAddress"] = emailID,
                    ["password"] = password
                };

                var response = wb.UploadValues(ApiDetails.GetAuthenticationUrl(), "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);

                return responseInString;

            }
        }

        public static string ExtractAuthenticationKey(string jsonResponse)
        {
            try
            {
                JObject rss = JObject.Parse(jsonResponse);

                string token = (string)rss["result"];

                //  Console.Write(token);

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                return null;

            }

        }

        public async static Task<string> GetRobots(string token)
        {

            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetStringAsync(ApiDetails.GetRobotsUrl());

                return response.ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                return null;
            }

        }

        public static List<Robots> GetRobotsDetails(string robotResponse, string oldMachineName, long oldMachineId)
        {
            try
            {
                JObject rss = JObject.Parse(robotResponse);
                var robotsDetails = JObject.Parse(robotResponse)["value"]
                .ToList();

                //JObject jalbum = robotsDetails[0] as JObject;
                //// Copy to a static Album instance
                //Robots album = jalbum.ToObject<Robots>();

                List<Robots> RobotsList = new List<Robots>();

                foreach (var robotdetail in robotsDetails)
                {
                    JObject Jrbt = robotdetail as JObject;
                    Robots rbt = Jrbt.ToObject<Robots>();
                    RobotsList.Add(rbt);
                }

                var x = RobotsList.
                        Where(a => a.MachineId == oldMachineId && a.MachineName == oldMachineName).ToList();

                Console.Write("Total Robots Count tobe updated in RainbowCafe " + x.Count);

                return x;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);


                return null;
            }
        }

        public static string UpdateRobotsWithNewMachineName(string token, Robots robot, string newMachineName, long newMachineId)

        {
            try
            {
                var modifiedRobot = new ModifiedRobot
                {
                    MachineId = newMachineId,
                    MachineName = newMachineName
                };

                var jsonString = JsonConvert.SerializeObject(modifiedRobot);

                HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                string newPatchurl = String.Concat(ApiDetails.PatchRobotDetailsUrl(), "(", robot.Id.ToString(), ")");

                Uri newPatchUri = new Uri(newPatchurl);

                var responseMessage = httpClient.PatchAsync(newPatchUri, httpContent).Result;

                return responseMessage.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                throw;
            }


        }

        public async static Task<string> GetMachines(string token)
        {

            try
            {
                Console.WriteLine("\n Getting Existing Machine details from Orchestrator .....");
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetStringAsync(ApiDetails.GetMachinesUrl());

                return response.ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack Trace : " + ex.StackTrace);

                throw;
            }

        }

        public static int SelectOldMachineDetails()
        {
            Console.WriteLine("\nSelect Old Machine Identifier ");
            var oldMachineDetails = Convert.ToInt32(Console.ReadLine());

            return oldMachineDetails;

        }

        public static int SelectNewMachineDetails()
        {
            Console.WriteLine("\nSelect New Machine Identifier ");
            var newMachineDetails = Convert.ToInt32(Console.ReadLine());

            return newMachineDetails;
        }

    }
}
