using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stocks", table =>
            {
                table.HasCheckConstraint(
                                    "ck_stocks_quantity",
                                    "quantity >= 0");

                table.HasCheckConstraint(
                    "ck_stocks_reserved_quantity",
                    "reserved_quantity >= 0");

                table.HasCheckConstraint(
                    "ck_stocks_reserved_lte_quantity",
                    "reserved_quantity <= quantity");

                table.HasCheckConstraint(
                    "ck_stocks_version",
                    "version >= 1");
            });

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.ProductId).HasColumnName("product_id");
            builder.Property(s => s.LocationId).HasColumnName("location_id");
            builder.Property(s => s.Quantity).HasColumnName("quantity").HasDefaultValue(0);
            builder.Property(s => s.ReservedQuantity).HasColumnName("reserved_quantity").HasDefaultValue(0);

            // 楽観的同時実行制御
            builder.Property(s=> s.Version)
                .HasColumnName("version")
                .HasDefaultValue(1)
                .IsConcurrencyToken();

            builder.Property(s => s.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.CreatedBy).HasColumnName("created_by");
            builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");
            
            builder.HasIndex( s => new { s.ProductId, s.LocationId})
                .IsUnique()
                .HasDatabaseName("uq_stocks_product_location");

            // Productとのリレーション
            builder.HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Locationとのリレーション
            builder.HasOne(s => s.Location)
                .WithMany()
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
