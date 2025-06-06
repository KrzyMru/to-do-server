namespace to_do_server.Requests
{
    public class GetTaskCountsRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Timezone { get; set; }
    }
}
