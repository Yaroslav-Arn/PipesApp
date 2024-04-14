using Microsoft.EntityFrameworkCore;
using PipesApp.Models;

namespace PipesApp.Contexts
{
    public class ApplicationContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Pipe> Pipes { get; set; }
        public DbSet<SteelGrade> SteelGrades { get; set; }
        public DbSet<Package> Packages { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options):base(options)
        {
            Database.EnsureCreated();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка связей между моделями

            modelBuilder.Entity<Pipe>()
                .HasOne(p => p.Package)
                .WithMany(pck => pck.Pipes)
                .HasForeignKey(p => p.PackageId)
                .IsRequired(false);


            modelBuilder.Entity<Pipe>()
                .HasOne(p => p.SteelGrade)
                .WithMany(sg => sg.Pipes)
                .HasForeignKey(p => p.SteelGradeId);
        }
    }
}
