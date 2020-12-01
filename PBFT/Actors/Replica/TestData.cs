using System.Text;
using Akka.Actor;
using ConsensusMessages;
using NSec.Cryptography;

namespace PBFT
{
    public static class TestData
    {
        public static void PopulateTXpool(IActorRef Self)
        {
                string ss= "emanuel kant";
                string jalile ="david hume";
                string bilale ="bilale jalalati";
                string ahmad ="winrar ";
                string kayane="mohammed hijab";
                string sam="sam harris";
                string william="william lane craig";
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

                Self.Tell(new ClientTransaction(data));
                Self.Tell(new ClientTransaction(data2));
                Self.Tell(new ClientTransaction(data4));
                Self.Tell(new ClientTransaction(data5));
                Self.Tell(new ClientTransaction(data6));

                Self.Tell(new ClientTransaction(data7));
                Self.Tell(new ClientTransaction(data8));
                
        }
    }
}