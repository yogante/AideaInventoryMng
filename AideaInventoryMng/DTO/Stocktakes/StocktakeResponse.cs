namespace AideaInventoryMng.DTO.Stocktakes
{
    public class StocktakeResponse
    {
        // 棚卸ID
        public long Id { get; set; }

        // 倉庫ID
        public long WarehouseId { get; set; }

        // 棚卸日
        public DateOnly StocktakeDate { get; set; }

        // 棚卸ステータス
        public string Status { get; set; } = string.Empty;

        // 棚卸開始日時
        public DateTime CreatedAt { get; set; }
    }
}
