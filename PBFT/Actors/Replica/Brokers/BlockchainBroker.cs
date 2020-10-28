using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Akka.Actor;
using Akka.Event;
using ConsensusMessages;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NSec.Cryptography;
using NetwrokMessages;
namespace PBFT
{
    class BlockchainBroker : ReceiveActor 
    {
        bool ready { get; set;}
        private readonly ILoggingAdapter log = Context.GetLogger();
        public BlockchainContext Blockchain =new BlockchainContext(); 
        public int SequenceNumber {get ; set ;}= 1;

        //Dictionary<CandidtaeList,int> candidateVotes = new Dictionary<CandidtaeList, int>();
        
        public int ViewNumber {get ; set ;}= 0;
        public Block LastBlock{ get; set;}
        public List<Block> Unstored {get ;set;}= new List<Block>();        
        
        protected override void PreStart()
        {
            CreateGenesisBlock(Blockchain);            
        }
        public  BlockchainBroker()
        {
            
            
            Receive<BlockchainRequest>(Message => 
            {
                switch (Message.Request)
                {
                    case BlockchainOpsRequests.AddBlock :
                        AddBlock(Message,Context);
                    break;
                    
                    case BlockchainOpsRequests.GetLastBlockHash :
                        GetLastHash();
                    break;

                    case BlockchainOpsRequests.UpdateSequenceNumber:
                        UpdateSequenceNumber();
                    break;

                    case BlockchainOpsRequests.CheckDuplicateTransactions:
                        
                        if(CheckDuplicateTransactions(Message))
                        {
                            Sender.Tell(new ValidBlock());
                        }
                        else
                        {
                            Sender.Tell(new ErrorMessage("Block Contains Duplicate Transactions"));
                        }

                    break;

                    case BlockchainOpsRequests.CountVotes:
                       // CountVotes();
                    break;
                }
            });
            Receive<UpdateViewNumber>( _ => ViewNumber++);
        }


       /* public void CountVotes()
        {
            candidateVotes[CandidtaeList.alghazali]=0;
            candidateVotes[CandidtaeList.ibncina]=0;
            candidateVotes[CandidtaeList.ibnrochd]=0;

            var Blocks = Blockchain.Block.Where(b => b.SequenceNumber >1 ).Include(b => b.Transactions).ToList();

            foreach (var block in Blocks)
            {
                foreach (var item in block.Transactions)
                {
                    switch (item.VoteTo)
                    {
                        case CandidtaeList.alghazali:
                            candidateVotes[CandidtaeList.alghazali]++;
                        break;   

                        case CandidtaeList.ibnrochd:
                            candidateVotes[CandidtaeList.ibnrochd]++;
                        break;

                        case CandidtaeList.ibncina:
                            candidateVotes[CandidtaeList.ibncina]++;
                        break;
                        
                        default:
                            System.Console.WriteLine("Malformed tx");
                        break;                     
                    }   
                }
            }
            PrintVotgingResults();
        }

        private void PrintVotgingResults()
        {
            foreach (var item in candidateVotes)
            {
                System.Console.WriteLine("Candidate "+item.Key+" has : "+item.Value+" Votes");   
            }
        }*/

        private void CreateGenesisBlock(BlockchainContext Blockchain)
        {
            try
            {
                if(Blockchain.Block.Count() == 0){

                    //garbage data must change later
                    System.Console.WriteLine("Creating Genesis Block");
                    string jalile ="Genesis";
                    string talale ="Block";
                    Sha256 sha = new Sha256();
                    byte[] data= sha.Hash(Encoding.UTF8.GetBytes(jalile));
                    byte[] data2=sha.Hash(Encoding.UTF8.GetBytes(talale));
                    List<BlockTransaction> transaction= new List<BlockTransaction>(){new BlockTransaction(data),new BlockTransaction(data2)};
                    Block block = new Block(transaction,1,ViewNumber,data);
                    Blockchain.Block.Add(block);
                    Blockchain.SaveChanges();
                    LastBlock= block;

                    ready=true;

                    log.Info("genesis block added");
                }
            }
            catch (DbUpdateException ex)
            {
                var Sqlexception = ex.GetBaseException() as MySqlException;
                log.Info(Sqlexception.Message);
                throw;
            }
        }

