using ASTRALib;
using Microsoft.Win32;
using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace State_Estimation.Command
{
	internal class LoadOiCommand : ICommand
	{
		private readonly AppViewModel _vm;
		public LoadOiCommand(AppViewModel vm)
		{
			_vm = vm;
		}
		public bool CanExecute(object parameter)
		{
			return _vm.NodeList != null && _vm.BranchList != null;
		}

		public void Execute(object parameter)
		{
			if (parameter is IRastr rastrWin)
			{
				_vm.OiList = RastrWinReader.ReadRastrTM(rastrWin);//TODO: надо придумать как быть с сетевым узлом	
				_vm.Log("Чтение ТМ выполнено!");

			}
			else if (parameter is Window)
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "rg2 files (*.rg2)|*.rg2|csv files (*.csv)|*.csv|All files (*.*)|*.*";
				if (openFileDialog.ShowDialog() == true)
				{
					string path = openFileDialog.FileName;
					_vm.Log("Начато чтение файла " + path);
					switch (System.IO.Path.GetExtension(openFileDialog.FileName))
					{
						case ".rg2":
							IRastr rastr = new Rastr();
							rastr.Load(RG_KOD.RG_REPL, path, "");
							_vm.OiList = RastrWinReader.ReadRastrTM(rastr);//TODO: надо придумать как быть с сетевым узлом	
							_vm.Log("Чтение ТМ выполнено!");

							break;
						case ".csv":
							using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
							{
								string line;
								while ((line = sr.ReadLine()) != null)
								{
									OperationInfo oi = new OperationInfo();
									var massive = line.Split(';');
									int typeCsv = Convert.ToInt32(massive[1]);
									oi.Type = (OperationInfo.KeyType)typeCsv;
									oi.Id = Convert.ToInt32(massive[0]);
									oi.Measurement = Convert.ToDouble(massive[2]);
									oi.NodeNumb = Convert.ToInt32(massive[3]);
									if (massive[4] != string.Empty)
										oi.NodeNumb2 = Convert.ToInt32(massive[4]);
									_vm.OiList.Add(oi);
								}
							}
							break;
					}
				}
			}

		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
