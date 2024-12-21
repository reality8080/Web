using System.ComponentModel.DataAnnotations;
using System.Data;
using Web.Data;
using Web.Models;

namespace Web.Repository
{
    public interface IRoomRepository
    {
        //public Task Insert(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc);
        public Task<int> InsertRoom(Model_Room room);
        //public Task InsertPut(string? id, string? nameHost, string? url, string? pass, string? status, String? description,DateTime? time,String? contactNumeber);
        public Task Edit(int Id, Model_Room room);
        public Task Delete(int Id);
        public Task<IEnumerable<Model_Room>> Display();
        public Task<Model_Room> Search(int Id);
    }
}
