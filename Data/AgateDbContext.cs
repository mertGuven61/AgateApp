using AgateApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AgateApp.Data
{
	public class AgateDbContext : DbContext
	{
		public AgateDbContext(DbContextOptions<AgateDbContext> options) : base(options)
		{
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<Campaign> Campaigns { get; set; }
		public DbSet<Advert> Adverts { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Requirement 7: Ensure money values are stored correctly in SQL
			modelBuilder.Entity<Campaign>().Property(c => c.EstimatedCost).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.Budget).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.ActualCost).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.AmountPaid).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Advert>().Property(a => a.Cost).HasColumnType("decimal(18,2)");
		}
	}
}
