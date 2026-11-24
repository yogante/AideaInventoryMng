using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Products
{
    public class ProductResponse
    {
        //ID
        public long Id { get; set; }

        //商品コード
        public string ProductCode { get; set; } = string.Empty;

        //商品名
        public string Name { get; set; } = string.Empty;

        // 商品説明
        public string? Description { get; set; }

        // 販売価格
        public decimal Price { get; set; }

        // 単位
        public string Unit { get; set; } = string.Empty;

        // 最低在庫数
        public int? MinStockQuantity { get; set; }

        // Version
        public int Version { get; set; }

        // 登録日時
        public DateTime CreatedAt { get; set; }

        // 更新日時
        public DateTime UpdatedAt { get; set; }
    }
}
