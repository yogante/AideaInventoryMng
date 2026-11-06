using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Models
{
    public class StockMovement
    {
        public long Id { get; set; }

        // 商品ID
        public long ProductId { get; set; }

        // 商品との関連
        public Product Product { get; set; } = null!;

        // ロケーションID
        public long LocationId { get; set; }

        // ロケーションとの関連
        public Location Location { get; set; } = null!;

        // 入出庫種別
        [Required]
        [MaxLength(30)]
        public string MovementType { get; set; } = string.Empty;

        // 在庫増減数量
        public int QuantityChange { get; set; }

        // 移動番号（移動以外はNULL）
        public long? TransferNo { get; set; }

        //在庫変動の発生日時
        public DateTime OccurredAt { get; set; }

        // 備考
        [MaxLength(200)]
        public string? Remarks { get; set; } 

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public long? CreatedBy { get; set; }


    }
}
