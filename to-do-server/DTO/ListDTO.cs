using to_do_server.Models;

namespace to_do_server.DTO
{
    public class ListDTO(List list, List<Models.Task> tasks)
    {
        public int Id { get; set; } = list.Id;
        public string Name { get; set; } = list.Name;
        public string IconType { get; set; } = list.IconType;
        public string BackgroundColor { get; set; } = list.BackgroundColor;
        public string TextColor { get; set; } = list.TextColor;
        public string IconColor { get; set; } = list.IconColor;
        public List<TaskDTO> Tasks { get; set; } = tasks.Select(task => new TaskDTO(task, list)).ToList();
    }
}
