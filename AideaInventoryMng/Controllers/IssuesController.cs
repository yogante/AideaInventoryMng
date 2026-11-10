using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Issues;
using AideaInventoryMng.DTO.Stocks;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateIssue(
             [FromBody] CreateIssueRequest request)
        {
            // ① 商品の存在確認
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "商品が存在しないか、削除されています。"
                });
            }

            // ② ロケーションの存在確認
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == request.LocationId);

            if (location == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "Locationが存在しないか、削除されています。"
                });
            }

            // ③ 倉庫の存在確認
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == location.WarehouseId);

            if (warehouse == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "倉庫が存在しないか、削除されています。"
                });
            }

            // ④ トランザクション開始
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // ⑤ ロケーション行をロック
            var lockedLocationIds = await _context.Database
                .SqlQuery<long>($"""
                SELECT id as "Value"
                FROM locations
                WHERE id = {request.LocationId}
                AND deleted_at IS NULL
                FOR UPDATE
                """)
                .ToListAsync();

            if (lockedLocationIds.Count == 0)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "Locationが存在しないか、削除されています。"
                });
            }

            // ⑥ 対象倉庫が棚卸中か確認
            var isStocktateInProgress = await _context.Stocktakes
                .AnyAsync(s =>
                s.WarehouseId == location.WarehouseId &&
                s.Status == "IN_PROGRESS");

            if (isStocktateInProgress)
            {
                return Conflict(new
                {
                    code = "STOCKTAKE_IN_PROGRESS",
                    message = "対象倉庫は棚卸中のため、出庫できません。"
                });
            }

            // ⑦ 対象商品の在庫レコードを取得
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s =>
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId);

            // 在庫レコードが存在しない場合
            if (stock == null)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_STOCK",
                    message = "対象商品の在庫が存在しないため、出庫できません。"
                });
            }

            // ⑧ 引当可能数量を計算
            var availableQuantity = stock.Quantity - stock.ReservedQuantity;

            // 在庫の整合性確認
            if (availableQuantity < 0)
            {
                return Conflict(new
                {
                    code = "INVALID_STATE",
                    message = "在庫数量が引当数量を下回っています。"
                });
            }

            // ⑨ 出庫可能数量を確認
            if (request.Quantity > availableQuantity)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_STOCK",
                    message = "引当可能数量を超えているため、出庫できません。"
                });
            }

            // 現在時刻
            var now = DateTime.UtcNow;

            // ⑩ 在庫数量を減算
            stock.Quantity -= request.Quantity;

            // 楽観的同時実行制御用のバージョンを更新
            stock.Version++;

            // 更新日時
            stock.UpdatedAt = now;

            // ⑪ 出庫履歴を作成
            var movement = new StockMovement
            {
                ProductId = request.ProductId,
                LocationId = request.LocationId,
                MovementType = "ISSUE",
                QuantityChange = -request.Quantity,
                OccurredAt = request.OccurredAt ?? now,
                Remarks = request.Remarks,
                CreatedAt = now
            };

            _context.StockMovements.Add(movement);

            try{
                // ⑫ 在庫と出庫履歴をDBに保存
                await _context.SaveChangesAsync();

                // ⑬ トランザクションを確定
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    code = "VERSION_CONFLICT",
                    message = "他の処理によって在庫が更新されています。最新の在庫情報を取得して再度実行してください。"
                });
            }

            // ⑭ レスポンスを作成
            var response = new IssueResponse
            {
                MovementId = movement.Id,
                Stock = new StockResponse
                {
                    Id = stock.Id,
                    ProductId = product.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.Name,
                    WarehouseId = warehouse.Id,
                    WarehouseCode = warehouse.Code,
                    LocationId = location.Id,
                    LocationCode = location.LocationCode,
                    Quantity = stock.Quantity,
                    ReservedQuantity = stock.ReservedQuantity,
                    AvailableQuantity =
                        stock.Quantity - stock.ReservedQuantity,
                    Version = stock.Version
                }

            };

            return CreatedAtAction(
                 nameof(GetIssue),
                 new { id = movement.Id },
                 response
             );
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetIssue(long id)
        {
            // ① 出庫履歴を取得
            var movement = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m=> m.Location)
                .ThenInclude(l => l.Warehouse)
                .FirstOrDefaultAsync(m =>
                m.Id == id &&
                m.MovementType == "ISSUE");

            // ② 出庫履歴が存在しない場合
            if (movement == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "指定された出庫履歴が存在しません。"
                });
            }

            // ③ レスポンスを作成
            var response = new IssueDetailResponse
            {
                MovementId = movement.Id,

                ProductId = movement.ProductId,
                ProductCode = movement.Product.ProductCode,
                ProductName = movement.Product.Name,

                WarehouseId = movement.Location.WarehouseId,
                WarehouseCode = movement.Location.Warehouse.Code,
                WarehouseName = movement.Location.Warehouse.Name,

                LocationId = movement.LocationId,
                LocationCode = movement.Location.LocationCode,
                LocationName = movement.Location.LocationName,

                Quantity = -movement.QuantityChange,
                OccurredAt = movement.OccurredAt,
                Remarks = movement.Remarks,
                CreatedAt = movement.CreatedAt

            };

            // ④ HTTP 200 OK
            return Ok(response);

        }

    }
}
