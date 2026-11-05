using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Products
{
    public class ProductResponse
    {
        //ID
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // 商品説明
        public string? Description { get; set; }

        // 販売価格
        public decimal Price { get; set; }

        // 在庫数
        public int StockQty { get; set; }

        // 登録日時
        public DateTime CreatedAt { get; set; }

        // 更新日時
        public DateTime UpdatedAt { get; set; }
    }
}
