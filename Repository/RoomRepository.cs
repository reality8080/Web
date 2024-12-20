using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Web.Data;
using Web.Models;

namespace Web.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private DbContextRoom _context;
        private IMapper _map;

        //public Task Insert(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc);
        public RoomRepository(DbContextRoom context, IMapper mapper)
        {
            _context = context;
            _map = mapper;
        }
        //public async Task InsertPut(string? id, string? nameHost, string? url, string? pass, string? status, String? description, DateTime? time, String? contactNumeber);
        //{

        //}

        public async Task Edit(int Id, Model_Room room)
        {
            if (Id == room.Id)
            {
                var updateRoom = _map.Map<Room>(room);
                _context.Rooms!.Update(updateRoom);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Model_Room>> Display()
        {
            var rooms = await _context.Rooms!.ToListAsync();
            return _map.Map<List<Model_Room>>(rooms);
        }
        public async Task<Model_Room> Search(Guid url)
        {
            var room = await _context.Rooms!.FirstOrDefaultAsync(r=>r.Url==url);
            return _map.Map<Model_Room>(room);
        }

        public async Task<Model_Room> Search(int id)
        {
            var room = await _context.Rooms!.FindAsync(id);
            return _map.Map<Model_Room>(room);
        }

        public async Task<int> InsertRoom(Model_Room room)
        {
            var newRoom = _map.Map<Room>(room);
            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();
            return newRoom.Id;
        }

        public async Task Delete(Guid url)
        {
            var deleteRoom = _context.Rooms!.SingleOrDefault(b => b.Url == url);
            if (deleteRoom != null)
            {
                _context.Rooms!.Remove(deleteRoom);
                await _context.SaveChangesAsync();
            }
        }
    }
}
