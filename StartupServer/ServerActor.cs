using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using NetwrokMessages;

namespace StartupServer
{
    public class ServerActor : ReceiveActor
    {
        
        public ServerActor()
        {
            var RegistrationHandler= Context.ActorOf(Props.Create<RegistrationHandler>(),"RegistrationHandler");
            System.Console.WriteLine("server Actor started");        
            System.Console.WriteLine(Self);

            Receive<RegistrationRequest>(msg => RegistrationHandler.Tell(msg));

        }
    }
}
