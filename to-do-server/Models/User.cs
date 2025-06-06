using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace to_do_server.Models
{
    [Table("user")]
    public class User : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("email")]
        public string Email { get; set; } = null!;
        [Column("password")]
        public string Password { get; set; } = null!;
    }
}
