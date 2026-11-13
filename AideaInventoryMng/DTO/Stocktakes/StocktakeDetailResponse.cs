namespace AideaInventoryMng.DTO.Stocktakes
{
    public class StocktakeDetailResponse
    {
        public long Id { get; set; }

        public long WarehouseId { get; set; }

        public DateOnly StocktakeDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<StocktakeLineResponse> Lines { get; set; } = new();

    }
}
