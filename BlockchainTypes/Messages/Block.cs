using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NSec.Cryptography;

namespace ConsensusMessages
{
    public class Block 
    {
        public int SequenceNumber {get ; private set;}
        public int ViewNumber {get ; private set;}
        public virtual BlockHeader Header {get ;  private set;}


        public List<BlockTransaction> Transactions {get ; private set; }

        // to create a new block a request must be sent to the transaction pool to get the List of the transactions
        // and a request to the Blockchain to get the final Block hash
        public Block(List<BlockTransaction> transactions,int sequencenumber,int viewNumber, byte[] previoushash)
        {
            Transactions= transactions;
            SequenceNumber=sequencenumber;
            ViewNumber = viewNumber;
            Header=new BlockHeader(previoushash,sequencenumber,viewNumber,transactions);        
            Header.SequenceNumber=sequencenumber;
        }
        public Block(){
        }
    }
    public class BlockHeader
    {
        [MaxLength(256)]
        public byte[] Hash {get ;} 
        [MaxLength(256)]
        public byte[]  PreviousHash { get;}
        public DateTime timestamp { get; }
        public int version{ get; }
        [MaxLength(256)]
        public byte[] MerkleRoot{ get; }
        public int SequenceNumber {get ; set;}
        public int ViewNumber{get ; set;}
        //this property is used only to configuire the one to one relationship
        public Block block  { get; set; }
        public BlockHeader (byte[] previoushash,int sequencenumber,int viewNumber,List<BlockTransaction> transactions)
        {
            SequenceNumber=sequencenumber;
            ViewNumber= viewNumber;
            timestamp = DateTime.UtcNow;
            PreviousHash =  previoushash;
            version = 1;
            MerkleRoot = ComputeMerkleRoot(transactions);
            Hash = ComputeBlockHash();
        }

        public BlockHeader(){

        }
        public byte[] ComputeMerkleRoot(List<BlockTransaction> transactions)
        {
            Sha256 Sha256 = new Sha256();

            byte[] Root= null ;
            bool Iterate = true;
            List<byte[]> Templist = new List<byte[]>();   
            foreach (Transaction item in transactions)
            {
                Templist.Add(item.Hash);
            }
            while (Iterate)
            {
                List<byte[]> Templist2 = new List<byte[]>();
                byte[] item = null;
                int count = Templist.Count;

                

                for (int i = 0; i < count ; i = i + 2)
                {
                    byte[] concat = new byte[(Templist[0].Length) * 2];
                    if ((Templist.Count - i == 1))
                    {
                        System.Buffer.BlockCopy(Templist[i], 0, concat, 0, Templist[i].Length);
                        System.Buffer.BlockCopy(Templist[i], 0, concat, Templist[i].Length, Templist[i].Length);
                        item = Sha256.Hash(concat);
                   
                        Templist2.Add(item);
                    }
                    else
                    {
                        System.Buffer.BlockCopy(Templist[i], 0, concat, 0, Templist[i].Length);
                        System.Buffer.BlockCopy(Templist[i + 1], 0, concat, Templist[i].Length, Templist[i + 1].Length);
                        item = Sha256.Hash(concat);
                      
                        Templist2.Add(item);
                    }
                }
                Templist = new List<byte[]>(Templist2);
                if (count == 2)
                {
                    Root = item ;
                    Iterate = false ;
                }
            }
            return Root;
        }
        public byte[] ComputeBlockHash()
        {
            Sha256 sha256 = new Sha256();
            String HeaderString = BitConverter.ToString(PreviousHash)+timestamp+version+BitConverter.ToString(MerkleRoot)+SequenceNumber;
            return sha256.Hash(Encoding.UTF8.GetBytes(""+HeaderString));
        }
    }
}
