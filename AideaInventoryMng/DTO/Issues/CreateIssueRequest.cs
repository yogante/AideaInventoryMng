using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Issues
{
    public class CreateIssueRequest
    {
        // 商品ID
        [Range(1, long.MaxValue)]
        public long ProductId { get; set; }

        // ロケーションID
        [Range(1, long.MaxValue)]
        public long LocationId { get; set; }

        // 出庫数量
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // 出庫発生日時（省略可能）
        public DateTime? OccurredAt { get; set; }

        // 備考
        [MaxLength(200)]
        public string? Remarks { get; set; }
    }
}
