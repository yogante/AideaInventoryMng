using AideaInventoryMng.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AideaInventoryMng.Data.Configurations
{
    public class StocktakeConfiguration : IEntityTypeConfiguration<Stocktake>
    {
        public void Configure(EntityTypeBuilder<Stocktake> builder)
        {
            builder.ToTable("stocktakes", table =>
            {
                table.HasCheckConstraint(
                    "ck_stocktakes_status",
                    """
                    status IN (
                    'DRAFT'
                    ,'IN_PROGRESS'
                    ,'COMPLETED'
                    ,'CANCELLED'
                    )
                    """);
            }
                );

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id");
            builder.Property(s => s.StocktakeDate).HasColumnName("stocktake_date");
            builder.Property(s => s.Status).HasColumnName("status").HasDefaultValue("DRAFT");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(s => s.CreatedBy).HasColumnName("created_by");
            builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");

            //warehouseとリレーション
            builder.HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
                
        }
    }
}
