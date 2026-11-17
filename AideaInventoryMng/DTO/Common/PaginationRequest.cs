namespace AideaInventoryMng.DTO.Common
{
    public class PaginationRequest
    {
        // ページ番号
        public int Page { get; set; } = 1;

        // 1ページあたりの取得件数
        public int PageSize { get; set; } = 20;
    }
}
