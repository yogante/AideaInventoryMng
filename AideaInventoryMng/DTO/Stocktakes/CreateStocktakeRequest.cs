namespace AideaInventoryMng.DTO.Stocktakes
{
    public class CreateStocktakeRequest
    {
        // 倉庫ID
        public long WarehouseId { get; set; }

        // 棚卸日
        public DateOnly StocktakeDate { get; set; }
    }
}
