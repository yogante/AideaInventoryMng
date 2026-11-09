namespace AideaInventoryMng.DTO.Receipts
{
    public class StockResponse
    {
        // 在庫ID
        public long Id { get; set; }

        // 商品ID
        public long ProductId { get; set; }

        // 商品コード
        public string ProductCode { get; set; } = string.Empty;

        // 商品名
        public string ProductName { get; set; } = string.Empty;

        // 倉庫ID
        public long WarehouseId { get; set; }

        // 倉庫コード
        public string WarehouseCode { get; set; } = string.Empty;

        // ロケーションID
        public long LocationId { get; set; }

        // ロケーションコード
        public string LocationCode { get; set; } = string.Empty;

        // 在庫数量
        public int Quantity { get; set; }

        // 引当数量
        public int ReservedQuantity { get; set; }

        // 引当可能数量
        public int AvailableQuantity { get; set; }

        // バージョン
        public int Version { get; set; }
   
    }
}
