using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Conctrete.EfMemory
{
    public class NorthwindContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CarData;Trusted_Connection=true;");
        }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Color> Color { get; set; }
        public DbSet<Brand> Brands { get; set; }
    }
}
