using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        //倉庫名
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        //倉庫コード
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        // 住所
        [MaxLength(200)]
        public string? Address { get; set; }

        // 備考
        [MaxLength(500)]
        public string? Description { get; set; }

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //更新日時(UTC)
        public DateTime UpdatedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }


    }
}
