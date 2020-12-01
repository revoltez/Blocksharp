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
            Configs configs =new Configs(IPAddress.Parse(GetServerIpAddress()));

            System.Console.WriteLine("----------------------------------------");

            Console.WriteLine("PBFT node Running at Ip Address : "+configs.MyAddress);
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("-----------------------------------------------------");
            
            string ConfigHead = @"
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
            string ConfigTail=@"
                        }
                    }
                }";

            string ActorsystemConfig= string.Concat(ConfigHead,Hostname,ConfigTail);

            var MySystem = ActorSystem.Create("Blocksharp",ActorsystemConfig);
            IActorRef CNode=MySystem.ActorOf(Props.Create<ConsensusNode>(configs),"ConsensusNode");
            
            Console.WriteLine("Press Ctrl + C to cancel!");
            
            Console.CancelKeyPress += ((s, a) =>
            {
               
                Console.WriteLine("Shuting Down Consensus Node");
                _closingEvent.Set();
            });
            _closingEvent.WaitOne();
        
        }

        public static string GetServerIpAddress()
        {
            IPAddress iP;
            string address="";
            bool loop = true;
            Console.WriteLine("would you please Enter the Server Address");
            System.Console.WriteLine("---------------------------------------");

            while (loop)
            {
                address =Console.ReadLine();
                if(IPAddress.TryParse(address,out iP))
                {
                    loop =true;
                }
                else
                {
                    System.Console.WriteLine("please Eneter a valid ip Address");
                } 
            }
            
            return address;
        }

    }
}
