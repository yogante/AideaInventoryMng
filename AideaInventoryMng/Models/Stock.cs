using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Models
{
    public class Stock
    {
        public long Id { get; set; }

        // 商品ID
        public long ProductId { get; set; }

        // ロケーションID
        public long LocationId { get; set; }

        // 在庫数量

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        //引当数量
        [Range(0, int.MaxValue)]
        public int ReservedQuantity { get; set; }

        // 楽観的同時実行制御
        public int Version { get; set; } = 1;

        // 登録日時（UTC）
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 更新日時（UTC）
        public DateTime UpdatedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? UpdatedBy { get; set; }

        // 商品との関連
        public Product Product { get; set; } = null!;

        // ロケーションとの関連
        public Location Location { get; set; } = null!;
    }
}
