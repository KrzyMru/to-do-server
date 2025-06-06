namespace to_do_server.Requests
{
    public class UpdateTaskRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Due { get; set; }
    }
}
