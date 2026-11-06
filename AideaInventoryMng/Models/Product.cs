using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Models
{
    public class Product
    {
        public int Id {  get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        //商品名
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // 商品説明
        [MaxLength(500)]
        public string? Description { get; set; }

        //販売価格
        [Precision(10,2)]
        [Range(typeof(decimal),"0", "99999999.99")]
        public decimal Price { get; set; }


        public int StockQty { get; set; }
        [Required]
        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int? MinStockQuantity { get; set; }

        public int Version { get; set; } = 1;

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //更新日時(UTC)
        public DateTime UpdatedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
