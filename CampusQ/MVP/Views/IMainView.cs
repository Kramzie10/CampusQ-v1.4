using System.Windows.Forms;

namespace CampusQ.MVP.Views
{
    public interface IMainView
    {
        void ShowMessage(string text, string caption, MessageBoxIcon icon);
        void AddToQueue(string purpose, string service);
    }
}