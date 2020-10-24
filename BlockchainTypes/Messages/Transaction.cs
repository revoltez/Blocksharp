using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NSec.Cryptography;
using NetwrokMessages;
namespace ConsensusMessages
{
    
    public abstract class Transaction  
    {
        [MaxLength(256)]
        [Key]
        public byte[] Hash {get ; set;}  

        [MaxLength(256)]
        public byte[] Id {get; set;}

        public CandidtaeList VoteTo {get; set;}

        [MaxLength(256)]
        public byte[] Signature { get ; set;}
        
        public DateTime Timestamp{ get; set;}


        public Transaction()
        {
            
        }

        public Transaction(byte[] id,CandidtaeList voteTo)
        {
                Timestamp= DateTime.Now;
                Id=id;
                VoteTo= voteTo;
                Sha256 sha = new Sha256();
                Hash = sha.Hash(GetBytes());
        }

        public override string ToString()
        {
            //must add timstamp later , for now cant 
            return ""+BitConverter.ToString(Id)+VoteTo;
        } 
        
        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

    }

    [Table("BlockTransactions")]
    public class BlockTransaction : Transaction
    {

        //this constructor is used only for testing
        public BlockTransaction(byte[] hash)
        {
            Hash = hash;
        }
        public BlockTransaction(byte[] Id, CandidtaeList VoteTo):base(Id,VoteTo)
        {

        }
    }


    /*
        clientTransaction<T> : BlockTransaction
        {
            public clientTransaction
            {
                clinetTx = t;
            }
        }
    */

    [Table("TransactionPool")]
    public class ClientTransaction : Transaction  {
        
        public ClientTransaction(byte[] Id,CandidtaeList VoteTo): base(Id,VoteTo)  
        {  
        }
    }


/*    public class Vote:ClientTransaction 
    {
        public byte[] Id { get; set;}
        public string VoteTo { get; private set;}="";

        // base gets called automatically
        public Vote(byte[] id,string voteTo):base()
        {
            Id=id;
            VoteTo = voteTo;
            Hash = this.GetBytes();
        }
        
        public override string ToString()
        {
            return ""+BitConverter.ToString(Id)+VoteTo+Timestamp.ToString();   
        }
    }*/
}