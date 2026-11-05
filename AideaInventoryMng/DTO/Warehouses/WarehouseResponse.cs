using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Warehouses
{
    public class WarehouseResponse
    {
        public int Id { get; set; }

        //商品名
        public string Name { get; set; } = string.Empty;

        //倉庫コード
        public string Code { get; set; } = string.Empty;

        // 住所
        public string? Address { get; set; }

        // 備考
        public string? Description { get; set; }

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //更新日時(UTC)
        public DateTime UpdatedAt { get; set; }
    }
}
