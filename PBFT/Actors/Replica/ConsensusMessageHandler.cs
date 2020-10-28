using Akka.Actor;
using ConsensusMessages;
using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PBFT
{
    class ConsensusMessageHandler : ReceiveActor
    {
        public SortedList<string,byte[]> Members {get; set;}
        public Key Keys {get ; private set;}
        public Configs Configurations {get ; }
        IActorRef Log,BlockchainBroker,ViewChange;

        public int ViewNumber { get; private set; } = 0;
        public int SequenceNumber { get; private set; }= 2;

        protected override void PreStart()
        {

        }
        
        // Must change in production ret
        public ConsensusMessageHandler(Configs config,IActorRef Blockchain)
        {
            Members= config.Members;
            BlockchainBroker = Blockchain;
            Configurations = config;

            ViewChange = Context.ActorOf(Props.Create<ViewChangeHandler>(Configurations),"ViewChangeHandler");
            Log = Context.ActorOf(Props.Create<LogBroker>(BlockchainBroker,Configurations),"LogBroker");
            Context.Parent.Tell(Log);
            
            Receive<PBFTMessage>(Message => VerifyMessageValidty(Message), Message =>
            {   
                if (Message.MsgType.Equals(PBFTMessage.MessageType.ViewChange)||Message.MsgType.Equals(PBFTMessage.MessageType.NewView))
                {
                    ViewChange.Tell(Message);
                }
                else
                {
                    Log.Tell(Message);
                }
            });

            Receive<UpdateSequenceNumber>(Message => SequenceNumber=Message.SequenceNum);
            Receive<UpdateViewNumber>( _ => ViewNumber++);
            Receive<RaiseViewChange>(_ =>
            {
                var SignedViewchange = SignMessage(new PBFTMessage(PBFTMessage.MessageType.ViewChange,Configurations.PubKey,ViewNumber,SequenceNumber));
                foreach (var item in Configurations.ReplicaRefrences)
                {
                    item.Tell(SignedViewchange.PBFTmsg);   
                }    
            });

            Receive<ConsensusMessageRequest>(Message =>
            {       
                switch (Message.Request)
                {
                    case ConsensusMessageOps.SignPrepareMsg:
                        Sender.Tell(SignMessage( new PBFTMessage(PBFTMessage.MessageType.Prepare,config.PubKey.ToArray(),Message.PBFTMsg.ViewNumber,Message.PBFTMsg.SequenceNumber,Message.PBFTMsg.BlockId.ToArray())));
                    break;

                    case ConsensusMessageOps.SignCommitMsg:
                        Sender.Tell(SignMessage( new PBFTMessage(PBFTMessage.MessageType.Commit,config.PubKey.ToArray(),Message.PBFTMsg.ViewNumber,Message.PBFTMsg.SequenceNumber,Message.PBFTMsg.BlockId.ToArray())));
                    break;                    
                
                    case ConsensusMessageOps.SignPrePrepareMsg:
                        Sender.Tell(SignMessage(new PBFTMessage(PBFTMessage.MessageType.PrePrepare,config.PubKey.ToArray(),Message.PBFTMsg.ViewNumber,Message.PBFTMsg.SequenceNumber,Message.PBFTMsg.BlockId.ToArray())));
                    break;
                    
                    case ConsensusMessageOps.SignViewChangeMsg:
                        Sender.Tell(SignMessage( new PBFTMessage(PBFTMessage.MessageType.ViewChange,config.PubKey.ToArray(),Message.PBFTMsg.ViewNumber,Message.PBFTMsg.SequenceNumber)));
                    break;
                    
                    case ConsensusMessageOps.SignNeViewMsg:
                        Sender.Tell(SignMessage( new PBFTMessage(PBFTMessage.MessageType.NewView,config.PubKey.ToArray(),Message.PBFTMsg.ViewNumber,Message.PBFTMsg.SequenceNumber)));
                    break;
                }
            });
        }

        public bool VerifyMessageValidty(PBFTMessage Message)
        {
            var Algorithm = SignatureAlgorithm.Ed25519;
            if (Configurations.Members.Values.Any(x => x.SequenceEqual(Message.Id)))
            {
                if (Message.ViewNumber >= ViewNumber && Message.SequenceNumber >= SequenceNumber)
                {
                    PublicKey pubkey = PublicKey.Import(Algorithm,Message.Id,KeyBlobFormat.NSecPublicKey);
                    return Algorithm.Verify(pubkey,Message.GetBytes(),Message.Signature);
                }
                else
                {
                    System.Console.WriteLine(Message.MsgType+" "+Message.SequenceNumber+":"+Message.ViewNumber+"Dropped\n  current sequence and view number : "+SequenceNumber+" "+ViewNumber);
                    return false;
                }
            }
            System.Console.WriteLine("Message Received From Uknown Node");
            return false;
        }
        
        public PBFTMessageSigned SignMessage(PBFTMessage Message)
        {
            Message.Signature = SignatureAlgorithm.Ed25519.Sign(Configurations.Keys,Message.GetBytes());
            return new PBFTMessageSigned(Message);
        }
    }
}
