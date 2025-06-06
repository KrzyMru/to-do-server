using to_do_server.Models;

namespace to_do_server.DTO
{
    public class TaskDTO(Models.Task task, List list)
    {
        public int Id { get; set; } = task.Id;
        public string Name { get; set; } = task.Name;
        public string Description { get; set; } = task.Description;
        public DateTimeOffset Due { get; set; } = task.Due;
        public DateTimeOffset Created { get; set; } = task.Created;
        public bool Completed { get; set; } = task.Completed;
        public ListHeaderDTO ListHeader { get; set; } = new ListHeaderDTO(list);
    }
}
