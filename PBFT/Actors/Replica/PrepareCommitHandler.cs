using System;
using System.Collections.Generic;
using Akka.Actor;
using System.Linq;
using ConsensusMessages;

namespace PBFT
{
    class PrepareCommitHandler : ReceiveActor
    {
        
        PrePrepareLog prePrepare{get ;set;} 

        Configs Configurations {get; set; } 
        public string blockid {get ; set; }="";
        public List<PBFTMessage> UnstoredCertificates{ get; set;} = new List<PBFTMessage>();
        
        IActorRef CMessageHandler { get; set;}
        
        protected override void PreStart()
        {
        
        }
        
        public PrepareCommitHandler(Configs configurations,IActorRef CMessageRef)
        {
        
            CMessageHandler= CMessageRef;
            Configurations = configurations;    
            Receive<PBFTMessage>( Message =>
            {
                switch (Message.MsgType)
                {
                    case PBFTMessage.MessageType.PrePrepare:
                        prePrepare = new PrePrepareLog((PrePrepare)Message,configurations.Members);
                        System.Console.WriteLine("prePrepare Received ");
                        blockid=BitConverter.ToString(prePrepare.BlockId);
                        prePrepare.PrepareCounter++;
                        //verify if there are any unstored Messages
                        foreach (var item in UnstoredCertificates)
                        {
                            if (item.BlockId.SequenceEqual( prePrepare.BlockId))
                            {
                                System.Console.WriteLine("unstored certificate matches prePrepare Blockid");
                                VerifyMessageThreshold(Context,item);
                            }
                            else {  System.Console.WriteLine("Message BlockId does not match prePrepare Blockid");}   
                        }
                    break;        

                    case PBFTMessage.MessageType.Prepare:
                    case PBFTMessage.MessageType.Commit:

                        if(prePrepare!=null)
                        {
                            System.Console.WriteLine("Processing Certificate");
                            VerifyMessageThreshold(Context,Message);    
                        }
                        else
                        {
                            System.Console.WriteLine("prePrepare still not Recieved yet");
                            UnstoredCertificates.Add(Message);
                        }
                    break;
                }
            });
        }
        public async void VerifyMessageThreshold(IUntypedActorContext context,PBFTMessage Message)
        {

                if(prePrepare.BlockId.SequenceEqual(Message.BlockId))
                {
                    System.Console.WriteLine("sender id :" + BitConverter.ToString(Message.Id));

                    if(Message.MsgType.Equals(PBFTMessage.MessageType.Prepare) && !prePrepare.preparebool[BitConverter.ToString(Message.Id)])
                    {
                        //test
                        //check if the message was already sent  
                        prePrepare.PrepareCounter++;
                        prePrepare.preparebool[BitConverter.ToString(Message.Id)]= true;

                        System.Console.WriteLine("prepare counter : "+prePrepare.PrepareCounter +", commit counter : "+prePrepare.CommitCounter);
                        if( prePrepare.PrepareCounter == Configurations.ConsensusTHreashold )
                        {
                                    
                            var pBFTMessageSigned =(PBFTMessageSigned) await CMessageHandler.Ask(new ConsensusMessageRequest(ConsensusMessageOps.SignCommitMsg,Message));                                        
                                        
                            foreach (var item in Configurations.ReplicaRefrences)
                            {
                                if(item.Path.ToString().Contains("akka.tcp")) item.Tell(pBFTMessageSigned.PBFTmsg);   
                            }
                            System.Console.WriteLine("Commit Message Sent");
                                prePrepare.CommitCounter++;
                        }
                                
                        if( prePrepare.PrepareCounter >=Configurations.ConsensusTHreashold &&  prePrepare.CommitCounter >= Configurations.ConsensusTHreashold && prePrepare.Commited == false)
                        {
                                        //commit the Block and reply to the client 
                            prePrepare.Commited= true;
                            context.ActorSelection("akka://Blocksharp/user/ConsensusNode/BlockchainBroker").Tell(new BlockchainRequest(BlockchainOpsRequests.AddBlock,prePrepare.Block));
                        }       
                    }
                    else if (! prePrepare.commitbool[BitConverter.ToString(Message.Id)])
                    {
                                    //test
                        prePrepare.CommitCounter++;
                        prePrepare.commitbool[BitConverter.ToString(Message.Id)] = true;

                        System.Console.WriteLine("prepare counter : "+prePrepare.PrepareCounter +", commit counter : "+prePrepare.CommitCounter);
                        if(prePrepare.PrepareCounter >= Configurations.ConsensusTHreashold &&  prePrepare.CommitCounter >=Configurations.ConsensusTHreashold && prePrepare.Commited == false)
                        {
                                    //commit the Block and reply to the client 
                            prePrepare.Commited = true;
                            context.ActorSelection("akka://Blocksharp/user/ConsensusNode/BlockchainBroker").Tell(new BlockchainRequest(BlockchainOpsRequests.AddBlock,prePrepare.Block));
                       
                            System.Console.WriteLine("-----------------------");
                            System.Console.WriteLine("Commiting The New Block");
                            System.Console.WriteLine("-----------------------");
                        }  
                    }
                }
                else
                {
                    System.Console.WriteLine("Message does not match prePrepare Block id");   
                }
        }
    }
}
