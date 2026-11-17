namespace AideaInventoryMng.DTO.Common
{
    public class PagedDateRangeRequest
    {
        // ページ番号
        public int Page { get; set; } = 1;

        // 1ページあたりの取得件数
        public int PageSize { get; set; } = 20;

        // 発生日時の開始（UTC）
        public DateTime? From { get; set; }

        // 発生日時の終了（UTC）
        public DateTime? To { get; set; }
    }
}
