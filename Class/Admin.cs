namespace Web.Models.Class
{
    public class Admin : Human
    {
        private string? _admin;
        private string? _password;
        private static readonly bool _typeOfAccount = true;

        public string ADmin { get => string.IsNullOrEmpty(_admin) ? "" : _admin; set => _admin = value; }
        public string Password { get => string.IsNullOrEmpty(_password) ? "" : _password; set => _password = value; }

        public static bool TypeOfAccount => _typeOfAccount;
        //public String TypeOfAccount { get => String.IsNullOrEmpty(_typeOfAccount) ? "" : _typeOfAccount; set => _typeOfAccount = value; }
    }
}
