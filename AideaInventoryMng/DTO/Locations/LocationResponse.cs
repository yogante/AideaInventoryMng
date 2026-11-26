
namespace AideaInventoryMng.DTO.Locations
{
    public class LocationResponse
    {
        public long Id { get; set; }

        // 所属する倉庫ID
        public long WarehouseId { get; set; }

        // 所属する倉庫名
        public string WarehouseName { get; set; } = string.Empty;

        // 保管場所コード
        public string LocationCode { get; set; } = string.Empty;

        // 保管場所名
        public string LocationName { get; set; } = string.Empty;

        // 登録日時(UTC)
        public DateTime CreatedAt { get; set; }

        // 更新日時(UTC)
        public DateTime UpdatedAt { get; set; }

        // 楽観的同時実行制御
        public int Version { get; set; }
    }
}
