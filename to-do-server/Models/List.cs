using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace to_do_server.Models
{
    [Table("list")]
    public class List : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("icon_type")]
        public string IconType { get; set; } = null!;
        [Column("background_color")]
        public string BackgroundColor { get; set; } = null!;
        [Column("text_color")]
        public string TextColor { get; set; } = null!;
        [Column("icon_color")]
        public string IconColor { get; set; } = null!;
        [Column("user_id")]
        public int UserId { get; set; }
    }
}
