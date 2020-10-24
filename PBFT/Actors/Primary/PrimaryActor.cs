using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using ConsensusMessages;
using NSec.Cryptography;
using System.Linq;

namespace PBFT
{
    class PrimaryActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();

        public Configs Configurations{ get; set;}
        public int SequenceNumber{ get;  set; }
        public int ViewNumber{ get;  set; }=0;
        public bool IsPrimary{ get; set;} 
        public IActorRef BlockchainBroker,Log;

        public PrimaryActor  (Configs configs,IActorRef blockchainBroker,IActorRef lg )
        {
            
            Log=lg;  
            BlockchainBroker=blockchainBroker;
            Configurations=configs;

            Receive<UpdateSequenceNumber>(Message => 
            {
                SequenceNumber= Message.SequenceNum;

                System.Console.WriteLine("*********************************");
                System.Console.WriteLine("Current sequence number is "+SequenceNumber);
                System.Console.WriteLine("*********************************");
                
                if(CheckPrimary()) RequestBlock(Context);
            });

            Receive<UpdateViewNumber>(Message =>
            {
                ViewNumber++;
                System.Console.WriteLine("************************");
                System.Console.WriteLine("New View, view Number : "+ViewNumber);
                System.Console.WriteLine("************************");
                if(CheckPrimary()) RequestBlock(Context);
            });

            Receive<ActorIdentity>(Message =>
            {
            });

            Receive<BlockCreated>(Message => 
            {
                List<BlockTransaction> transactions= new List<BlockTransaction>();
                foreach (var item in Message.clientTransactions)
                {
                    transactions.Add(new BlockTransaction(item.Id,item.VoteTo));      
                }
                Block block = new Block(transactions,SequenceNumber,ViewNumber,Message.hash);
                // send the block received   
                foreach (var item in Configurations.ReplicaRefrences)
                {
                    item.Tell(block);   
                }
            });
        }

        public async void RequestBlock(IUntypedActorContext context)
        {
            
                var Response = await Log.Ask(new LogOpRequest(LogOps.CreateBlock)); 
                
                    if (Response is BlockCreated)
                    {
                        var createdBlock =  Response as BlockCreated;
                        List<BlockTransaction> transactions = new List<BlockTransaction>(); 
                        
                        foreach (var item in createdBlock.clientTransactions)
                        {
                            transactions.Add(new BlockTransaction(item.Id,item.VoteTo));      
                        }

                        Block bl = new Block(transactions,SequenceNumber,ViewNumber,createdBlock.hash);
                        PrePrepare prePrepare = new PrePrepare(bl,Configurations.PubKey,ViewNumber,SequenceNumber); 
                        prePrepare.Signature = SignatureAlgorithm.Ed25519.Sign(Configurations.Keys,prePrepare.GetBytes());                         
                        
                        System.Console.WriteLine("Sending New Block with Sequence And View Number "+prePrepare.SequenceNumber+": "+prePrepare.ViewNumber);
                       
                        foreach (var item in Configurations.ReplicaRefrences)
                        {
                            item.Tell(prePrepare);   
                        }
                    }
                    else
                    {
                        System.Console.WriteLine((Response as ErrorMessage).Reason);
                        //should start a timer and resend  
                    }
        }

        public bool CheckPrimary()
        {
            String index = BitConverter.ToString(Configurations.Members.Values[ViewNumber%Configurations.Members.Count]);
            String Pubkey = BitConverter.ToString(Configurations.PubKey);
            
                if (index.Equals(Pubkey))
                {
                    System.Console.WriteLine("Node is Primary");
                    IsPrimary=true;
                    return true;    
                }
            return false;
        }
    }
    public class RetrySendBlock { }       
}
