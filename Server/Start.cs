using Common.Config;

namespace Server
{
    static class Start
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
