using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Models
{
    public class Stocktake
    {
        public long Id { get; set; }

        // 棚卸対象倉庫ID
        public long WarehouseId { get; set; }

        // 関連する倉庫
        public Warehouse Warehouse { get; set; } = null!;

        // 棚卸日
        public DateOnly StocktakeDate { get; set; }

        // 棚卸状態
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "DRAFT";

        // 作成日時（UTC）
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 更新日時（UTC）
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 作成者ID
        public long? CreatedBy { get; set; }

        // 更新者ID
        public long? UpdatedBy { get; set; }

       
    }
}
