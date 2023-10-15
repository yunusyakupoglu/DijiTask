using Microsoft.EntityFrameworkCore;
using OL;
using System.Configuration;


namespace DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Machines> Machines { get; set; }
        public DbSet<Shifts> Shifts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=YUNUSYAKUPOGLU;Database=DijiTaskDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;");
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["dijiTaskDb"].ConnectionString);
        }
    }
}
