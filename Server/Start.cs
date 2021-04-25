using Common.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.GlobalConfig;

namespace Server
{
    class Start
    {
        static void Main(string[] args)
        {
            var ip = GlobalConfig.DefaultGateway;
            var port = GlobalConfig.Port;

            if (args.Length > 1) 
            {
                ip = args[1];
                if (args.Length == 3) 
                {

                    port = int.Parse(args[2]);
                }
            }

            var server = new Models.Server(ip, port);

            server.StartServerListener();
        }
    }

    
    
}
