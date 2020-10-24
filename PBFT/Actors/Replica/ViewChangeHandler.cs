using System.Collections.Generic;
using System.Timers;
using Akka.Actor;
using ConsensusMessages;

namespace PBFT
{
    /*
        actor responsible for anything related to view changes and new views
    */
    public class ViewChangeHandler : ReceiveActor
    {
        public int ViewNumber =0;
        public int SequenceNumber =2; 
        public Configs Configurations{get ; set;}
        public int ViewCounter {get ; set;}=0;
        public Timer ViewChangeTimeout =new Timer(12000);   
        

        protected override void PreStart()
        {
            ReSetViewChangeTimeout(Context);
        }
        
        public ViewChangeHandler(Configs conifgurations)
        {
            Configurations = conifgurations;

            System.Console.WriteLine("----------------------------------");
            System.Console.WriteLine("View Change Handler intialized properly");
            System.Console.WriteLine("----------------------------------");

            Receive<UpdateSequenceNumber>(Message =>
            {
                SequenceNumber = Message.SequenceNum;
            });

            Receive<ResetViewChange>( _ => ReSetViewChangeTimeout(Context));

            Receive<PBFTMessage>(Message => 
            {
                switch(Message.MsgType)
                {
                    case PBFTMessage.MessageType.ViewChange:
                    
                    ViewCounter++;
                    if(ViewCounter==3)
                    {
                        // send a new view Message
                        ViewCounter = 0;
                        ViewNumber++;
                        Context.ActorSelection("akka://VotingSystem/user/ConsensusNode/PrimaryActor").Tell(new UpdateViewNumber());            
                        Context.ActorSelection("akka://VotingSystem/user/ConsensusNode/BlockchainBroker").Tell(new UpdateViewNumber());            
                        Context.Parent.Tell(new UpdateViewNumber());
                        System.Console.WriteLine("----------------------");
                        System.Console.WriteLine("New Primary is At : "+Configurations.Members.Keys[ViewNumber%Configurations.Members.Count]);
                        System.Console.WriteLine("----------------------");
                        /*var NewView =(PBFTMessageSigned) await cmessageH.Ask(new ConsensusMessageRequest(ConsensusMessageOps.SignNeViewMsg, new PBFTMessage(PBFTMessage.MessageType.NewView,Conifgurations.PubKey,ViewNumber,0)));   
                        foreach (var item in ReplicaRefrences)
                        {
                            if(item.Path.ToString().Contains("akka.tcp")) item.Tell(NewView);     
                        }*/
                        ReSetViewChangeTimeout(Context);
                    }
                    break;

                    case PBFTMessage.MessageType.NewView:
                        System.Console.WriteLine("New View Message Recived");
                    break;
                }
            });
        }
       
       //Tochange
        public void ReSetViewChangeTimeout(IUntypedActorContext context){
            
            System.Console.WriteLine("Reinitializing View Change Timeout");  
            ViewChangeTimeout.Stop();
            ViewChangeTimeout=new System.Timers.Timer(12000);
            ViewChangeTimeout.AutoReset=false;
            ViewChangeTimeout.Elapsed+= async (sender,e)=>
            {
                System.Console.WriteLine("View Change Timeout Elapseed ");
                var Unsigned=new PBFTMessage(PBFTMessage.MessageType.ViewChange,Configurations.PubKey,ViewNumber,SequenceNumber);
                var ViewChange= (PBFTMessageSigned) await context.Parent.Ask(new ConsensusMessageRequest(ConsensusMessageOps.SignViewChangeMsg,Unsigned));    
                foreach (var item in Configurations.ReplicaRefrences)
                {
                    
                    item.Tell(ViewChange.PBFTmsg);
                }
                    System.Console.WriteLine("View change Message sent");
            };
            ViewChangeTimeout.Start();
                    
        }
    }


    public class UpdateViewNumber
    {
        public UpdateViewNumber()
        {
        }
    }
    public class ResetViewChange
    {
        
    }
}