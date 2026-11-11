using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Reservations;
using AideaInventoryMng.DTO.Stocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("reserve")]
        public async Task<ActionResult<StockResponse>> Reserve (
            [FromBody] CreateReservationRequest request)
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
            var isStocktakeInProgress = await _context.Stocktakes
                .AnyAsync(s =>
                    s.WarehouseId == location.WarehouseId &&
                    s.Status == "IN_PROGRESS");

            if (isStocktakeInProgress)
            {
                return Conflict(new
                {
                    code = "STOCKTAKE_IN_PROGRESS",
                    message = "対象倉庫は棚卸中のため、在庫引当できません。"
                });
            }

            // ⑦ 対象商品の在庫レコードを取得
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s =>
                    s.ProductId == request.ProductId &&
                    s.LocationId == request.LocationId);

            if (stock == null)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_STOCK",
                    message = "対象商品の在庫が存在しないため、引当できません。"
                });
            }

            // ⑧ 引当可能数量を計算
            var availableQuantity =
                stock.Quantity - stock.ReservedQuantity;

            // 在庫の整合性確認
            if (availableQuantity < 0)
            {
                return Conflict(new
                {
                    code = "INVALID_STATE",
                    message = "在庫数量が引当数量を下回っています。"
                });
            }

            // ⑨ 引当可能数量を確認
            if (request.Quantity > availableQuantity)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_STOCK",
                    message = "引当可能数量を超えているため、引当できません。"
                });
            }

            // ⑩ 引当数量を加算
            stock.ReservedQuantity += request.Quantity;

            // バージョンを更新
            stock.Version++;

            // 更新日時
            stock.UpdatedAt = DateTime.UtcNow;

            try
            {
                // ⑪ 在庫をDBに保存
                await _context.SaveChangesAsync();

                // ⑫ トランザクションを確定
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

            // ⑬ レスポンスを作成
            var response = new StockResponse
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
            };


            return Ok(response);
        }

        [HttpPost("release")]
        public async Task<ActionResult<StockResponse>> Release (
            [FromBody] CreateReservationRequest request)
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
            var isStocktakeInProgress = await _context.Stocktakes
                .AnyAsync(s =>
                s.WarehouseId == location.WarehouseId &&
                s.Status == "IN_PROGRESS");

            if (isStocktakeInProgress)
            {
                return Conflict(new
                {
                    code = "STOCKTAKE_IN_PROGRESS",
                    message = "対象倉庫は棚卸中のため、引当解除できません"
                });
            }

            // ⑦ 対象商品の在庫レコードを取得
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s =>
                s.ProductId == request.ProductId &&
                s.LocationId == request.LocationId);

            if(stock == null)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_RESERVATION",
                    message = "対象商品の引当情報が存在しません。"
                });
            }

            // ⑧ 引当数量を確認
            if (request.Quantity > stock.ReservedQuantity)
            {
                return Conflict(new
                {
                    code = "INSUFFICIENT_RESERVATION",
                    message = "解除数量が現在の引当数量を超えています。"
                });
            }

            // ⑨ 引当数量を減算
            stock.ReservedQuantity -= request.Quantity;

            // バージョンを更新
            stock.Version++;

            // 更新日時
            stock.UpdatedAt = DateTime.UtcNow;

            try
            {
                // ⑩ 在庫をDBに保存
                await _context.SaveChangesAsync();

                // ⑪ トランザクションを確定
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

            var response = new StockResponse
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
            };

            return Ok(response); 
        }
    }
}
