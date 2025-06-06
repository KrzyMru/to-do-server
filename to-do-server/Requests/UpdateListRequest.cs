namespace to_do_server.Requests
{
    public class UpdateListRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconType { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string IconColor { get; set; }
    }
}
