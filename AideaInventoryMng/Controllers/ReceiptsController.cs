using AideaInventoryMng.Models;
using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Receipts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt(
            [FromBody] CreateReceiptRequest request)
        {
            // ① 商品の存在確認（論理削除済み商品は除外）
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

            // ② ロケーションの存在確認（論理削除済みは除外）
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

            // ③ 倉庫の存在確認（論理削除済みは除外）
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == location.WarehouseId);
            if (warehouse  == null)
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

            // ⑤ ロケーション行をロック（SELECT FOR UPDATE）
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
                    message = "対象倉庫は棚卸中のため、入庫できません。"
                });
            }

            // ⑦ 対象商品の在庫レコードを取得
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s =>
                    s.ProductId == request.ProductId &&
                    s.LocationId == request.LocationId);

            // 現在時刻
            var now = DateTime.UtcNow;

            // ⑧ 在庫レコードが存在しない場合は新規作成
            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = request.ProductId,
                    LocationId = request.LocationId,
                    Quantity = request.Quantity,
                    ReservedQuantity = 0,
                    Version = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Stocks.Add(stock);
            }
            else
            {
                // ⑨ 整数オーバーフローを防止
                if (request.Quantity > int.MaxValue - stock.Quantity)
                {
                    return Conflict(new
                    {
                        code = "INVALID_STATE",
                        message = "入庫後の在庫数量が上限を超えるため、入庫できません。"
                    });
                }
                
                // 既存在庫に入庫数量を加算
                stock.Quantity += request.Quantity;

                // 楽観的同時実行制御用のバージョンを更新
                stock.Version++;

                // 更新日時
                stock.UpdatedAt = now;
            }

            // ⑩ 入庫履歴を作成
            var movement = new StockMovement
            {
                ProductId = request.ProductId,
                LocationId = request.LocationId,
                MovementType = "RECEIPT",
                QuantityChange = request.Quantity,
                OccurredAt = request.OccurredAt ?? now,
                Remarks = request.Remarks,
                CreatedAt = now

            };

            _context.StockMovements.Add(movement);

            try
            {
                // ⑪ 在庫と入庫履歴をDBに保存
                await _context.SaveChangesAsync();

                // ⑫ トランザクションを確定
                await transaction.CommitAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                // 同時更新競合が発生した場合はロールバック
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    code = "VERSION_CONFLICT",
                    message = "他の処理によって在庫が更新されています。最新の在庫情報を取得して再度実行してください。"
                });
            }

            // ⑬ レスポンスを作成
            var response = new ReceiptResponse
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
                    AvailableQuantity = stock.Quantity - stock.ReservedQuantity,
                    Version = stock.Version
                }
            };

            // ⑭ HTTP 201 Createdを返す
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
