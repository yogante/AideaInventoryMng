namespace AideaInventoryMng.Models
{
    public class StocktakeLine
    {
        public long Id { get; set; }

        // 倉庫ID
        public long StocktakeId { get; set; }

        //stocktake との関連
        public Stocktake Stocktake { get; set; } = null!;
        
        // 商品ID
        public long ProductId { get; set; }

        // 商品との関連
        public Product Product { get; set; } = null!;

        // ロケーションID
        public long LocationId { get; set; }

        // ロケーションとの関連
        public Location Location { get; set; } = null!;

        //帳簿数量
        public int BookQuantity {  get; set; }

        //実在庫数量
        public int? ActualQuantity { get; set; }

        //差異数量
        public int? DifferenceQuantity { get; set; }

        // 作成日時（UTC）
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 更新日時（UTC）
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 作成者ID
        public long? CreatedBy { get; set; }

        // 更新者ID
        public long? UpdatedBy { get; set; }
    }
}
