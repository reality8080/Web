namespace Web.Models.ClassModel
{
    public class User_Model
    {
        private string? _name, _birthday, _cccd;
        public string? Name { get => string.IsNullOrEmpty(_name) ? "" : _name; set => _name = value; }
        public string? Birthday { get => string.IsNullOrEmpty(_birthday) ? "DD/MM/YYYY" : _birthday; set => _birthday = value; }
        public string? Cccd { get => string.IsNullOrEmpty(_cccd) ? "0000000000" : _cccd; set => _cccd = value; }
        private string? _user;
        private string? _password;
        private static readonly bool _typeOfAccount = false;

        public string? User { get => string.IsNullOrEmpty(_user) ? "" : _user; set => _user = value; }
        public string? Password { get => string.IsNullOrEmpty(_password) ? "" : _password; set => _password = value; }

        public static bool TypeOfAccount => _typeOfAccount;
    }
}
