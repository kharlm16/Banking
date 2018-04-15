using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KharlBanking.Models;
using KharlBanking.Models.Entities;

namespace KharlBanking.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

		public DbSet<Transaction> Transactions { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
        {
			base.OnModelCreating(builder);
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);

			builder.Entity<Transaction>(entity =>
			{
				entity.ToTable("Transaction");
				entity.HasOne(p => p.CreatedBy)
					.WithMany(p => p.TransactionCreatedBys)
					.HasForeignKey(p => p.CreatedById)
					.OnDelete(DeleteBehavior.Restrict);
				entity.HasOne(p => p.User)
					.WithMany(p => p.Transactions)
					.HasForeignKey(p => p.UserId)
					.OnDelete(DeleteBehavior.Restrict);
			});
		}
    }
}
