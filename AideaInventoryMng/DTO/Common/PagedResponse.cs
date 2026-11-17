namespace AideaInventoryMng.DTO.Common
{
    public class PagedResponse<T>
    {
        //検索結果
        public List<T> Items { get; set; } = new();

        // 検索条件に一致した総件数
        public int TotalCount { get; set; }

        // ページ番号
        public int Page { get; set; }

        // 1ページ当たりの取得件数
        public int PageSize { get; set; }

        // 総ページ数
        public int TotalPages =>
            PageSize > 0 
            ? (int)Math.Ceiling((double)TotalCount / PageSize) 
            : 0;

    }
}
