using System.Collections.Generic;
using System.Net;
using Akka.Actor;
using NSec.Cryptography;

namespace PBFT
{
    public class Configs
        {
        
        public SortedList<string,byte[]> Members{ get; set;}
        
        public List<IActorRef> _replicaRefs = new List<IActorRef>(); 
        public List<IActorRef> ReplicaRefrences{ 
        get
        {
            return _replicaRefs;
        } 
        set
        {
            _replicaRefs = new List<IActorRef>(value);
            ConsensusTHreashold = 2*((_replicaRefs.Count -1) / 3) + 1;
            System.Console.WriteLine("Consensus threashold : "+ConsensusTHreashold);   
        } }
            
        public int ConsensusTHreashold =0;
        
        public List<string> ClientMembers{ get; set;}

        public byte[] PubKey { get; } 

        public Key Keys {get ;}

        public string MyAddress { get; }

        public IPAddress ServerAddress {get;}   

        public Configs(IPAddress ServerAddr)
        {
            Keys = new Key(SignatureAlgorithm.Ed25519);
            PubKey = Keys.PublicKey.Export(KeyBlobFormat.NSecPublicKey);

            string hostname=Dns.GetHostName();
            var ip=Dns.GetHostEntry(hostname);
            var Address=ip.AddressList[0];
            MyAddress = Address.ToString();
            ServerAddress = ServerAddr;
        }
    }
    
}