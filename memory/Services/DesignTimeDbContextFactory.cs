using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; 
using System.IO; 

namespace memory.Services
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // ApplicationDbContext.OnConfiguring で設定された接続文字列を使用するか、ここで明示的に指定
            optionsBuilder.UseSqlite("Data Source=memory.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 