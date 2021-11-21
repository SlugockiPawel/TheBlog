namespace TheBlog.ViewModels
{
    public class EmailSettings
    {
        //We can configure and use an SMTP server (e.g. google)
        public string Email { get; set; }
        // public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }  
    }
}
