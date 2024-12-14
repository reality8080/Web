namespace Web.Models.Class
{
    public class Usercs : Human
    {
        private string? _user;
        private string? _password;
        private static readonly bool _typeOfAccount = false;

        public string? User { get => string.IsNullOrEmpty(_user) ? "" : _user; set => _user = value; }
        public string? Password { get => string.IsNullOrEmpty(_password) ? "" : _password; set => _password = value; }

        public static bool TypeOfAccount => _typeOfAccount;
    }
}
