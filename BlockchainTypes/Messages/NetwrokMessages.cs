using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Net;
using System.Collections;
namespace NetwrokMessages
{
    
    #region ServerMessages
   
   public enum CandidtaeList
   {
        alghazali,ibnrochd,ibncina   
   }
   
   
    public class RegistrationRequest
    {
        //cannot Send the public key over the network as byte, same thing for IPAddress , the both need to be changed 
        // must always have a setter , serilizer needs it
        public byte[] PublicKey{ get; private set; }
                    
        public string Address { get; private set; }

        public RegistrationRequest(byte[] PubKey, string iP)
        {
            PublicKey = PubKey;
            Address = iP;
             
        }
    }

    public class MembersList
    {
        public SortedList<string,byte[]> Members { get;  set;}
        public List<IActorRef> MembersReferences { get;  set;}
        public MembersList(SortedList<string,byte[]> members,List<IActorRef> membersrefs)
        {
            Members= new SortedList<string, byte[]>(members);   
            MembersReferences = new List<IActorRef>(membersrefs);
        }
    }
    #endregion
}