namespace ILA_Server.Areas.Identity.Models
{
    public class JsonWebToken
    {
        public string AccessToken { get; set; }
        public long Expires { get; set; }
    }
}
