namespace AideaInventoryMng.DTO.StockMovements
{
    public class StockMovementDetailResponse
    {

        public long Id { get; set; }

        // 商品ID
        public long ProductId { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public long WarehouseId { get; set; }

        public string WarehouseCode { get; set; } = string.Empty;

        // ロケーションID
        public long LocationId { get; set; }

        public string LocationCode { get; set; } = string.Empty;

        public string MovementType { get; set; } = string.Empty;

        // 在庫増減数量
        public int QuantityChange { get; set; }

        // 移動番号（移動以外はNULL）
        public long? TransferNo { get; set; }

        //在庫変動の発生日時
        public DateTime OccurredAt { get; set; }

        // 備考
        public string? Remarks { get; set; }

        //登録日時(UTC)
        public DateTime CreatedAt { get; set; }


    }
}
