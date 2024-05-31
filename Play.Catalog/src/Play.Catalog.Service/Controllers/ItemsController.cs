using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
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
        private readonly IPublishEndpoint publishEndpoint;
        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAysnc()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());
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
            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

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
            await publishEndpoint.Publish(new CatalogItemUpdated(exisitngItem.Id, exisitngItem.Name, exisitngItem.Description));

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
            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}