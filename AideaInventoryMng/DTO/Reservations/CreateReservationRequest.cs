using System.ComponentModel.DataAnnotations;

namespace AideaInventoryMng.DTO.Reservations
{
    public class CreateReservationRequest
    {
        [Range(1, long.MaxValue)]
        public long ProductId { get; set; }

        [Range(1, long.MaxValue)]
        public long LocationId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
