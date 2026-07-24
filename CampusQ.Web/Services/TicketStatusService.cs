using CampusQ.MVP.Data;

namespace CampusQ.Web.Services
{

    public class TicketStatusService
    {
        private readonly QueueRepository _queueRepository;
        private readonly int _averageMinutesPerTicket;

        public TicketStatusService(QueueRepository queueRepository, IConfiguration configuration)
        {
            _queueRepository = queueRepository;
            _averageMinutesPerTicket = configuration.GetValue<int?>("TicketMonitoring:AverageMinutesPerTicket") ?? 5;
        }

        public TicketStatusResult GetStatus(int ticketNumber)
        {
            var entry = _queueRepository.GetByTicketNumber(ticketNumber);
            if (entry != null)
            {
                var peopleAhead = _queueRepository.CountAhead(entry.Service, entry.Purpose, entry.TicketNumber);
                return new TicketStatusResult
                {
                    State = TicketState.Waiting,
                    TicketNumber = entry.TicketNumber,
                    TicketLabel = entry.TicketLabel,
                    Service = entry.Service,
                    Purpose = entry.Purpose,
                    PeopleAhead = peopleAhead,
                    PositionInLine = peopleAhead + 1,
                    EstimatedWaitMinutes = peopleAhead * _averageMinutesPerTicket
                };
            }

            var history = _queueRepository.GetHistoryByTicketNumber(ticketNumber);
            if (history != null)
            {
                return new TicketStatusResult
                {
                    State = TicketState.Served,
                    TicketNumber = history.TicketNumber,
                    TicketLabel = history.TicketLabel,
                    Service = history.Service,
                    Purpose = history.Purpose,
                    PeopleAhead = 0,
                    PositionInLine = 0,
                    EstimatedWaitMinutes = 0,
                    ServedAt = history.ServedAt
                };
            }

            return new TicketStatusResult
            {
                State = TicketState.NotFound,
                TicketNumber = ticketNumber
            };
        }
    }
}
