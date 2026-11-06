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
        public DbSet<Product> Products { get; set; } = null!;

        //Warehouseテーブル
        public DbSet<Warehouse> Warehouses { get; set; } = null!;

        //Locationテーブル
        public DbSet<Location> Locations { get; set; } = null!;

        //Stockテーブル
        public DbSet<Stock> Stocks { get; set; } = null!;

        //StockMovementテーブル
        public DbSet<StockMovement> StockMovements { get; set; } = null!;

        //Stocktakeテーブル
        public DbSet<Stocktake> Stocktakes { get; set; } = null!;
        
        //StocktakeLineテーブル
        public DbSet<StocktakeLine> StocktakeLines { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurationsフォルダ内の設定クラスを自動読み込み
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

        }
    }
}
