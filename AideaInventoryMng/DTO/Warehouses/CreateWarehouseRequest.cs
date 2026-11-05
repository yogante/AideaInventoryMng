using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Warehouses
{
    public class CreateWarehouseRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        //倉庫コード
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        // 住所
        [MaxLength(200)]
        public string? Address { get; set; }

        // 備考
        [MaxLength(500)]
        public string? Description { get; set; }

    }
}

