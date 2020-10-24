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
            //BootStarpLoader = Context.ActorOf(Props.Create<BootStrapLoader>(Configurations.ServerAddress,Configurations.PubKey,Configurations.MyAddress),"BootStarpLoader");
            BootStarp();
        }

        public ConsensusNode(Configs configs)
        {
            Configurations = configs;

            
            Receive<ClientTransaction>(Message => TransactionHandler.Tell(Message) );
            
            Receive<PBFTMessage>(Message => ConsensusStarted,Message => 
            {
                System.Console.WriteLine("Received "+ Message.MsgType+" Message ");
                CMessageHandler.Tell(Message);
            });
            
            //should be improved 
            Receive<IActorRef>(Message =>
            {
                if (Message.Path.ToString().Contains("Log"))
                {
                    System.Console.WriteLine("------------------------------------------");
                    System.Console.WriteLine("Node Is Ready To receive Client Transaction");
                    System.Console.WriteLine("------------------------------------------");
                    Initialize(Message);

                }
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
            Context.ActorSelection($"akka.tcp://VotingSystem@{Configurations.ServerAddress}:5005/user/VotingServerActor/RegistrationHandler").Tell(Register);  
        }

        private void Initialize(IActorRef Message)
        {
            
                Log =Message;
                TransactionHandler =Context.ActorOf(Props.Create<TransactionHandler>(Log,Configurations.ClientMembers),"TransactionHandler");
                //test
                PopulateTXpool();
                Primary = Context.ActorOf(Props.Create<PrimaryActor>(Configurations,BlockchainBroker,Log),"PrimaryActor");
                Primary.Tell(new UpdateSequenceNumber(1));
                
        }















        private void PopulateTXpool()
        {
                string ss= "ddjfhsldf";
                string jalile ="sflhdslf";
                string bilale ="bilale jalalati";
                string ahmad ="winrar ";
                string kayane="mohammed hijab";
                string sam="harris";
                string william="lane craig";
                string alghazali="kalam cosmological";
                Sha256 sha = new Sha256();
                
                byte[] data= sha.Hash(Encoding.UTF8.GetBytes(ss));
                byte[] data2= sha.Hash(Encoding.UTF8.GetBytes(jalile));
                byte[] data3=sha.Hash(Encoding.UTF8.GetBytes(bilale));
                byte[] data4=sha.Hash(Encoding.UTF8.GetBytes(ahmad));
                byte[] data5=sha.Hash(Encoding.UTF8.GetBytes(kayane));
                byte[] data6=sha.Hash(Encoding.UTF8.GetBytes(sam));
                byte[] data7=sha.Hash(Encoding.UTF8.GetBytes(william));
                byte[] data8=sha.Hash(Encoding.UTF8.GetBytes(alghazali));

                Self.Tell(new ClientTransaction(data,CandidtaeList.alghazali));
                Self.Tell(new ClientTransaction(data2,CandidtaeList.ibnrochd));
                Self.Tell(new ClientTransaction(data4,CandidtaeList.alghazali));
                Self.Tell(new ClientTransaction(data5,CandidtaeList.alghazali));
                Self.Tell(new ClientTransaction(data6,CandidtaeList.alghazali));

                Self.Tell(new ClientTransaction(data7,CandidtaeList.ibncina));
                Self.Tell(new ClientTransaction(data8,CandidtaeList.ibncina));
                
        }
    }
    
}