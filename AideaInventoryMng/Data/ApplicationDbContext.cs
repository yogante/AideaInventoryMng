using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options ) 
            : base(options)
        {

        }
    }
}
