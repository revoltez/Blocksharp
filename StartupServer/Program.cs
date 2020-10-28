using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;

namespace StartupServer
{
    class Program
    {

        private static readonly AutoResetEvent _closingEvent = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            
            string hostname=Dns.GetHostName();
            var ip=Dns.GetHostEntry(hostname);
            var Address=ip.AddressList[0];

            Console.WriteLine("***************************");
            Console.WriteLine("Server Running At Address :"+Address);
            Console.WriteLine("***************************");
            
            string UpperHalf = @"
                akka {
                    actor {
                        provider = remote
                        serializers {
                            hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                        }
                        serialization-bindings {
                            ""System.Object"" = hyperion
                        }
                    }
                    remote {
                        helios.tcp { 
                            port = 5005";
            
            string Hostname=$"\n hostname = {Address}";
            string Tail=@"
                        }
                    }
                }";

            string ConcatinatedString = string.Concat(UpperHalf,Hostname,Tail);
            var hocon=ConfigurationFactory.ParseString(ConcatinatedString);
            /*var hocon = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = remote
                    }
                    remote {
                        helios.tcp { 
                            port = 5005 
                            hostname = 127.0.0.1
                        }
                    }
                }
            ");*/

            
            
            var System = ActorSystem.Create("Blocksharp",hocon);
            var Server= System.ActorOf(Props.Create(()=> new ServerActor()),"ServerActor"); 
            
            // Setup an actor that will handle deadletter type messages
            var deadletterWatchMonitorProps= Props.Create(() => new DeadletterMonitor());
            var deadletterWatchActorRef = System.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor"); 
            
            System.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));  





            Console.WriteLine("Press Ctrl + C to cancel!");
            
            Console.CancelKeyPress += ((s, a) =>
            {
                Console.WriteLine("Bye!");
                _closingEvent.Set();
            });
            _closingEvent.WaitOne();




            Console.ReadLine();
        }
    }

}
