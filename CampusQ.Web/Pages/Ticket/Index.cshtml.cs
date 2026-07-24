using CampusQ.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusQ.Web.Pages.Ticket
{
    public class IndexModel : PageModel
    {
        private readonly TicketStatusService _ticketStatusService;
        private readonly IConfiguration _configuration;

        public IndexModel(TicketStatusService ticketStatusService, IConfiguration configuration)
        {
            _ticketStatusService = ticketStatusService;
            _configuration = configuration;
        }

        public TicketStatusResult Status { get; set; } = new();

        public int RefreshIntervalSeconds { get; set; }

        public void OnGet(int ticketNumber)
        {
            RefreshIntervalSeconds = _configuration.GetValue<int?>("TicketMonitoring:RefreshIntervalSeconds") ?? 10;
            Status = _ticketStatusService.GetStatus(ticketNumber);
        }
    }
}
