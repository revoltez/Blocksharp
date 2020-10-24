using Akka.Actor;
using System;
using NSec.Cryptography;
using System.Collections.Generic;
using Akka.Configuration;
using System.Net;
using System.Threading;

namespace PBFT
{
    class Program
    {
        private static readonly AutoResetEvent _closingEvent = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            Console.WriteLine("would you please Enter the Server Address");
            System.Console.WriteLine("---------------------------------------");
            var address =Console.ReadLine();
            System.Console.WriteLine("----------------------------------------");
            Configs configs =new Configs(IPAddress.Parse(address));

            Console.WriteLine("PBFT node Running at Ip Address : "+configs.MyAddress);
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("-----------------------------------------------------");
            
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
                            port = 5000";
            
            string Hostname=$"\n hostname = {configs.MyAddress}";
            string Tail=@"
                        }
                    }
                }";

            string ConcatinatedString = string.Concat(UpperHalf,Hostname,Tail);

            // ""akka.tcp://ConsensusNode@"+configs.MyAddress+@":6969"",
            
            var MyVotingSystem = ActorSystem.Create("System",ConcatinatedString);
            IActorRef CNode=MyVotingSystem.ActorOf(Props.Create<ConsensusNode>(configs),"ConsensusNode");
            
            Console.WriteLine("Press Ctrl + C to cancel!");
            
            Console.CancelKeyPress += ((s, a) =>
            {
               
                Console.WriteLine("Shuting Down Consensus Node");
                _closingEvent.Set();
            });
            _closingEvent.WaitOne();
        
        }
    }
}
