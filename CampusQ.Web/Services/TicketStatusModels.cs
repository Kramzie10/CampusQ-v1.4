namespace CampusQ.Web.Services
{
    public enum TicketState
    {
        NotFound,
        Waiting,
        Served
    }

    public class TicketStatusResult
    {
        public TicketState State { get; set; }
        public int TicketNumber { get; set; }
        public string TicketLabel { get; set; } = "";
        public string Service { get; set; } = "";
        public string Purpose { get; set; } = "";
        public int PeopleAhead { get; set; }
        public int PositionInLine { get; set; }
        public int EstimatedWaitMinutes { get; set; }
        public DateTime? ServedAt { get; set; }
    }
}
