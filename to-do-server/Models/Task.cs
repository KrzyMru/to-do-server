using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace to_do_server.Models
{
    [Table("task")]
    public class Task : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("description")]
        public string Description { get; set; } = null!;
        [Column("due")]
        public DateTimeOffset Due { get; set; }
        [Column("created")]
        public DateTimeOffset Created { get; set; }
        [Column("completed")]
        public bool Completed { get; set; }
        [Column("list_id")]
        public int ListId { get; set; }
    }
}
