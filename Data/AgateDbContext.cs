using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgateApp.Models;

namespace AgateApp.Data
{
	public class AgateDbContext : IdentityDbContext
	{
		public AgateDbContext(DbContextOptions<AgateDbContext> options) : base(options)
		{
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<Campaign> Campaigns { get; set; }
		public DbSet<Advert> Adverts { get; set; }
		public DbSet<CampaignAssignment> CampaignAssignments { get; set; }
		public DbSet<AdvertNote> AdvertNotes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Campaign>().Property(c => c.EstimatedCost).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.Budget).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.ActualCost).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Campaign>().Property(c => c.AmountPaid).HasColumnType("decimal(18,2)");
			modelBuilder.Entity<Advert>().Property(a => a.Cost).HasColumnType("decimal(18,2)");
		}
	}
}