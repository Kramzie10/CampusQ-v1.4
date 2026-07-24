using System.Collections.Generic;
using System.Windows.Forms;
using CampusQ.MVP.Models;

namespace CampusQ.MVP.Views
{
 public interface IAdminView
 {
 void ShowQueue(List<QueuePersistDto> items);
 void ShowAccountsView(List<UserAccount> accounts);
 void ShowMessage(string text, string caption, MessageBoxIcon icon);
 }
}