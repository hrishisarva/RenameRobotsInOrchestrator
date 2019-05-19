using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Linq;

namespace jsonplay
{
    public class Machines
    {
        public string MachineName { get; set; }
        public long MachineId { get; set; }
        public int Identifier { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {

            // Read entire text file content in one string  
            string text = File.ReadAllText(@"c:\users\hrish sarva\source\repos\RenameMachine\jsonplay\machine.json");
            // Console.WriteLine(text);

            List<Machines> Machines = new List<Machines>();

            var robotsDetails = JObject.Parse(text)["value"]
            .ToList();

            int i = 1;

            Console.WriteLine("Identifier\t\tMachine Name\t\t\t MachineId");

            foreach(var token in robotsDetails)

            {
                Machines mm = new Machines();
                mm.Identifier = i++; 
                mm.MachineName =
                token.Value<string>("Name");

                mm.MachineId  =
                token.Value<long>("Id");

                Console.WriteLine("{0}\t\t{1}\t\t\t{2}", mm.Identifier, mm.MachineName, mm.MachineId);
                Machines.Add(mm);

            }




            Console.Read();

            //JObject rss = JObject.Parse(jsonResponse);

            //string token = (string)rss["result"];

        }
    }
}





