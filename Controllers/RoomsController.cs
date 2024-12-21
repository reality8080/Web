using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using Web.Repository;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private IRoomRepository _context;

        //private readonly DbContextRoom _context;

        //public RoomsController(DbContextRoom context)
        //{
        //    _context = context;
        //}

        public RoomsController(IRoomRepository context)
        {
            _context = context;
        }
        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model_Room>>> GetRooms()
        {
            try
            {
                return Ok(await _context!.Display());

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Model_Room>> GetRoom(int id)
        {
            try
            {
                var room = await _context!.Search(id);
                return Ok(new { Results = room });
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }
        }

        // PUT: api/Rooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Model_Room room)
        {
            //if (id != room.Id)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(room).State = EntityState.Modified;

            try
            {
                //await _context.SaveChangesAsync();
                await _context.Edit(id, room);
                return Ok(new { Results = await _context.Display() });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Model_Room>> PostRoom(Model_Room room)
        {
            //_context.Add(room);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetRoom", new { id = room.Id }, room);
            
            return Ok(new { result = await _context.InsertRoom(room) });
        }
        

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            //var room = await _context.FindAsync(id);
            //if (room == null)
            //{
            //    return NotFound();
            //}

            //_context.Rooms.Remove(room);
            //await _context.SaveChangesAsync();

            //return NoContent();
            try
            {
                await _context.Delete(id);
                return Ok(await _context!.Display());
            }
            catch
            {
                return BadRequest();
            }
        }

        //private bool RoomExists(Guid id)
        //{
        //    return _context.(e => e.Id == id);
        //}
    }
}
