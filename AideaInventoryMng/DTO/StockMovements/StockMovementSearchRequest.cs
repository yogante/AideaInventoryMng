using AideaInventoryMng.DTO.Common;


namespace AideaInventoryMng.DTO.StockMovements
{
    public class StockMovementSearchRequest : PagedDateRangeRequest
    {

        public long? ProductId { get; set; }

        public long? WarehouseId { get; set; }

        public long? LocationId { get; set; }

        public string? MovementType { get; set; } 

    }
}
