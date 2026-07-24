using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CampusQ.MVP.Models;

namespace CampusQ.MVP.Views
{
 public interface IStaffView
 {
 void BindQueue(BindingList<QueueEntry> view);
 string SelectedService { get; }
 void ShowMessage(string text, string caption, MessageBoxIcon icon);
 void SetSelectedService(string service);
 }
}