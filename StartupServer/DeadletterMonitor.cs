using Akka.Actor;
using Akka.Event;

namespace StartupServer 
{
    public class DeadletterMonitor : ReceiveActor
    {
        public DeadletterMonitor()
        {

             Receive<DeadLetter>(dl => System.Console.WriteLine("dead message content"+dl.Message));
        }
    }
}