using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Receipts
{
    public class CreateReceiptRequest
    {
        // 商品ID
        [Range(1, long.MaxValue)]
        public long ProductId { get; set; }

        // ロケーションID
        [Range(1, long.MaxValue)]
        public long LocationId { get; set; }

        // 入庫数量
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // 入庫発生日時（省略可能）
        public DateTime? OccurredAt { get; set; }

        // 備考
        [MaxLength(200)]
        public string? Remarks { get; set; }
    }
}
