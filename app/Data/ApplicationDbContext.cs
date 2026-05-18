using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                 entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Group
             modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            });

            // GroupMember
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(gm => new { gm.UserId, gm.GroupId });

                entity.HasOne(gm => gm.User)
                      .WithMany(u => u.GroupMembers)
                      .HasForeignKey(gm => gm.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gm => gm.Group)
                      .WithMany(g => g.Members)
                      .HasForeignKey(gm => gm.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Message
             modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Content).IsRequired().HasMaxLength(2000);
                entity.Property(m => m.SentAt).IsRequired();

                // Sender → Message
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Direct message recipient (nullable)
                entity.HasOne(m => m.Recipient)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.RecipientId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // Group message (nullable)
                entity.HasOne(m => m.Group)
                      .WithMany(g => g.Messages)
                      .HasForeignKey(m => m.GroupId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}