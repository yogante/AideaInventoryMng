using System.Security.Cryptography.X509Certificates;
using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options ) 
            : base(options){}

        //Productsテーブル
        public DbSet<Product> Products { get; set; }

    }
}
