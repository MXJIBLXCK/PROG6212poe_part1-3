using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay_EventMS.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure 1-to-1 relationship between User and UserProfile
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships & cascade delete rules
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.EventsOrganised)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(en => en.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure email is unique per user
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}