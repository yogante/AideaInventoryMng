using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            // カラム名
            builder.Property(l => l.Id).HasColumnName("id");
            builder.Property(l => l.WarehouseId).HasColumnName("warehouse_id");
            builder.Property(l => l.LocationCode).HasColumnName("location_code");
            builder.Property(l => l.LocationName).HasColumnName("location_name");
            builder.Property(l => l.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(l => l.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(l => l.CreatedBy).HasColumnName("created_by");
            builder.Property(l => l.UpdatedBy).HasColumnName("updated_by");
            builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");

            // 倉庫内のロケーションコードを一意にする
            builder.HasIndex(l => new { l.WarehouseId, l.LocationCode })
                .IsUnique()
                .HasDatabaseName("uq_locations_warehouse_location_code");

            // Warehouseとのリレーション
            builder.HasOne(l => l.Warehouse)
                .WithMany()
                .HasForeignKey(l => l.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 論理削除フィルター
            builder.HasQueryFilter(l => l.DeletedAt == null);
        }
    }
}