    public void UpdateSequenceNumber()
    {

        log.Info("received sequencenumber request");
        try
        {
            Sender.Tell(new UpdateSequenceNumber( Blockchain.Block.Count()));
        }
        catch (DbUpdateException ex)
        {
            var Sqlexception = ex.GetBaseException() as MySqlException;
            log.Info(Sqlexception.Message);
        }
    }

    public bool CheckDuplicateTransactions(BlockchainRequest Message)
    {
        foreach (var item in Message.BlockTransactions)
        {
            if(Blockchain.BlockTransactions.Contains(item)) return false; 
        }
        return true;
        
    }

    public async void AddBlock(BlockchainRequest Message,IUntypedActorContext context)
    {
        try
        {
            if(SequenceNumber+1 == Message.block.SequenceNumber)
            {
                LastBlock = Message.block;
                Blockchain.Block.Add(Message.block);
                Blockchain.SaveChanges();
                SequenceNumber++;
                
                System.Console.WriteLine("NEW BLOCK ADDED, SEQUENCE NUMBER : "+SequenceNumber);
              
                VerifyUnstored();
                 
                context.ActorSelection("akka://Blocksharp/user/ConsensusNode/ConsensusMessageHandler").Tell(new UpdateSequenceNumber(SequenceNumber));
                context.ActorSelection("akka://Blocksharp/user/ConsensusNode/ConsensusMessageHandler/ViewChangeHandler").Tell(new UpdateSequenceNumber(SequenceNumber));
                
                await context.ActorSelection("akka://Blocksharp/user/ConsensusNode/ConsensusMessageHandler/LogBroker").Ask(new LogOpRequest(LogOps.RemoveTransaction,Message.block.Transactions));
                context.ActorSelection("akka://Blocksharp/user/ConsensusNode/PrimaryActor").Tell(new UpdateSequenceNumber(SequenceNumber));

            
            }
            else if(Message.block.SequenceNumber > SequenceNumber+1)
            {
                System.Console.WriteLine("Block Stored until Appropriate Block arrives");
                Unstored.Add(Message.block);
            }
        }
        catch (DbUpdateException ex)
        {
            var Sqlexception = ex.GetBaseException() as MySqlException;
            if (Sqlexception.Code ==0)
            {
               log.Info("duplicate block already exists");
            }
        }
    }

        /* private void UpdateVoteCount(Block block)
        {
            foreach (var item in block.Transactions)
            {
                switch (item.VoteTo)
                {
                    case CandidtaeList.alghazali:
                        candidateVotes[CandidtaeList.alghazali]++;
                    break;   

                    case CandidtaeList.ibnrochd:
                        candidateVotes[CandidtaeList.ibnrochd]++;
                    break;

                    case CandidtaeList.ibncina:
                        candidateVotes[CandidtaeList.ibncina]++;
                    break;
                        
                    default:
                        System.Console.WriteLine("Malformed tx");
                    break;                     
                }   
            }
            PrintVotgingResults();   
        }
    */
        private void VerifyUnstored()
        {
            for (int i = 0; i < Unstored.Count; i++)
            {
                if(SequenceNumber+1==Unstored[i].SequenceNumber){
                    try
                    {
                       Blockchain.Block.Add(Unstored[i]) ;
                       Blockchain.SaveChanges();
                       SequenceNumber++;
                       Unstored.Remove(Unstored[i]);
                       i=0;
                       System.Console.WriteLine("NEW BLOCK ADDED, SEQUENCE NUMBER : "+SequenceNumber);
                    
                    }
                    catch (DbUpdateException ex)
                    {
                        var Sqlexception = ex.GetBaseException() as MySqlException;
                        if (Sqlexception.Code ==0)
                        {
                           System.Console.WriteLine("Block Already Exists");
                        }
                    }
                }
            }   
        }
    
    public void GetLastHash()
    {
        try
        {
            if (ready)
            {
                Sender.Tell(new LastBlockHash(LastBlock.Header.Hash));
            }
            else
            {
                Sender.Tell(new ErrorMessage("BlockchainBroker is not yet ready to process The primary Request"));
            }
        }
        catch (DbUpdateException ex)
        {
            var Sqlexception = ex.GetBaseException() as MySqlException;
            log.Info(Sqlexception.Message);
        }
    }
    
}
}