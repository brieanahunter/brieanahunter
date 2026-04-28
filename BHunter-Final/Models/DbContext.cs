using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BHunter_Final.Models
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options)
            : base(options)
        {
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieCast> MovieCasts { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Messages> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                      .HasColumnName("UserId")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                      .HasColumnName("FirstName")
                      .IsRequired();

                entity.Property(e => e.LastName)
                      .HasColumnName("LastName")
                      .IsRequired();

                entity.Property(e => e.Email)
                      .HasColumnName("Email")
                      .IsRequired();

                entity.Property(e => e.Password)
                    .HasColumnName("Password")
                    .IsRequired();

                entity.Property(e => e.ProfileImagePath)
                      .HasColumnName("ProfileImagePath")
                      .IsRequired(false);

                entity.Property(e => e.GroupId)
                      .HasColumnName("GroupId")
                      .IsRequired(false);
            });

            modelBuilder.Entity<Messages>()
                .HasKey(m => m.MessageId);
            modelBuilder.Entity<MovieCast>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.Cast)
                .HasForeignKey(mc => mc.MovieId);

            modelBuilder.Entity<Favorites>()
        .HasKey(f => new { f.UserId, f.MovieId });

            modelBuilder.Entity<Favorites>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorites>()
                .HasOne(f => f.Movie)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

           

            modelBuilder.Entity<Movie>()
       .HasKey(m => m.MovieId);

            modelBuilder.Entity<Movie>().ToTable("Movies");
            modelBuilder.Entity<MovieCast>().ToTable("MovieCast");

            modelBuilder.Entity<Movie>()
                .HasKey(m => m.MovieId);

            modelBuilder.Entity<MovieCast>()
                .HasKey(mc => mc.CastId);

            modelBuilder.Entity<MovieCast>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.Cast)
                .HasForeignKey(mc => mc.MovieId);


            modelBuilder.Entity<Group>()
                 .HasOne(g => g.CreatedByUser)
                 .WithMany()
                 .HasForeignKey(g => g.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.MovieOfTheWeek)
                .WithMany()
                .HasForeignKey(g => g.MovieOfTheWeekId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Messages)
                .HasForeignKey(m => m.GroupId);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId);

        }

    }
}