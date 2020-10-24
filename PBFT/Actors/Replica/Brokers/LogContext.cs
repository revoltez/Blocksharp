using System.Net;
using ConsensusMessages;
using Microsoft.EntityFrameworkCore;

namespace PBFT
{
    class LogContext : DbContext
    {
        public IPAddress DBAddr{ get; set;}
        public LogContext()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder builder){
            builder.UseMySQL("server=localhost; database = Log ;user = root ; password =sdeath ");
        }
        protected override void OnModelCreating(ModelBuilder builder){
                
                builder.Entity<BlockHeader>().Property(p => p.Hash).HasMaxLength(256);
                builder.Entity<BlockHeader>().HasKey(header => header.Hash);
                builder.Entity<BlockHeader>().Property(p => p.version);
                builder.Entity<BlockHeader>().Property(p => p.MerkleRoot);
                builder.Entity<BlockHeader>().Property(p => p.PreviousHash);
                builder.Entity<BlockHeader>().Property(p => p.timestamp);
                
                builder.Entity<Block>().HasKey(block =>new {block.SequenceNumber, block.ViewNumber});
                builder.Entity<Block>().HasOne<BlockHeader>(block => block.Header)
                .WithOne(header => header.block)
                .HasForeignKey<BlockHeader>(header => new {header.SequenceNumber,header.ViewNumber});

                builder.Entity<PrePrepareLog>().HasKey(log => new {log.SequenceNumber,log.ViewNumber});
                
        }
        public DbSet<PrePrepareLog> Logs {get ; set ;}
        public DbSet<ClientTransaction> TransactionPool {get ; set ;}
    }
}