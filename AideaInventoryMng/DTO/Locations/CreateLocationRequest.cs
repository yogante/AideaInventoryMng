using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Locations
{
    public class CreateLocationRequest
    {
        // 所属する倉庫ID
        [Required]
        public long WarehouseId { get; set; }

        // 保管場所コード
        [Required]
        [MaxLength(30)]
        public string LocationCode { get; set; } = string.Empty;

        // 保管場所名
        [Required]
        [MaxLength(100)]
        public string LocationName { get; set; } = string.Empty;
    }
}
