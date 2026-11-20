using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.Web.Models
{
    public class CreateProductRequest
    {
            // 商品コード
            [Required]
            [MaxLength(50)]
            public string ProductCode { get; set; } = string.Empty;

            // 商品名
            [Required]
            [MaxLength(200)]
            public string Name { get; set; } = string.Empty;

            // 商品説明
            [MaxLength(500)]
            public string? Description { get; set; }

            // 販売価格
            [Range(typeof(decimal), "0", "99999999.99")]
            public decimal Price { get; set; }

            // 単位
            [Required]
            [MaxLength(20)]
            public string Unit { get; set; } = string.Empty;

            // 最低在庫数
            [Range(0, int.MaxValue)]
            public int? MinStockQuantity { get; set; }
        }
    }


