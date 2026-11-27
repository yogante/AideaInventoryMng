using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Locations;
using AideaInventoryMng.DTO.Warehouses;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ロケーション一覧取得
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationResponse>>> GetLocations()
        {
            var locations = await _context.Locations
                .Where(l => l.DeletedAt == null)
                .Select(l => new LocationResponse
                {
                    Id = l.Id,
                    WarehouseId = l.WarehouseId,
                    WarehouseName = l.Warehouse.Name,
                    LocationCode = l.LocationCode,
                    LocationName = l.LocationName,
                    Version = l.Version,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                }).ToListAsync();

            return Ok(locations);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<LocationResponse>> GetLocation(int id)
        {
            var location = await _context.Locations
                  .Where(l => l.Id == id && l.DeletedAt == null)
                  .Select(l => new LocationResponse
                  {
                      Id = l.Id,
                      WarehouseId = l.WarehouseId,
                      WarehouseName = l.Warehouse.Name,
                      LocationCode = l.LocationCode,
                      LocationName = l.LocationName,
                      Version = l.Version,
                      CreatedAt = l.CreatedAt,
                      UpdatedAt = l.UpdatedAt,
                  })
                  .FirstOrDefaultAsync();

            // 存在しない場合は404を返す
            if (location == null)
            {
                return NotFound();
            }

            // 存在する場合は200を返す
            return Ok(location);


        }

        // ロケーション登録
        [HttpPost]
        public async Task<ActionResult<LocationResponse>> CreateLocation(
           CreateLocationRequest request)
        {
            // 倉庫IDの入力チェック
            if (request.WarehouseId <= 0)
            {
                return BadRequest(new
                {
                    message = "倉庫IDを正しく指定してください。"
                });
            }

            // 倉庫の存在確認（論理削除済みの倉庫は対象外）

            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                w.Id == request.WarehouseId &&
                w.DeletedAt == null);

            if (warehouse == null)
            {
                return BadRequest(new
                {
                    message = "指定された倉庫が存在しません。"
                });
            }

            // 同一倉庫内のロケーションコード重複確認
            var exists = await _context.Locations
                .AnyAsync(l =>
                l.WarehouseId == request.WarehouseId &&
                l.LocationCode == request.LocationCode);

            if (exists)
            {
                return Conflict(new
                {
                    message = "この倉庫には同じロケーションコードが登録されています。"
                });
            }

            // ロケーション登録
            var location = new Location
            {
                WarehouseId = request.WarehouseId,
                LocationCode = request.LocationCode,
                LocationName = request.LocationName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = 1
            };

            _context.Locations.Add(location);

            try
            {
                //データベースに保存
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx &&
            pgEx.ConstraintName == "uq_locations_warehouse_location_code")
            {
                return Conflict(new
                {
                    message = "この倉庫には同じロケーションコードが登録されています。"
                });
            }

            //Entity からResponseDTOに変換
            var response = new LocationResponse
            {
                Id = location.Id,
                WarehouseId = location.WarehouseId,
                WarehouseName = warehouse.Name,
                LocationCode = location.LocationCode,
                LocationName = location.LocationName,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt,
                Version = location.Version
            };

            return CreatedAtAction(
                nameof(GetLocation),
                new { id = location.Id },
                response);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(long id, UpdateLocationRequest request)
        {
            var existingLocation = await _context.Locations.FindAsync(id);

            //Locationが存在しない場合は404を返す
            if (existingLocation == null)
            {
                return NotFound();
            }
            // Versionの入力チェック
            if (request.Version < 1)
            {
                return BadRequest(new
                {
                    message = "Versionを正しく指定してください。"
                });
            }

            // Versionの競合チェック
            if (existingLocation.Version != request.Version)
            {
                return VersionConflict();
            }

            // 倉庫IDの入力チェック
            if (request.WarehouseId <= 0)
            {
                return BadRequest(new
                {
                    message = "倉庫IDを正しく指定してください。"
                });
            }

            // 変更先の倉庫の存在確認
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w =>
                w.Id == request.WarehouseId &&
                w.DeletedAt == null);
            
            if (warehouse == null)
            {
                return BadRequest(new
                {
                    message = "指定された倉庫が存在しません。"
                });
            }

            // 同一倉庫内のロケーションコード重複確認
            // 自分自身のレコードは除外する
            var exists = await _context.Locations
                .AnyAsync(l =>
                l.Id != id &&
                 l.WarehouseId == request.WarehouseId &&
                l.LocationCode == request.LocationCode);

            if (exists)
            {
                return Conflict(new
                {
                    message = "この倉庫には同じロケーションコードが登録されています。"
                });
            }



            // DTOからEntityへ値を反映
            existingLocation.WarehouseId = request.WarehouseId;
            existingLocation.LocationCode = request.LocationCode;
            existingLocation.LocationName = request.LocationName;
          
            //更新日時を設定
            existingLocation.UpdatedAt = DateTime.UtcNow;

            // Versionを更新
            existingLocation.Version++;

            try{
                //データベースに保存
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return VersionConflict();
            }
            catch (DbUpdateException ex)
             when (ex.InnerException is PostgresException pgEx &&
              pgEx.ConstraintName == "uq_locations_warehouse_location_code")
            {

                return Conflict(new
                {
                    message = "この倉庫には同じロケーションコードが登録されています。"
                });
            }

            //更新成功(HTTP 204)
            return NoContent();


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(long id)
        {
            // IDを指定してLocationを検索
            var location = await _context.Locations
                .Where(l => l.Id == id && l.DeletedAt == null)
                .FirstOrDefaultAsync();


            // Locationが存在しない場合は404を返す
            if (location == null)
            {
                return NotFound();
            }

            // 在庫数または引当数が残っているか確認
            var hasStock = await _context.Stocks
                .AnyAsync(s => s.LocationId == id &&
                (s.Quantity > 0 || s.ReservedQuantity > 0));

            // 在庫数または引当数が残っている場合は409を返す

            if (hasStock)
            {
                return Conflict(new
                {
                    code = "HAS_STOCK",
                    message = "在庫数または引当数が残っているため、ロケーションを削除できません。"
                });

            }

            // 論理削除日時を設定
            var now = DateTime.UtcNow;

            location.DeletedAt = now;
            location.UpdatedAt = now;
            location.Version++;

            try
            {
                // データベースに保存
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return VersionConflict();
            }

            // 削除成功（HTTP 204）
            return NoContent();

        }


        private IActionResult VersionConflict()
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "データの更新に失敗しました。",
                Detail = "他のユーザーによってデータが更新されています。最新のデータを取得してください。"
            };

            problem.Extensions["code"] = "VERSION_CONFLICT";

            return Conflict(problem);

        }

    }
}
