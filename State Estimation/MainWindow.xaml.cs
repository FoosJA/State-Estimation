using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ASTRALib;
using State_Estimation.Model;
using System.Collections.ObjectModel;
using TypeOi = State_Estimation.Model.OperInform.KeyType;
using State_Estimation.Foundation;

namespace State_Estimation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void BtnLoad_Click(object sender, RoutedEventArgs e)
		{
			string path;
			var viewModel = (this.DataContext as AppViewModel);
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "rg2 files (*.rg2)|*.rg2|csv files (*.csv)|*.csv|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)//TODO: реализовать чтение csv
			{
				path = openFileDialog.FileName;
				viewModel.Log("Начато чтение файла " + path);
				switch (System.IO.Path.GetExtension(openFileDialog.FileName))
				{
					case ".rg2":						
						ASTRALib.IRastr Rastr = new ASTRALib.Rastr();
						Rastr.Load(ASTRALib.RG_KOD.RG_REPL, path, "");
						viewModel.NodeList.Clear();
						viewModel.BranchList.Clear();
						viewModel.NodeList = RastrWinReader.ReadRastrNode(Rastr);
						viewModel.BranchList = RastrWinReader.ReadRastrBranch(Rastr);
						viewModel.Log("Чтение расчётной модели выполнено!");

						var result = MessageBox.Show("Считать ТМ из RastrWin3?", " Чтение ТМ", MessageBoxButton.YesNo, MessageBoxImage.Question);
						if (result == MessageBoxResult.Yes)
						{
							viewModel.OiList.Clear();
							viewModel.OiList = RastrWinReader.ReadRastrTM(Rastr);//TODO: надо придумать как быть с сетевым узлом		
							viewModel.Log("Чтение ТМ выполнено!");
						}
						break;
					case ".csv":
						viewModel.OiList.Clear();
						using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
						{
							string line;
							while ((line = sr.ReadLine()) != null)
							{
								OperInform oi = new OperInform();
								var massiv = line.Split(';');								
								int typeCsv = Convert.ToInt32(massiv[1]);
								oi.Type = (TypeOi)typeCsv;
								oi.Id = Convert.ToInt32(massiv[0]);
								oi.Meas = Convert.ToDouble(massiv[2]);
								oi.NodeNumb = Convert.ToInt32(massiv[3]);
								if (massiv[4]!=string.Empty)
									oi.NodeNumb2= Convert.ToInt32(massiv[4]);
								viewModel.OiList.Add(oi);
							}
							
						}
						break;
					default:
						break;
				}

			}
		}

		


	}
}
