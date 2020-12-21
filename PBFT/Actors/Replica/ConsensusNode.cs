using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Event;
using ConsensusMessages;
using NetwrokMessages;
using NSec.Cryptography;

namespace PBFT
{
    class ConsensusNode : ReceiveActor
    {
        public bool ConsensusStarted{ get; set;} = false;
        private readonly ILoggingAdapter log = Context.GetLogger();
        public Configs Configurations {get; private set;}
        
        IActorRef Log,CMessageHandler,BlockchainBroker,TransactionHandler,Primary;
        
        protected override void PreStart()
        {
            BlockchainBroker = Context.ActorOf(Props.Create<BlockchainBroker>(),"BlockchainBroker");
            BootStarp();
        }

        public ConsensusNode(Configs configs)
        {
            Configurations = configs;

            Receive<ClientTransaction>(Message => TransactionHandler.Tell(Message) );
            
            //  function contains two parameters, the first accepts a predicate and the second is the body, both params are lambda expressions
            Receive<PBFTMessage>(Message => ConsensusStarted, Message => 
            {
                System.Console.WriteLine("Received "+ Message.MsgType+" Message ");
                CMessageHandler.Tell(Message);
            });
            
            //should be improved 
            Receive<LogBrokerCreated>(Message =>
            {
                    System.Console.WriteLine("------------------------------------------");
                    System.Console.WriteLine("Node Is Ready To receive Client Transaction");
                    System.Console.WriteLine("------------------------------------------");
                    Initialize(Message.LogRef);

            });
             
            Receive<MembersList>(Message => 
            {
                System.Console.WriteLine("---------------------------------");
                System.Console.WriteLine("Members list received from Server");
                System.Console.WriteLine("---------------------------------");

                ConsensusStarted = true;
                Configurations.Members = new SortedList<string,byte[]>(Message.Members);
                Configurations.ReplicaRefrences = new List<IActorRef>(Message.MembersReferences);
                CMessageHandler = Context.ActorOf(Props.Create<ConsensusMessageHandler>(Configurations,BlockchainBroker),"ConsensusMessageHandler");

                foreach (var item in Configurations.Members)
                {
                        System.Console.WriteLine("Received: "+BitConverter.ToString(item.Value)+" : "+item.Key);
                }
            });
        }

        public void BootStarp()
        {
            var Register= new RegistrationRequest(Configurations.PubKey,Configurations.MyAddress);
            Context.ActorSelection($"akka.tcp://Blocksharp@{Configurations.ServerAddress}:5005/user/ServerActor/RegistrationHandler").Tell(Register);  
        }

        private void Initialize(IActorRef LogRef)
        {
                Log =LogRef;
                TransactionHandler =Context.ActorOf(Props.Create<TransactionHandler>(Log,Configurations.ClientMembers),"TransactionHandler");
                //test
                TestData.PopulateTXpool(Self); 
                Primary = Context.ActorOf(Props.Create<PrimaryActor>(Configurations,BlockchainBroker,Log),"PrimaryActor");
                Primary.Tell(new UpdateSequenceNumber(1));
        }
    }
    
}