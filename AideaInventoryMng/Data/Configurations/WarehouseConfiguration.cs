using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class WarehouseConfiguration: IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("warehouses", table =>
            {
                table.HasCheckConstraint(
                    "ck_warehouses_version",
                    "version >= 1"
                );
            });

            // カラム名
            builder.Property(w => w.Id).HasColumnName("id");
            builder.Property(w => w.Name).HasColumnName("name");
            builder.Property(w => w.Code).HasColumnName("warehouse_code");
            builder.Property(w => w.Address).HasColumnName("address");
            builder.Property(w => w.Description).HasColumnName("description");
            builder.Property(w => w.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(w => w.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(w => w.CreatedBy).HasColumnName("created_by");
            builder.Property(w => w.UpdatedBy).HasColumnName("updated_by");
            builder.Property(w => w.DeletedAt).HasColumnName("deleted_at");
            builder.Property(w => w.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();
            
            // 倉庫コードの一意制約
            builder.HasIndex(w => w.Code)
                .IsUnique()
                .HasDatabaseName("uq_warehouses_warehouse_code");


            // 論理削除フィルター
            builder.HasQueryFilter(w => w.DeletedAt == null);
        }
    }
}
