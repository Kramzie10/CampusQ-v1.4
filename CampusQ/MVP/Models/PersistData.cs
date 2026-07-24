using System.Collections.Generic;

namespace CampusQ.MVP.Models
{
 public class PersistData
 {
 public List<QueueEntry>? MasterQueue { get; set; }
 public int NextTicketNumber { get; set; }
 }
}