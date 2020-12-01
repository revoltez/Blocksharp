using System.Collections.Generic;
using Akka.Actor;
using ConsensusMessages;

namespace PBFT
{
    public class ErrorMessage 
    {
        public string Reason {get ;}

        public ErrorMessage(string reason)
        {
            Reason = reason;
        }
    }
    
    #region BlockMessages
    public enum BlockchainOpsRequests
    {
        CheckDuplicateTransactions, 
        AddBlock,
        GetLastBlockHash,
        UpdateSequenceNumber,
        CountVotes
    }
    public class BlockchainRequest
    {
        public Block block {get;}
        public BlockchainOpsRequests Request { get ;}
        public List<BlockTransaction> BlockTransactions{ get; }
    
        public BlockchainRequest(BlockchainOpsRequests req)
        {
            Request=req;
        }
    
        public BlockchainRequest(BlockchainOpsRequests req,Block blk)
        {
            Request = req;
            block = blk;
        }
    
        public BlockchainRequest(BlockchainOpsRequests req,List<BlockTransaction> list)
        {
            Request = req;
            BlockTransactions = new List<BlockTransaction>(list);
        }
    }

    class ValidBlock { }

    class RaiseViewChange
    {
        public RaiseViewChange()
        {
        }
    }

    public class UpdateSequenceNumber{
        public int SequenceNum {get;}
        public UpdateSequenceNumber(int seq)
        {
                SequenceNum = seq+1;
        }
    }
    public class LastBlockHash
    {
        public byte[] Hash{ get; }
        public LastBlockHash(byte[] last)
        {
            Hash=last;
        }
    }
    #endregion

    #region LogMessages
    public enum LogOps
        {
            RemoveTransaction,
            CreateBlock
        }

    public class UnavaiableTransactions
    {
        public UnavaiableTransactions()
        {
        }
    }
        public class LogOpRequest
        {
            public LogOps Op {get ; } 
            public  List<BlockTransaction> Transactions { get ;  }
  
            public LogOpRequest(LogOps op){
                Op = op ;
            }


            public LogOpRequest(LogOps op, List<BlockTransaction> transactions){
                
                Op = op ;
                Transactions = transactions;
            }            
        }
        public class BlockCreated {
            public byte[] hash {get ;}
            public List<ClientTransaction> clientTransactions{ get; set;}
            public BlockCreated(byte[] blk, List<ClientTransaction> txs)
            {
                hash = blk;
                clientTransactions = txs;
            }
        }

    #endregion

    #region ConsensusMessageHandler Requests
    public enum ConsensusMessageOps
    {
        SignPBFTMessage,
        SendBlockToClients,
        GetReplicaRefrences,
        SignPrepareMsg,
        SignCommitMsg,
        SignNeViewMsg,
        SignPrePrepareMsg,
        SignViewChangeMsg
    }
    
    public class ConsensusMessageRequest
    {
        public ConsensusMessageOps Request{get ;}
        public PBFTMessage PBFTMsg{ get;}
        public Block block { get; }
        public ConsensusMessageRequest(ConsensusMessageOps req)
        {
            Request = req;
        }
        public ConsensusMessageRequest(ConsensusMessageOps req,PBFTMessage message)
        {
            Request = req;
            PBFTMsg=message;
        }
    }

    public class PBFTMessageSigned
    {
        public PBFTMessage PBFTmsg {get;}
        public PBFTMessageSigned(PBFTMessage msg)
        {
            PBFTmsg = msg;
        }
    }
    public class ReplicaRefrences 
    {
        public List<IActorRef> ReplicaRefs {get;}
        public ReplicaRefrences(List<IActorRef> refs)
        {
            ReplicaRefs= new List<IActorRef>(refs);   
        }
    }

    #endregion

}