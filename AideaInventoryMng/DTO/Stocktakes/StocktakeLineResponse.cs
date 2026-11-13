namespace AideaInventoryMng.DTO.Stocktakes
{
    public class StocktakeLineResponse
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public long LocationId { get; set; }

        public string LocationCode { get; set; } = string.Empty;

        public int BookQuantity { get; set; }

        public int? ActualQuantity { get; set; }

        public int? DifferenceQuantity { get; set; }

    
    
    }
}
