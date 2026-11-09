namespace AideaInventoryMng.DTO.Receipts
{
    public class ReceiptResponse
    {        
        // 入庫履歴ID
        public long MovementId { get; set; }

        // 更新後の在庫情報
        public StockResponse Stock { get; set; } = null!;
    }
}
