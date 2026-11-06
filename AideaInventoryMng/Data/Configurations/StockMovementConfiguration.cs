using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            // テーブル名とCHECK制約
            builder.ToTable("stock_movements", table =>
            {
                // 数量変動は0以外
                table.HasCheckConstraint(
                    "ck_stock_movements_quantity_change",
                    "quantity_change <> 0");

                // 入出庫種別を5種類に限定
                table.HasCheckConstraint(
                    "ck_stock_movements_movement_type",
                    """
                    movement_type IN (
                        'RECEIPT',
                        'ISSUE',
                        'TRANSFER_OUT',
                        'TRANSFER_IN',
                        'STOCKTAKE_ADJUST'
                    )
                    """);

                // 入出庫種別と数量変動の正負をチェック
                table.HasCheckConstraint(
                    "ck_stock_movements_type_quantity",
                    """
                    (
                        movement_type IN ('RECEIPT', 'TRANSFER_IN')
                        AND quantity_change > 0
                    )
                    OR
                    (
                        movement_type IN ('ISSUE', 'TRANSFER_OUT')
                        AND quantity_change < 0
                    )
                    OR
                    (
                        movement_type = 'STOCKTAKE_ADJUST'
                        AND quantity_change <> 0
                    )
                    """);

            });

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.ProductId).HasColumnName("product_id");
            builder.Property(s => s.LocationId).HasColumnName("location_id");
            builder.Property(s => s.MovementType).HasColumnName("movement_type");
            builder.Property(s => s.QuantityChange).HasColumnName("quantity_change");
            builder.Property(s => s.TransferNo).HasColumnName("transfer_no");
            builder.Property(s => s.OccurredAt).HasColumnName("occurred_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.Remarks).HasColumnName("remarks");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.CreatedBy).HasColumnName("created_by");

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
