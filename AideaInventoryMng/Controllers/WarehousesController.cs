using AideaInventoryMng.Data;
using AideaInventoryMng.DTO.Products;
using AideaInventoryMng.DTO.Warehouses;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WarehousesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseResponse>>> GetWarehouses() 
        {
            var warehouses = await _context.Warehouses
                 .Select(w => new WarehouseResponse
                  {
                      Id = w.Id,
                      Name = w.Name,
                      Code = w.Code,
                      Address = w.Address,
                      Description = w.Description,
                      CreatedAt = w.CreatedAt,
                      UpdatedAt = w.UpdatedAt

                  }).ToListAsync();

            return Ok(warehouses);
        }

        // 商品詳細取得
        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseResponse>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouses
                .Where(w => w.Id == id)
                .Select(w => new WarehouseResponse 
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Address = w.Address,
                    Description = w.Description,
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt
                }).FirstOrDefaultAsync();

            //存在しない場合は404を返す
            if (warehouse == null)
            {
                return NotFound();
            }

            // 存在する場合は200を返す
            return Ok(warehouse);

        }

        // 倉庫登録
        [HttpPost]
        public async Task<ActionResult<WarehouseResponse>> CreateWarehouse(CreateWarehouseRequest request)
        {
            var warehouse = new Warehouse
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            //倉庫を登録対象に追加
            _context.Warehouses.Add(warehouse);

            //データベースに保存
            await _context.SaveChangesAsync();

            //Entity からResponseDTOに変換
            var response = new WarehouseResponse
            {
                Name = warehouse.Name,
                Code = warehouse.Code,
                Address = warehouse.Address,
                Description = warehouse.Description,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };

            // 登録した倉庫を返す （HTTP 201）
            return CreatedAtAction(
                nameof(GetWarehouse),
                new { id = warehouse.Id },
                warehouse
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWarehouse(int id, UpdateWarehouseRequest request)
        {
            // IDを指定して既存の倉庫を検索
            var existingWarehouse = await _context.Warehouses.FindAsync(id);

            //倉庫が存在しない場合は404を返す
            if (existingWarehouse == null)
            {
                return NotFound();
            }

            //DTOからEntityへ値を反映
            existingWarehouse.Name = request.Name;
            existingWarehouse.Code = request.Code;
            existingWarehouse.Address = request.Address;
            existingWarehouse.Description = request.Description;

            //更新日時を設定
            existingWarehouse.UpdatedAt = DateTime.UtcNow;

            //データベースに保存
            await _context.SaveChangesAsync();

            //更新成功(HTTP 204)
            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {

            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null)
            {
                return NotFound();
            }
            // 削除対象に設定
            _context.Warehouses.Remove(warehouse);

            // データベースに保存
            await _context.SaveChangesAsync();

            // 削除成功(HTTP 204)
            return NoContent();

        }


    }
}
