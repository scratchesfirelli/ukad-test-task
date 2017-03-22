namespace ukad_test_task.Models
{
    public class UrlResponseTime
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
    }

    
}