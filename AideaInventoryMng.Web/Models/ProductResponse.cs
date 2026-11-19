namespace AideaInventoryMng.Web.Models
{
    public class ProductResponse
    {
        public long Id { get; set; }

        public string ProductCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Unit { get; set; } = string.Empty;

        public int? MinStockQuantity { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
