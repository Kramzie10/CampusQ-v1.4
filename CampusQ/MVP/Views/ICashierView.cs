using System;
using System.ComponentModel;
using System.Windows.Forms;
using CampusQ.MVP.Models;

namespace CampusQ.MVP.Views
{

 public interface ICashierView : IStaffView
 {
 BindingList<QueueEntry>? CurrentQueue { get; }
 event EventHandler? QueueChanged;
 void DisplayServedTicket(QueueEntry entry);
 }
}
