using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Stocks;
using AideaInventoryMng.DTO.Stocktakes;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocktakesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StocktakesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<StocktakeResponse>> CreateStocktake(
            [FromBody] CreateStocktakeRequest request)
        {
            // ① 倉庫の存在確認
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.Id == request.WarehouseId);

            if (warehouse == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "倉庫が存在しないか、削除されています。"
                });
            }

            // ② 棚卸日の入力確認
            if (request.StocktakeDate == default)
            {
                return BadRequest(new
                {
                    code = "INVALID_STOCKTAKE_DATE",
                    message = "棚卸日を指定してください。"
                });
            }

            // ③ 現在時刻（UTC）
            var now = DateTime.UtcNow;

            // ④ 棚卸ヘッダーを作成
            var stocktake = new Stocktake
            {
                WarehouseId = request.WarehouseId,
                StocktakeDate = request.StocktakeDate,
                Status = "DRAFT",
                CreatedAt = now,
                UpdatedAt = now
            };

            // ⑤ DBに登録
            _context.Stocktakes.Add(stocktake);

            await _context.SaveChangesAsync();

            // ⑥ レスポンス作成
            var response = new StocktakeResponse
            {
                Id = stocktake.Id,
                WarehouseId = stocktake.WarehouseId,
                StocktakeDate = stocktake.StocktakeDate,
                Status = stocktake.Status,
                CreatedAt = stocktake.CreatedAt
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }


        [HttpPost("{id:long}/start")]
        public async Task<ActionResult<StocktakeResponse>> StartStocktake(long id)
        {
            var stocktake = await _context.Stocktakes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stocktake == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "棚卸データが存在しません。"
                });
            }

            // ② 棚卸ステータスの確認
            if (stocktake.Status != "DRAFT")
            {
                return Conflict(new
                {
                    code = "INVALID_STOCKTAKE_STATUS",
                    message = "棚卸開始できるのはDRAFT状態のデータのみです。"
                });
            }

            // ③ トランザクション開始
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // ④ 対象倉庫の有効なロケーションをロック
            var lockedLocationIds = await _context.Database
                .SqlQuery<long>($"""
                SELECT id AS "Value"
                FROM locations
                WHERE warehouse_id = {stocktake.WarehouseId}
                AND deleted_at IS NULL
                ORDER BY id
                FOR UPDATE
                """)
                .ToListAsync();

            // ⑤ 棚卸ステータスを再確認
            await _context.Entry(stocktake).ReloadAsync();

            if (stocktake.Status != "DRAFT")
            {
                return Conflict(new
                {
                    code = "INVALID_STOCKTAKE_STATUS",
                    message = "棚卸開始できるのはDRAFT状態のデータのみです。"
                });
            }
            // ⑤ 有効なロケーションの存在確認
            if (lockedLocationIds.Count == 0)
            {
                return Conflict(new
                {
                    code = "NO_ACTIVE_LOCATIONS",
                    message = "対象倉庫に有効なロケーションがありません。"
                });
            }

            // ⑥ 同じ倉庫で進行中の棚卸がないか確認
            var isAnotherStocktakeInProgress = await _context.Stocktakes
                .AnyAsync(s =>
                    s.WarehouseId == stocktake.WarehouseId &&
                    s.Id != stocktake.Id &&
                    s.Status == "IN_PROGRESS");

            if (isAnotherStocktakeInProgress)
            {
                return Conflict(new
                {
                    code = "STOCKTAKE_ALREADY_IN_PROGRESS",
                    message = "この倉庫では、すでに別の棚卸が進行中です。"
                });
            }

            // ⑦ 対象ロケーションの在庫データを取得
            var stocks = await _context.Stocks
                .Where(s => lockedLocationIds.Contains(s.LocationId))
                .ToListAsync();

            var now = DateTime.UtcNow;

            var stocktakeLines = stocks.Select(s => new StocktakeLine
            {
                StocktakeId = stocktake.Id,
                ProductId = s.ProductId,
                LocationId = s.LocationId,
                BookQuantity = s.Quantity,
                ActualQuantity = null,
                DifferenceQuantity = null,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            // 棚卸明細を登録対象に追加
            _context.StocktakeLines.AddRange(stocktakeLines);

            // ⑨ 棚卸ステータスを更新
            stocktake.Status = "IN_PROGRESS";
            stocktake.UpdatedAt = now;
            
            // ⑩ DBに保存
            await _context.SaveChangesAsync();

            // ⑪ トランザクションを確定
            await transaction.CommitAsync();

            var response = new StocktakeResponse
            {
                Id = stocktake.Id,
                WarehouseId = stocktake.WarehouseId,
                StocktakeDate = stocktake.StocktakeDate,
                Status = stocktake.Status,
                CreatedAt = stocktake.CreatedAt
            };

            return Ok(response);
        }

    }
}
