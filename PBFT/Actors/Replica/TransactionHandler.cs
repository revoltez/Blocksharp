using System.Collections.Generic;
using Akka.Actor;
using ConsensusMessages;
using NSec.Cryptography;

namespace PBFT
{
    class TransactionHandler : ReceiveActor
    {
        IActorRef Log{get; set;}
        HashSet<byte[]> ClientMembers{ get; set; }

        public TransactionHandler(IActorRef log,HashSet<byte[]> ClientMembers)
        {
            Log=log;
            // verify validty of the message and 
            Receive<ClientTransaction>(Message => VerifyMessageValidity(Message),Message =>Log.Tell(Message));
        }

        private bool VerifyMessageValidity(ClientTransaction Message)
        {
            var Algorithm = SignatureAlgorithm.Ed25519;
/*            if ( ClientMembers.Contains(Message.Hash))
            {
                    PublicKey pubkey = PublicKey.Import(Algorithm,Message.Hash,KeyBlobFormat.NSecPublicKey);
                    return Algorithm.Verify(pubkey,Message.GetBytes(),Message.Signature);
            }*/
            return true;
        }
    }
}
