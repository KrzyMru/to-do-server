namespace to_do_server.Requests
{
    public class SaveTaskRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Due { get; set; }
        public string Created { get; set; }
        public int ListId { get; set; }
    }
}
