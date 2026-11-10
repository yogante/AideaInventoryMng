using AideaInventoryMng.DTO.Stocks;

namespace AideaInventoryMng.DTO.Issues
{
    public class IssueResponse
    {
        // 出庫履歴ID
        public long MovementId { get; set; }

        // 更新後の在庫情報
        public StockResponse Stock { get; set; } = null!;
    }
}
