using ASTRALib;
using Microsoft.Win32;
using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace State_Estimation.Command
{
	internal class LoadCommand : ICommand
	{
		private readonly AppViewModel _vm;
		public LoadCommand(AppViewModel vm)
		{
			_vm = vm;
		}

		public bool CanExecute(object parameter) => true;

		public void Execute(object parameter)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "rg2 files (*.rg2)|*.rg2|csv files (*.csv)|*.csv|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)//TODO: реализовать чтение csv
			{
				string path = openFileDialog.FileName;
				_vm.Log("Начато чтение файла " + path);
				switch (System.IO.Path.GetExtension(openFileDialog.FileName))
				{
					case ".rg2":
						IRastr rastr = new Rastr();
						rastr.Load(RG_KOD.RG_REPL, path, "");

						_vm.NodeList = RastrWinReader.ReadRastrNode(rastr);
						_vm.BranchList = RastrWinReader.ReadRastrBranch(rastr);
						_vm.Log("Чтение расчётной модели выполнено!");

						var result = MessageBox.Show("Считать ТМ из RastrWin3?", " Чтение ТМ", MessageBoxButton.YesNo, MessageBoxImage.Question);
						if (result == MessageBoxResult.Yes)
						{

							_vm.OiList = RastrWinReader.ReadRastrTM(rastr);//TODO: надо придумать как быть с сетевым узлом		
							_vm.Log("Чтение ТМ выполнено!");
						}
						break;
					case ".csv":
						using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
						{
							string line;
							while ((line = sr.ReadLine()) != null)
							{
								OperationInfo oi = new OperationInfo();
								var massiv = line.Split(';');
								int typeCsv = Convert.ToInt32(massiv[1]);
								oi.Type = (OperationInfo.KeyType)typeCsv;
								oi.Id = Convert.ToInt32(massiv[0]);
								oi.Measurement = Convert.ToDouble(massiv[2]);
								oi.NodeNumb = Convert.ToInt32(massiv[3]);
								if (massiv[4] != string.Empty)
									oi.NodeNumb2 = Convert.ToInt32(massiv[4]);
								_vm.OiList.Add(oi);
							}
						}
						break;
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
