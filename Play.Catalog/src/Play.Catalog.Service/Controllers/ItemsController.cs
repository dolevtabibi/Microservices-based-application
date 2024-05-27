using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entites;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    // https://localhost:5001/items
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemsRepository;
        private static int requestCounter = 0;
        public ItemsController(IRepository<Item> itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAysnc()
        {
            requestCounter++;
            Console.WriteLine($"Request {requestCounter}: Starting...");

            if (requestCounter <= 2)
            {
                Console.WriteLine($"Request {requestCounter}: Dealying...");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            if (requestCounter <= 4)
            {
                Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error).");
                return StatusCode(500);
            }
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());
            Console.WriteLine($"Request {requestCounter}: 200 (OK).");
            return Ok(items);
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        //More than one time of result depending of the situation
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return item.AsDto();
        }

        // POST /items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };
            await itemsRepository.CreateAsync(item);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        //return no content(did my job but nothing to return)
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var exisitngItem = await itemsRepository.GetAsync(id);

            if (exisitngItem == null)
            {
                return NotFound();
            }


            exisitngItem.Name = updateItemDto.Name;
            exisitngItem.Description = updateItemDto.Description;
            exisitngItem.Price = updateItemDto.Price;


            await itemsRepository.UpdateAsync(exisitngItem);

            return NoContent();
        }

        //DELETE /items/{id}
        [HttpDelete("{id}")]
        //return no content(did my job but nothing to return)
        public async Task<IActionResult> Delete(Guid id)
        {
            var Item = await itemsRepository.GetAsync(id);

            if (Item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(Item.Id);

            return NoContent();
        }
    }
}