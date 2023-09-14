using Microsoft.Win32;
using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;


namespace State_Estimation.Command
{
	internal class SaveCommand : ICommand
	{
		private readonly AppViewModel _vm;
		public SaveCommand(AppViewModel vm)
		{
			_vm = vm;
		}
		public bool CanExecute(object parameter)
		{
			return _vm.NodeList != null && _vm.BranchList != null;
		}

		public void Execute(object parameter)
		{
			var openFileDialog = new SaveFileDialog()
			{
				Filter = "csv files (*.csv)|*.csv",
				FileName = "Node"
			};
			if (openFileDialog.ShowDialog() != true) return;
			
			var nodeSb = new StringBuilder();
			foreach (var node in _vm.NodeList)
			{
				nodeSb.Append(node + "\r");
			}
			var pathNode = openFileDialog.FileName;
			using (var sw = new StreamWriter(pathNode, false))
			{
				sw.Write(nodeSb.ToString());
				_vm.Log($"Файл {pathNode} сохранен!");
			}

			var branchSb = new StringBuilder();
			foreach (var branch in _vm.BranchList)
			{
				branchSb.Append(branch + "\r");
			}
			var pathBranch = openFileDialog.FileName.Replace("Node","Branch");
			using (var sw = new StreamWriter(pathBranch, false))
			{
				sw.Write(branchSb.ToString());
				_vm.Log($"Файл {pathBranch} сохранен!");
			}
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
