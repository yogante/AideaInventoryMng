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
                // .AsNoTracking()
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

        [HttpGet("{id:long}")]
        public async Task<ActionResult<StocktakeDetailResponse>> GetStocktake(long id)
        {
            // ① 棚卸ヘッダーを取得
            var stocktake = await _context.Stocktakes
                .FirstOrDefaultAsync(s => s.Id == id);

            // ② 存在確認
            if (stocktake == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "棚卸データが存在しません。"

                });
            }

            // ③ 棚卸明細を取得
            var lines = await _context.StocktakeLines
                .Where(l => l.StocktakeId == id)
                .Select(l => new StocktakeLineResponse
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductCode = l.Product.ProductCode,
                    ProductName = l.Product.Name,
                    LocationId = l.LocationId,
                    LocationCode = l.Location.LocationCode,
                    BookQuantity = l.BookQuantity,
                    ActualQuantity = l.ActualQuantity,
                    DifferenceQuantity = l.DifferenceQuantity
                }).ToListAsync();

            // ④ レスポンスを作成
            var response = new StocktakeDetailResponse
            {
                Id = stocktake.Id,
                WarehouseId = stocktake.WarehouseId,
                StocktakeDate = stocktake.StocktakeDate,
                Status = stocktake.Status,
                Lines = lines,
            };

            return Ok(response);
        }

        [HttpPut("{id:long}/lines/{lineId:long}")]
        public async Task<ActionResult<StocktakeLineResponse>> UpdateStocktakeLine(
            long id,
            long lineId, 
            [FromBody] UpdateStocktakeLineRequest request)
        {
            var stocktake = await _context.Stocktakes.FirstOrDefaultAsync(s => s.Id == id);

            //Stocktakeが存在しない場合は404を返す
            if (stocktake == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "棚卸データが存在しません。"
                });
            }

            // ③ 棚卸ステータスの確認
            if (stocktake.Status != "IN_PROGRESS")
            {
                return Conflict(new
                {
                    code = "INVALID_STOCKTAKE_STATUS",
                    message = "実棚数量を更新できるのはIN_PROGRESS状態のデータのみです。"
                });
            }

            // ④ 実棚数量の入力確認
            if (request.ActualQuantity < 0 )
            {
                return BadRequest(new
                {
                    code = "INVALID_ACTUAL_QUANTITY",
                    message = "実棚数量は0以上を指定してください。"
                });
            }


            // ⑤ 棚卸明細を取得
            var line = await _context.StocktakeLines
                .Include(l => l.Product)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(l =>
                l.Id == lineId &&
                l.StocktakeId == id);

            // ⑥ 棚卸明細の存在確認
            if (line == null)
            {
                return NotFound(new
                {
                    code = "NOT_FOUND",
                    message = "棚卸明細が存在しません。"
                });
            }

            // ⑦ 実棚数量と棚卸差異を更新
            line.ActualQuantity = request.ActualQuantity;
            line.DifferenceQuantity = request.ActualQuantity - line.BookQuantity;

            line.UpdatedAt = DateTime.UtcNow;

            // ⑧ DBに保存
            await _context.SaveChangesAsync();

            // ⑨ レスポンス作成
            var response = new StocktakeLineResponse
            {
                Id = line.Id,
                ProductId = line.ProductId,
                ProductCode = line.Product.ProductCode,
                ProductName = line.Product.Name,
                LocationId = line.LocationId,
                LocationCode = line.Location.LocationCode,
                BookQuantity = line.BookQuantity,
                ActualQuantity = line.ActualQuantity,
                DifferenceQuantity = line.DifferenceQuantity
            };

            return Ok(response);
        }

    }
}
