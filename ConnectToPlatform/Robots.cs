﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectToPlatform
{
    public class Robots
    {

        public string LicenseKey { get; set; }
        public string MachineName { get; set; }
        public long MachineId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string HostingType { get; set; }
        public string Password { get; set; }
        public string CredentialType { get; set; }
        public string RobotEnvironments { get; set; }
        public string Id { get; set; }
        public string ExecutionSettings { get; set; }


    }


    public class ModifiedRobot
    {
        public string MachineName { get; set; }
        public long MachineId { get; set; }
     //   public string Id { get; set; }
    }

    public class Machines
    {
        public string MachineName { get; set;  }
        public long MachineId { get; set;  }
        public int Identifier { get; set;  }
    }
}
