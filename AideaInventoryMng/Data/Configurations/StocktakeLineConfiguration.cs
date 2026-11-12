using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class StocktakeLineConfiguration : IEntityTypeConfiguration<StocktakeLine>
    {
        public void Configure(EntityTypeBuilder<StocktakeLine> builder)
        {
            builder.ToTable("stocktake_lines", table => {
                // 帳簿数量は0以上
                table.HasCheckConstraint(
                    "ck_stocktake_lines_book_quantity",
                    "book_quantity >= 0");

                // 実在庫数量はNULLまたは0以上
                table.HasCheckConstraint(
                    "ck_stocktake_lines_actual_quantity",
                    "actual_quantity IS NULL OR actual_quantity >= 0");

                // 差異数量と実在庫数量の整合性
                table.HasCheckConstraint(
                    "ck_stocktake_lines_difference_quantity",
                    """
                    (
                    actual_quantity IS NULL
                    AND difference_quantity IS NULL
                    ) OR (
                        actual_quantity IS NOT NULL
                        AND difference_quantity IS NOT NULL
                        AND difference_quantity = actual_quantity - book_quantity
                    )
                    """);
            });

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.StocktakeId).HasColumnName("stocktake_id");
            builder.Property(s => s.ProductId).HasColumnName("product_id");
            builder.Property(s => s.LocationId).HasColumnName("location_id");
            builder.Property(s => s.BookQuantity).HasColumnName("book_quantity");
            builder.Property(s => s.ActualQuantity).HasColumnName("actual_quantity");
            builder.Property(s => s.DifferenceQuantity).HasColumnName("difference_quantity");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.CreatedBy).HasColumnName("created_by");
            builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");

            builder.HasOne(s => s.Stocktake)
                .WithMany()
                .HasForeignKey(s => s.StocktakeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Location)
                .WithMany()
                .HasForeignKey(s => s.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new
            {
                s.StocktakeId,
                s.ProductId,
                s.LocationId
            }).IsUnique()
            .HasDatabaseName("uq_stocktake_lines_stocktake_product_location");
        }
    }
}
