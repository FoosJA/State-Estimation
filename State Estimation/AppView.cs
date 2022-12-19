using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using State_Estimation.Infrastructure;

namespace State_Estimation
{
	internal class AppView : INotifyPropertyChanged
	{
		private readonly string path = @"C:\temp\StateEstimation.log";
		public void Log(string message)
		{
			using (var logFile = File.AppendText(path))
			{
				InfoCollect.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " " + message);
				logFile.WriteLine(DateTime.Now + " " + message);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void RaisePropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private ObservableCollection<string> _infoCollect = new ObservableCollection<string>();
		public ObservableCollection<string> InfoCollect
		{
			get => _infoCollect;
			set { _infoCollect = value; RaisePropertyChanged(); }
		}

		public ICommand ClearInfoCollect => new RelayCommand(ClearLog);
		private void ClearLog() { InfoCollect.Clear(); }
	}
}
