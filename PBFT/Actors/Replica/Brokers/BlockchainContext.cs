using System.Collections.Generic;
using System.Net;
using ConsensusMessages;
using Microsoft.EntityFrameworkCore;

namespace PBFT
{
        class BlockchainContext : DbContext
        {
            public IPAddress DBAddress {get; set;} 
            public BlockchainContext()
            {
            }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseMySQL("server =localhost  ;database = Blockchain ;user = root ; password =sdeath ");
            }
            protected override void OnModelCreating(ModelBuilder builder){
                
                builder.Entity<Block>().HasKey(block =>new { block.SequenceNumber, block.ViewNumber});
                builder.Entity<Block>().HasOne<BlockHeader>(block => block.Header)
                .WithOne(header => header.block)
                .HasForeignKey<BlockHeader>(header => new {header.SequenceNumber,header.ViewNumber});

                
                builder.Entity<BlockHeader>().Property(p => p.Hash).HasMaxLength(256);
                builder.Entity<BlockHeader>().HasKey(header => header.Hash);
                builder.Entity<BlockHeader>().Property(p => p.version);
                builder.Entity<BlockHeader>().Property(p => p.MerkleRoot); 
                builder.Entity<BlockHeader>().Property( p => p.PreviousHash);
                builder.Entity<BlockHeader>().Property( p => p.timestamp);  
            }
            public DbSet<Block> Block {get; set;}
            public DbSet<BlockHeader> Blockheaders {get; set;}
            public DbSet<BlockTransaction> BlockTransactions {get ; set;}
        }
}
