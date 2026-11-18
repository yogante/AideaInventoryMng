using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Common;
using AideaInventoryMng.DTO.StockMovements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockMovementsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public StockMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<StockMovementListResponse>>> GetStockMovements(
            [FromQuery] StockMovementSearchRequest request)
        {

            // ページ番号のチェック
            if (request.Page < 1)
            {
                return BadRequest(new
                {
                    code = "INVALID_PAGE",
                    message = "ページ番号は1以上を指定してください。"
                });
            }

            // 1ページあたりの取得件数のチェック
            if (request.PageSize < 1 || request.PageSize > 100)
            {
                return BadRequest(new
                {
                    code = "INVALID_PAGE_SIZE",
                    message = "1ページあたりの取得件数は1～100を指定してください。"
                });
            }

            // 日付範囲のチェック
            if (request.From.HasValue &&
                request.To.HasValue &&
                request.From.Value > request.To.Value)
            {
                return BadRequest(new
                {
                    code = "INVALID_DATE_RANGE",
                    message = "開始日時は終了日時以前を指定してください。"
                });
            }

            // ① 在庫変動履歴を取得するクエリを作成
            var query = _context.StockMovements
                .IgnoreQueryFilters()
                .AsQueryable();

            // 商品IDで絞り込み
            if (request.ProductId.HasValue)
            {
                query = query.Where(p =>
                p.ProductId == request.ProductId.Value);
            }

            // 倉庫IDで絞り込み
            if (request.WarehouseId.HasValue)
            {
                query = query.Where(w => 
                w.Location.WarehouseId == request.WarehouseId.Value);
            }

            // ロケーションIDで絞り込み
            if (request.LocationId.HasValue)
            {
                query = query.Where(l =>
                l.LocationId == request.LocationId.Value);
            }

            // 在庫変動種別で絞り込み
            if (!string.IsNullOrWhiteSpace(request.MovementType))
            {
                query = query.Where(p =>
                p.MovementType == request.MovementType);
            }

            // 発生日時の開始で絞り込み
            if (request.From.HasValue)
            {
                query = query.Where(p =>
                    p.OccurredAt >= request.From.Value);
            }

            // 発生日時の終了で絞り込み
            if (request.To.HasValue)
            {
                query = query.Where(p =>
                    p.OccurredAt <= request.To.Value);
            }

            var totalCount = await query.CountAsync();

               var items = await query
                .OrderByDescending(p => p.OccurredAt)
                .ThenByDescending(p => p.Id)
                .Skip( (request.Page -1) * request.PageSize )
                .Take(request.PageSize)
                .Select(p => new StockMovementListResponse
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductCode = p.Product.ProductCode,
                    ProductName = p.Product.Name,
                    WarehouseId = p.Location.WarehouseId,
                    WarehouseCode = p.Location.Warehouse.Code,
                    LocationId = p.LocationId,
                    LocationCode = p.Location.LocationCode,
                    MovementType = p.MovementType,
                    QuantityChange = p.QuantityChange,
                    TransferNo = p.TransferNo,
                    OccurredAt = p.OccurredAt,
                    Remarks = p.Remarks,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();


            // ② 仮の空レスポンスを返す
            var response = new PagedResponse<StockMovementListResponse> 
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return Ok(response);

        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<StockMovementDetailResponse>> GetStockMovementDetailById(long id)
        {

            var item = await _context.StockMovements
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new StockMovementDetailResponse
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductCode = p.Product.ProductCode,
                    ProductName = p.Product.Name,
                    WarehouseId = p.Location.WarehouseId,
                    WarehouseCode = p.Location.Warehouse.Code,
                    LocationId = p.LocationId,
                    LocationCode = p.Location.LocationCode,
                    MovementType = p.MovementType,
                    QuantityChange = p.QuantityChange,
                    TransferNo = p.TransferNo,
                    OccurredAt = p.OccurredAt,
                    Remarks = p.Remarks,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "指定された在庫変動履歴が見つかりません。"
                });
            }

            return Ok(item);

        }

    }
}
