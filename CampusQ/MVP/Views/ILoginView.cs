using System.Windows.Forms;

namespace CampusQ.MVP.Views
{
 public interface ILoginView
 {
 string Username { get; }
 string Password { get; }
 void ShowMessage(string text, string caption, MessageBoxIcon icon);
 void HideView();
 void CloseView();
 }
}