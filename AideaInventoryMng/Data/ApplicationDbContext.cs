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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                // 二重に書かないようにすること　entity.ToTable("products");
                // 販売価格のCHECK制約
                entity.ToTable("products", table =>
                {
                    table.HasCheckConstraint(
                        "ck_products_price"
                        , "price >= 0"
                        );
                });

                entity.Property(p => p.Id).HasColumnName("id");
                entity.Property(p => p.ProductCode).HasColumnName("product_code");
                entity.Property(p => p.Name).HasColumnName("name");
                entity.Property(p => p.Description).HasColumnName("description");
                entity.Property(p => p.Price).HasColumnName("price");
                entity.Property(p => p.Unit).HasColumnName("unit");
                entity.Property(p => p.MinStockQuantity).HasColumnName("min_stock_quantity");
                // 楽観的同時実行制御IsConcurrencyToken()
                entity.Property(p => p.Version).HasColumnName("version").IsConcurrencyToken();
                entity.Property(p => p.CreatedAt).HasColumnName("created_at");
                entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");
                entity.Property(p => p.CreatedBy).HasColumnName("created_by");
                entity.Property(p => p.UpdatedBy).HasColumnName("updated_by");
                entity.Property(p => p.DeletedAt).HasColumnName("deleted_at");

                // 商品コードの一意制約
                entity.HasIndex(p => p.ProductCode).IsUnique().HasDatabaseName("uq_products_product_code");

            });

            // 論理削除フィルター
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.DeletedAt == null);


            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("warehouses");

                // カラム名
                entity.Property(w => w.Id).HasColumnName("id");
                entity.Property(w => w.Name).HasColumnName("name");
                entity.Property(w => w.Code).HasColumnName("warehouse_code");
                entity.Property(w => w.Address).HasColumnName("address");
                entity.Property(w => w.Description).HasColumnName("description");
                entity.Property(w => w.CreatedAt).HasColumnName("created_at");
                entity.Property(w => w.UpdatedAt).HasColumnName("updated_at");
                entity.Property(w => w.CreatedBy).HasColumnName("created_by");
                entity.Property(w => w.UpdatedBy).HasColumnName("updated_by");
                entity.Property(w => w.DeletedAt).HasColumnName("deleted_at");

                // 倉庫コードの一意制約
                entity.HasIndex(w => w.Code)
                    .IsUnique()
                    .HasDatabaseName("uq_warehouses_warehouse_code");
            });

            // 論理削除フィルター
            modelBuilder.Entity<Warehouse>()
                .HasQueryFilter(w => w.DeletedAt == null);
        }
    }
}
