using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using ConsensusMessages;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace PBFT
{
    class LogBroker : ReceiveActor 
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        LogContext Log = new LogContext();
        IActorRef BlockchainBroker;
        public Configs Configurations { get; set;}
        protected  override void PreStart()
        {
        
        }

        public LogBroker(IActorRef blkbroker,Configs configurations)
        {
            
            BlockchainBroker = blkbroker;
            Configurations=configurations;

            ReceiveAsync<LogOpRequest>(async Message =>
            {
                switch (Message.Op)
                {
                    case LogOps.RemoveTransaction:

                        foreach (var item in Message.Transactions)
                        {
                            var tx = Log.TransactionPool.Where(t => t.Hash == item.Hash).ToList();
                            Log.TransactionPool.Remove(tx[0]);   
                        }                        
                        try
                        {
                            Log.SaveChanges(); 
                            System.Console.WriteLine("transactions removed successfully");
                            System.Console.WriteLine(Log.TransactionPool.Count()+" TXs remained in the pool");
                            Sender.Tell("Transactions Removed successufully");
                            
                        }
                        catch (Exception ex)
                        {
                            log.Info("Transaction does not exist in transaction pool \n"+ex.Message);   
                        }
                    
                    break;

                    case LogOps.CreateBlock:
            
                        var Received = await BlockchainBroker.Ask(new BlockchainRequest(BlockchainOpsRequests.GetLastBlockHash));

                            try
                            { 
                                if (Received is LastBlockHash)
                                {
                                    //tochange Magic number when retireiving transactions
                                    if(Log.TransactionPool.Count()>=2)
                                    {
                                        var lasthash= Received as LastBlockHash;
                                        var TransactionList= Log.TransactionPool.OrderByDescending(tx => tx.Timestamp).Take(2).ToList();
                                        Sender.Tell(new BlockCreated(lasthash.Hash,TransactionList));

                                    }
                                    else
                                    {
                                        Sender.Tell(new ErrorMessage("Transaction pool is empty"));
                                    }
                                }
                                else if(Received is ErrorMessage)
                                {
                                    Sender.Tell(Message);
                                }
                            }
                            catch (DbUpdateException ex)
                            {
                                var sqlexception = ex.GetBaseException() as MySqlException;
                            }
                    break;    
                }
            });

            Receive<ClientTransaction>(Message => 
            {
                try
                {
                    Log.TransactionPool.Add(Message);
                    Log.SaveChanges();
                    System.Console.WriteLine("TX Added");
                }
                catch(DbUpdateException ex) 
                {
                    var sqlexception = ex.GetBaseException() as MySqlException;
                    System.Console.WriteLine("Exception when trying to Add A client transaction : "+sqlexception.Message);
                }
            });


            Receive<PBFTMessageSigned>(Message => 
            {
               foreach (var item in Configurations.ReplicaRefrences)
               {
                    if(!item.Equals(Context.Self)) item.Tell(Message);   
               }
            });

            ReceiveAsync<PBFTMessage>(async Message => 
            {
                switch (Message.MsgType)
                {
                    case PBFTMessage.MessageType.PrePrepare:
                        await HandlePrePrepare(Message,Context);
                    break;    

                    case PBFTMessage.MessageType.Prepare: 
                    case PBFTMessage.MessageType.Commit:
                        DelegateToChild(Context,Message);
                    break;
                }
            });
        }

        /* Get the Prepreare from the Logdatabase and create
        a child actor that will handle all the incoming
        Prepare and commit certficates 
        check if a Child already exists if yes send it the prepare or commit certificate*/
        

 
        //handle exceptions properly
        private async Task HandlePrePrepare(PBFTMessage Message,IActorContext context)
        {
            try
            {
                var prePrepareLog =  new PrePrepareLog((PrePrepare) Message);

                    System.Console.WriteLine("------------------------");
                    System.Console.WriteLine("Received PREPREPARE "+Message.SequenceNumber+" : "+Message.ViewNumber);
                    System.Console.WriteLine("------------------------");
               
                var Response = await BlockchainBroker.Ask(new BlockchainRequest(BlockchainOpsRequests.CheckDuplicateTransactions,prePrepareLog.Block.Transactions));
                if(Response is ValidBlock)
                {
                    Log.Logs.Add(prePrepareLog);
                    Log.SaveChanges();
                    context.ActorSelection("akka://VotingSystem/user/ConsensusNode/ConsensusMessageHandler/ViewChangeHandler").Tell(new ResetViewChange());
                    
                    var Child =context.Child("PrepareCommitHandler"+Message.SequenceNumber+""+Message.ViewNumber);
                        if(Equals(Child,ActorRefs.Nobody))
                        {
                            var handler = context.ActorOf(Props.Create<PrepareCommitHandler>(Configurations,context.Parent),"PrepareCommitHandler"+Message.SequenceNumber+""+Message.ViewNumber);
                            handler.Tell(Message);
                        }
                        else
                        {
                            log.Info("Already received some corresponding certificates");
                            Child.Tell(Message);
                        }

                    if(!Message.Id.SequenceEqual(Configurations.PubKey))
                    {
                        var Prepare = (PBFTMessageSigned) await context.Parent.Ask(new ConsensusMessageRequest(ConsensusMessageOps.SignPrepareMsg,Message));
                        System.Console.WriteLine("PrePrepare Accepted ===>> Sending Prepare"); 
                        foreach (var item in Configurations.ReplicaRefrences)
                        {
                            //tochange
                            if(item.Path.ToString().Contains("akka.tcp")) item.Tell(Prepare.PBFTmsg); 
                        }
                    }
                    return;
                }
                else
                {
                    //Raise view change 
                    context.Parent.Tell(new RaiseViewChange());
                   log.Info("Block not valid View change sent");
                   return;
                }
            }

            catch (Exception ex)
            {
                log.Info("duplicate entry , preprepare already exists "+ex.Message);
                return;
            }
        }


        public void DelegateToChild(IUntypedActorContext context,PBFTMessage Message)
        {
                var Child= context.Child("PrepareCommitHandler"+Message.SequenceNumber+""+Message.ViewNumber);
                if(Equals(Child, ActorRefs.Nobody))
                {
                    log.Info("Child does not exist , creating child");
                    var preprepareLog = Log.Logs.Find(Message.SequenceNumber,Message.ViewNumber); 
                    if( preprepareLog == null)
                    {
                        //Record the message for future use
                        log.Info("prePrepare was not Received Yet ");
                        var handler =context.ActorOf(Props.Create<PrepareCommitHandler>(Configurations,context.Parent),"PrepareCommitHandler"+Message.SequenceNumber+""+Message.ViewNumber);
                        handler.Tell(Message);
                    }else
                    {
                        log.Info("something is wrong");
                    }   //else clause would never happen because of Primary always creates a the child 
                }
                else
                {
                    Child.Tell(Message);
                }
        }
    }

}



