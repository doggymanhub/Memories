using memory.Models;
using Microsoft.EntityFrameworkCore;
using System; // Guidのために追加
using System.Collections.Generic; // List<T> のために追加

namespace memory.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Group> Groups { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MemoryItem> Memories { get; set; }

        public DbSet<SnsLink> SnsLinks { get; set; } 
        public DbSet<PersonCategory> PersonCategories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Design-time DbContextインスタンスを作成するためにパラメータなしのコンストラクタも用意することが推奨される場合がある
        // public ApplicationDbContext() {}


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // データベースファイル名を memory.db とし、実行ファイルの隣に配置する例
                optionsBuilder.UseSqlite("Data Source=memory.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonCategory>()
                .HasKey(pc => new { pc.PersonId, pc.CategoryId });

            modelBuilder.Entity<PersonCategory>()
                .HasOne(pc => pc.Person)
                .WithMany(p => p.PersonCategories) 
                .HasForeignKey(pc => pc.PersonId);

            modelBuilder.Entity<PersonCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.PersonCategories)
                .HasForeignKey(pc => pc.CategoryId);

            modelBuilder.Entity<SnsLink>()
                .HasOne(sl => sl.Person)
                .WithMany(p => p.SnsLinks) 
                .HasForeignKey(sl => sl.PersonId);
            
            modelBuilder.Entity<Group>()
                .HasOne<Group>() 
                .WithMany()      
                .HasForeignKey(g => g.ParentId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }

    public class SnsLink
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public string Url { get; set; }
        public Person Person { get; set; }
    }

    public class PersonCategory
    {
        public Guid PersonId { get; set; }
        public Guid CategoryId { get; set; }
        public Person Person { get; set; }
        public Category Category { get; set; }
    }
} 