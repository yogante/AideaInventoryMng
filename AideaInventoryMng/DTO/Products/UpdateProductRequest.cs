using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Products
{
    public class UpdateProductRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // 商品説明
        [MaxLength(500)]
        public string? Description { get; set; }

        // 販売価格
        [Range(typeof(decimal), "0", "99999999.99")]
        public decimal Price { get; set; }

        // 在庫数
        [Range(0, int.MaxValue)]
        public int StockQty { get; set; }
    }
}
