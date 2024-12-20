using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Repository;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomCotrol : ControllerBase
    {
        private IRoomRepository _roomRepo;

        public RoomCotrol(IRoomRepository repo)
        {
            _roomRepo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await _roomRepo.Display());
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpGet("Id/{id}")]
        public async Task<IActionResult> GetUrlId([FromRoute] int id)
        {
            try
            {
                return Ok(await _roomRepo.Search(id));
            }
            catch(Exception ex)
            {
                return BadRequest(error:ex.Message);
            }
        }
        [HttpGet("URL/{url}")]
        public async Task<IActionResult> GetUrl([FromRoute] Guid url)
        {
            try
            {
                return Ok(await _roomRepo.Search(url));
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Add(Model_Room room)
        {
            try
            {
                var id=await _roomRepo.InsertRoom(room);
                return CreatedAtAction(nameof(GetUrlId),new
                {
                    controller = "RoomCotrol", id
                }, id);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
