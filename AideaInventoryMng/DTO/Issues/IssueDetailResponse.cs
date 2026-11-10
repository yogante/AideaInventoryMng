namespace AideaInventoryMng.DTO.Issues
{
    public class IssueDetailResponse
    {
        // 出庫履歴ID
        public long MovementId { get; set; }

        //商品情報
        public long ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;


        //倉庫情報
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;

        // Location情報
        public long LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;

        //出庫情報
        public int Quantity { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Remarks { get; set; }

        // 登録日時
        public DateTime CreatedAt { get; set; }
    }
}
