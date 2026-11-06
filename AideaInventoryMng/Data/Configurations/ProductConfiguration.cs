using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace AideaInventoryMng.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {

            // 二重に書かないようにすること　builder.ToTable("products");
            // 販売価格のCHECK制約
            builder.ToTable("products", table =>
            {
                table.HasCheckConstraint(
                    "ck_products_price"
                    , "price >= 0"
                    );

                table.HasCheckConstraint(
                    "ck_products_min_stock_quantity",
                    "min_stock_quantity IS NULL OR min_stock_quantity >= 0"
                );

                table.HasCheckConstraint(
                    "ck_products_version",
                    "version >= 1"
                );
            });

            builder.Property(p => p.Id).HasColumnName("id");
            builder.Property(p => p.ProductCode).HasColumnName("product_code");
            builder.Property(p => p.Name).HasColumnName("name");
            builder.Property(p => p.Description).HasColumnName("description");
            builder.Property(p => p.Price).HasColumnName("price");
            builder.Property(p => p.Unit).HasColumnName("unit");
            builder.Property(p => p.MinStockQuantity).HasColumnName("min_stock_quantity");
            // 楽観的同時実行制御IsConcurrencyToken()
            builder.Property(p => p.Version).HasColumnName("version")
                .HasDefaultValue(1)
                .IsConcurrencyToken();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.CreatedBy).HasColumnName("created_by");
            builder.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

            // 商品コードの一意制約
            builder.HasIndex(p => p.ProductCode).IsUnique().HasDatabaseName("uq_products_product_code");

            // 論理削除フィルター
            builder.HasQueryFilter(p => p.DeletedAt == null);
        }
    }
}
