using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Models
{
    public class Location
    {
        public long Id { get; set; }


        // 所属する倉庫ID
        public long WarehouseId { get; set; }

        // 所属する倉庫
        public Warehouse Warehouse { get; set; } = null!;

        // 保管場所コード
        [Required]
        [MaxLength(30)]
        public string LocationCode { get; set; } = string.Empty;

        // 保管場所名
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //更新日時(UTC)
        public DateTime UpdatedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? UpdatedBy { get; set; }
        
        // 論理削除日時
        public DateTime? DeletedAt { get; set; }
    }
}
