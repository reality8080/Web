using Microsoft.Data.SqlClient;

namespace Web.Models.Class
{
    public abstract class Human// Tạo đối tượng
    {
        private string? _name, _birthday, _cccd;
        public string? Name { get => string.IsNullOrEmpty(_name) ? "" : _name; set => _name = value; }
        public string? Birthday { get => string.IsNullOrEmpty(_birthday) ? "DD/MM/YYYY" : _birthday; set => _birthday = value; }
        public string? Cccd { get => string.IsNullOrEmpty(_cccd) ? "0000000000" : _cccd; set => _cccd = value; }
    }
}
