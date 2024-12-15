using System.Data;
using Web.Models.ClassModel;

namespace Web.Repository_Pattern
{
    public interface IData
    {
        public Task createTable();
        //public Task Insert(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc);
        public Task Insert(User_Model user);
        public Task InsertPut(string? cccd, string? name, string? birth, string? usersName, string? pass, Boolean typeOfAcc);

        public Task Delete();
        public Task Delete(string usersName);
        public Task<EnumerableRowCollection<Dictionary<string, object>>> Display(string columnName, string sortOrder);
        public Task<EnumerableRowCollection<Dictionary<string, object>>> Search(string usersName);


    }
}
