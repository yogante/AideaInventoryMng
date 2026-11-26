using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Locations
{
    public class UpdateLocationRequest
    {

        // 所属する倉庫ID
        public long WarehouseId { get; set; }

        // 保管場所コード
        [Required]
        [MaxLength(30)]
        public string LocationCode { get; set; } = string.Empty;

        // 保管場所名
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;

        // 楽観的同時実行制御
        [Required]
        public int Version { get; set; }
    }
}
