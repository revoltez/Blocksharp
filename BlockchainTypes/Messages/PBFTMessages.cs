using System;
using System.Text;
using NSec.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace ConsensusMessages
{
    public class PBFTMessage
    {
        public enum MessageType
        {PrePrepare,Prepare,Commit,ViewChange,NewView}
       
        public MessageType MsgType{ get; private set ;}

        [MaxLength(256)]
        public byte[] Id {get ; private set;}

        [MaxLength(256)]
        public byte[] Signature { get ; set;}
        public int ViewNumber { get; private set;}

        [Key]
        public int SequenceNumber{ get; private set;}

        [MaxLength(256)]
        public byte[] BlockId { get ; private set ; }

        //for EFCore
        public PBFTMessage(){


        }

        // first constructor in case the message is A newView or ViewChange Message
        public PBFTMessage(MessageType type, byte[] id,int View, int seq)
        {
            MsgType=type;
            Id = id.ToArray(); 
            ViewNumber = View;
            SequenceNumber = seq;
            //SignatureAlgorithm.Ed25519.Sign(keys,this.GetBytes());
        }
        //Second contructor to create a Prepare or commit message where the BlockId (hash) is needed
        public PBFTMessage(MessageType type, byte[] id,int View, int seq, byte[] blockId)
        {
            MsgType=type;
            Id = id.ToArray(); 
            ViewNumber = View;
            SequenceNumber = seq;
            BlockId= blockId.ToArray();
          
        }

        public override string ToString(){
            if(BlockId!= null)
            {
                return ""+MsgType+BitConverter.ToString(Id)+ViewNumber+SequenceNumber+BitConverter.ToString(BlockId);
            }
            else
            {
                return ""+MsgType+BitConverter.ToString(Id)+ViewNumber+SequenceNumber;
            }
        }
        public byte[] GetBytes(){
            return Encoding.UTF8.GetBytes(this.ToString());
        }
    }
    public class PrePrepare : PBFTMessage{

        public  Block Block {get ; set;}
        public PrePrepare(Block block,byte[] id,int view, int seq) : base(MessageType.PrePrepare,id,view,seq,block.Header.Hash)
        {
            Block =block;
        }
        
        public PrePrepare()
        {

        }

    }
    
    public class PrePrepareLog : PrePrepare{
        

        public bool Commited {get ; set ;} = false ;

        public Dictionary<string,bool> preparebool = new Dictionary<string, bool>();

        public Dictionary<string,bool> commitbool= new Dictionary<string, bool>();

        public int PrepareCounter{get ; set ;}=0;
        public int CommitCounter { get ; set ;} =0;
 
        public PrePrepareLog(){
        
        }
        public PrePrepareLog(PrePrepare msg,SortedList<string,byte[]> members) : base (msg.Block,msg.Id ,msg.ViewNumber, msg.SequenceNumber)
        {
            foreach (var item in members)
            {
                preparebool.Add(BitConverter.ToString(item.Value),false);
                commitbool.Add(BitConverter.ToString(item.Value),false); 
            }   
        }

        //this consctrucot is used whenever a Prepare Message is received 
        public PrePrepareLog(PrePrepare msg,int prepareCount,int commitCount):base()
        {
            PrepareCounter= prepareCount;
            CommitCounter= commitCount;    
        }
    }
}