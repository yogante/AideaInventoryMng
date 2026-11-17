namespace AideaInventoryMng.DTO.Common
{
    public class DateRangeRequest
    {
        // 開始日時（UTC）
        public DateTime? From { get; set; }

        // 終了日時（UTC）
        public DateTime? To { get; set; }
    }
}

