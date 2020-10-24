using System;
using System.Collections.Generic;
using Akka.Actor;
using NetwrokMessages;

namespace StartupServer
{
    public class RegistrationHandler :ReceiveActor
    {
        public List<IActorRef> Refrences { get; set; } = new List<IActorRef>();
        public SortedList<string,byte[]> Members { get; set; } = new SortedList<string, byte[]>(); 
        public RegistrationHandler()        
        {
                Receive<string>(msg =>  System.Console.WriteLine("Message recieved correctly"+msg+" and sender path is : "+Sender.Path));
                Receive<RegistrationRequest>(Message =>
                {
                    //to change
                    // check if ConsensusNode Already registred by checking his ipaddress AAAAnd key
                    if(! Members.ContainsKey(Message.Address))
                    {
                        System.Console.WriteLine("Received: "+BitConverter.ToString(Message.PublicKey)+" : "+Message.Address);
                        Refrences.Add(Sender);
                        Members.Add(Message.Address,Message.PublicKey);
                        System.Console.WriteLine("New Consensus Member Added ");

                        if (Members.Count == 4)
                        {
                            // send a reply to the consensus nodes
                            foreach (var item in Refrences)
                            {
                                System.Console.WriteLine("Sending MemberList to :"+item.Path);
                                item.Tell(new MembersList(Members,Refrences));
                            }               
                            
                        }
                    }
                }); 
        }       
    }
}