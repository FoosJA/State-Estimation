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
			if (openFileDialog.ShowDialog() == true)//TODO: выбор нескольких файлов
			{
				path = openFileDialog.FileName;
				viewModel.Log("Начато чтение файла " + path);
				bool flagReadTM = true;
				switch (System.IO.Path.GetExtension(openFileDialog.FileName))
				{
					case ".rg2":
						viewModel.NodeList.Clear();
						viewModel.BranchList.Clear();
						viewModel.AllOiList.Clear();
						viewModel.OiList.Clear();
						ASTRALib.IRastr Rastr = new ASTRALib.Rastr();
						Rastr.Load(ASTRALib.RG_KOD.RG_REPL, path, "");
						var reader = ReadRastrStruct(Rastr);
						viewModel.NodeList = reader.Item1;
						viewModel.BranchList = reader.Item2;
						viewModel.AllOiList = InitializationAllOi(viewModel.NodeList, viewModel.BranchList);

						if (flagReadTM)//TODO: записывать данные ТМ или нет
						{
							ReadRastrTM(Rastr, viewModel.AllOiList);
							foreach (var oi in viewModel.AllOiList)
							{
								if (oi.Meas != 0)//TODO: надо придумать как быть с сетевым узлом
									viewModel.OiList.Add(oi);
							}
						}
						break;
					default:
						break;
				}
				viewModel.Log("Чтение выполнено");
			}
		}

		/// <summary>
		/// Чтение структуры .rg2
		/// </summary>
		/// <param name="rastr"></param>
		private (ObservableCollection<Node>, ObservableCollection<Branch>) ReadRastrStruct(IRastr rastr)
		{
			ObservableCollection<Node> NodeList = new ObservableCollection<Node>();
			ObservableCollection<Branch> BranchList = new ObservableCollection<Branch>();

			ASTRALib.ITable NodeRastr = rastr.Tables.Item("node");

			ASTRALib.ICol staBus = NodeRastr.Cols.Item("sta");
			ASTRALib.ICol numberBus = NodeRastr.Cols.Item("ny");
			ASTRALib.ICol typeBus = NodeRastr.Cols.Item("tip");
			ASTRALib.ICol nameBus = NodeRastr.Cols.Item("name");
			ASTRALib.ICol Unom = NodeRastr.Cols.Item("uhom");
			ASTRALib.ICol Bsh = NodeRastr.Cols.Item("bsh");
			ASTRALib.ICol powerActiveLoad = NodeRastr.Cols.Item("pn"); //активная мощность нагрузки.
			ASTRALib.ICol powerRectiveLoad = NodeRastr.Cols.Item("qn"); //реактивная мощность нагрузки.
			ASTRALib.ICol powerActiveGen = NodeRastr.Cols.Item("pg"); //активная мощность нагрузки.
			ASTRALib.ICol powerRectiveGen = NodeRastr.Cols.Item("qg"); //реактивная мощность нагрузки.
			ASTRALib.ICol voltageBus = NodeRastr.Cols.Item("vras"); //напряжение.
			ASTRALib.ICol DeltaBus = NodeRastr.Cols.Item("delta"); //Угол напряжения.			

			for (int NumbBus = 0; NumbBus < NodeRastr.Count; NumbBus++)
			{
				Node node = new Node
				{
					Sta = staBus.get_ZN(NumbBus),
					Numb = numberBus.get_ZN(NumbBus),
					TypeIndex = typeBus.get_ZN(NumbBus),
					Name = nameBus.get_ZN(NumbBus),
					Unom = Unom.get_ZN(NumbBus),
					B = Bsh.get_ZN(NumbBus)
				};
				double p = powerActiveGen.get_ZN(NumbBus) - powerActiveLoad.get_ZN(NumbBus);
				double q = powerRectiveGen.get_ZN(NumbBus) - powerRectiveLoad.get_ZN(NumbBus);
				if (p == 0 && q == 0)
					node.TypeIndex = 5;
				node.Type = (TypeNode)node.TypeIndex;
				NodeList.Add(node);
			}

			ASTRALib.ITable BranchRastr = rastr.Tables.Item("vetv");

			ASTRALib.ICol staVetv = BranchRastr.Cols.Item("sta");
			ASTRALib.ICol tipVetv = BranchRastr.Cols.Item("tip");
			ASTRALib.ICol niVetv = BranchRastr.Cols.Item("ip");
			ASTRALib.ICol njVetv = BranchRastr.Cols.Item("iq");
			ASTRALib.ICol paralVetv = BranchRastr.Cols.Item("np");
			ASTRALib.ICol nameVetv = BranchRastr.Cols.Item("name");
			ASTRALib.ICol rVetv = BranchRastr.Cols.Item("r");
			ASTRALib.ICol xVetv = BranchRastr.Cols.Item("x");
			ASTRALib.ICol bVetv = BranchRastr.Cols.Item("b");
			ASTRALib.ICol gVetv = BranchRastr.Cols.Item("g");
			ASTRALib.ICol ktrVetv = BranchRastr.Cols.Item("ktr");
			ASTRALib.ICol piVetv = BranchRastr.Cols.Item("pl_ip");
			ASTRALib.ICol qiVetv = BranchRastr.Cols.Item("ql_ip");
			ASTRALib.ICol pjVetv = BranchRastr.Cols.Item("pl_iq");
			ASTRALib.ICol qjVetv = BranchRastr.Cols.Item("ql_iq");
			ASTRALib.ICol iiVetv = BranchRastr.Cols.Item("ib");
			ASTRALib.ICol ijVetv = BranchRastr.Cols.Item("ie");
			for (int NumbVetv = 0; NumbVetv < BranchRastr.Count; NumbVetv++)
			{
				Branch branch = new Branch();
				if (staVetv.get_ZN(NumbVetv) == 0)
					branch.Sta = false;
				else
					branch.Sta = true;
				branch.Numb = NumbVetv;
				branch.Type = (TypeBranch)tipVetv.get_ZN(NumbVetv);
				branch.Ni = niVetv.get_ZN(NumbVetv);
				branch.Nj = njVetv.get_ZN(NumbVetv);
				branch.Paral = paralVetv.get_ZN(NumbVetv);
				branch.Name = nameVetv.get_ZN(NumbVetv);
				branch.R = rVetv.get_ZN(NumbVetv);
				branch.X = xVetv.get_ZN(NumbVetv);
				branch.B = bVetv.get_ZN(NumbVetv);
				branch.G = gVetv.get_ZN(NumbVetv);
				branch.Kt = ktrVetv.get_ZN(NumbVetv);
				if (branch.Kt == 0)
					branch.Kt = 1;

				if (branch.Paral != 0)
				{
					var doubleBranch = BranchList.FirstOrDefault(x => x.Ni == branch.Ni && x.Nj == branch.Nj);
					if (doubleBranch != null)
					{
						doubleBranch.R = (doubleBranch.R * branch.R) / (doubleBranch.R + branch.R);
						doubleBranch.X = (doubleBranch.X * branch.X) / (doubleBranch.X + branch.X);
						doubleBranch.B = doubleBranch.B + branch.B;
						doubleBranch.G = doubleBranch.G + branch.G;
					}
					else
					{
						BranchList.Add(branch);
					}
				}
				else
				{
					BranchList.Add(branch);
				}
			}
			return (NodeList, BranchList);
		}

		/// <summary>
		/// Чтение ТМ из файла .rg2
		/// </summary>
		/// <param name="rastr"></param>
		/// <param name="oiList"></param>
		private void ReadRastrTM(IRastr rastr, ObservableCollection<OperInform> oiList)
		{
			ASTRALib.ITable NodeRastr = rastr.Tables.Item("node");
			ASTRALib.ICol numberBus = NodeRastr.Cols.Item("ny");
			ASTRALib.ICol powerActiveLoad = NodeRastr.Cols.Item("pn"); //активная мощность нагрузки.
			ASTRALib.ICol powerRectiveLoad = NodeRastr.Cols.Item("qn"); //реактивная мощность нагрузки.
			ASTRALib.ICol powerActiveGen = NodeRastr.Cols.Item("pg"); //активная мощность ген.
			ASTRALib.ICol powerRectiveGen = NodeRastr.Cols.Item("qg"); //реактивная мощность ген.
			ASTRALib.ICol voltageBus = NodeRastr.Cols.Item("vras"); //напряжение.
			ASTRALib.ICol DeltaBus = NodeRastr.Cols.Item("delta"); //Угол напряжения.			

			for (int NumbBus = 0; NumbBus < NodeRastr.Count; NumbBus++)
			{
				int nodeNumb = numberBus.get_ZN(NumbBus);

				double P = powerActiveGen.get_ZN(NumbBus) - powerActiveLoad.get_ZN(NumbBus);
				if (P != 0)
					oiList.First(x => x.NodeNumb == nodeNumb && x.NodeNumb2 == 0 && x.Type == TypeOi.P).Meas = P;
				double Q = powerRectiveGen.get_ZN(NumbBus) - powerRectiveLoad.get_ZN(NumbBus);
				if (Q != 0)
					oiList.First(x => x.NodeNumb == nodeNumb && x.NodeNumb2 == 0 && x.Type == TypeOi.Q).Meas = Q;
				double U = voltageBus.get_ZN(NumbBus);
				oiList.First(x => x.NodeNumb == nodeNumb && x.NodeNumb2 == 0 && x.Type == TypeOi.U).Meas = U;
				double Delta = DeltaBus.get_ZN(NumbBus);
				oiList.First(x => x.NodeNumb == nodeNumb && x.NodeNumb2 == 0 && x.Type == TypeOi.Delta).Meas = Delta;
			}

			ASTRALib.ITable BranchRastr = rastr.Tables.Item("vetv");
			ASTRALib.ICol niVetv = BranchRastr.Cols.Item("ip");
			ASTRALib.ICol njVetv = BranchRastr.Cols.Item("iq");
			ASTRALib.ICol paralVetv = BranchRastr.Cols.Item("np");
			ASTRALib.ICol piVetv = BranchRastr.Cols.Item("pl_ip");
			ASTRALib.ICol qiVetv = BranchRastr.Cols.Item("ql_ip");
			ASTRALib.ICol pjVetv = BranchRastr.Cols.Item("pl_iq");
			ASTRALib.ICol qjVetv = BranchRastr.Cols.Item("ql_iq");
			ASTRALib.ICol iiVetv = BranchRastr.Cols.Item("ib");
			ASTRALib.ICol ijVetv = BranchRastr.Cols.Item("ie");
			for (int NumbVetv = 0; NumbVetv < BranchRastr.Count; NumbVetv++)
			{
				int nodeNumb1 = niVetv.get_ZN(NumbVetv);
				int nodeNumb2 = njVetv.get_ZN(NumbVetv);

				double Pi = -piVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb1 && x.NodeNumb2 == nodeNumb2 && x.Type == TypeOi.Pij).Meas = Pi;

				double Qi = -qiVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb1 && x.NodeNumb2 == nodeNumb2 && x.Type == TypeOi.Qij).Meas = Qi;

				double Pj = pjVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb2 && x.NodeNumb2 == nodeNumb1 && x.Type == TypeOi.Pij).Meas = Pj;

				double Qj = qjVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb2 && x.NodeNumb2 == nodeNumb1 && x.Type == TypeOi.Qij).Meas = Qj;

				double Ii = iiVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb1 && x.NodeNumb2 == nodeNumb2 && x.Type == TypeOi.Iij).Meas = Ii;

				double Ij = ijVetv.get_ZN(NumbVetv);
				oiList.First(x => x.NodeNumb == nodeNumb2 && x.NodeNumb2 == nodeNumb1 && x.Type == TypeOi.Iij).Meas = Ij;

				/*if (paralVetv.get_ZN(NumbVetv) != 0)
				{
					var doubleBranch = BranchList.FirstOrDefault(x => x.Ni == branch.Ni && x.Nj == branch.Nj);
					if (doubleBranch != null)
					{
						doubleBranch.R = (doubleBranch.R * branch.R) / (doubleBranch.R + branch.R);
						doubleBranch.X = (doubleBranch.X * branch.X) / (doubleBranch.X + branch.X);
						doubleBranch.B = doubleBranch.B + branch.B;
						doubleBranch.G = doubleBranch.G + branch.G;
						doubleBranch.Pi = doubleBranch.Pi + branch.Pi;
						doubleBranch.Pj = doubleBranch.Pj + branch.Pj;
						doubleBranch.Qi = doubleBranch.Qi + branch.Qi;
						doubleBranch.Qj = doubleBranch.Qj + branch.Qj;
						doubleBranch.Ii = doubleBranch.Ii + branch.Ii;
						doubleBranch.Ij = doubleBranch.Ij + branch.Ij;
					}
					else
					{
						BranchList.Add(branch);
					}
				}
				else
				{
					BranchList.Add(branch);
				}*/
			}
		}

		/// <summary>
		/// Инициализациях режима ЭС
		/// </summary>
		/// <param name="nodeList"></param>
		/// <param name="branchList"></param>
		/// <returns></returns>
		private ObservableCollection<OperInform> InitializationAllOi(ObservableCollection<Node> nodeList,
			ObservableCollection<Branch> branchList)
		{
			ObservableCollection<OperInform> allOiList = new ObservableCollection<OperInform>();
			foreach (var node in nodeList)
			{
				node.P = new OperInform
				{
					NodeNumb = node.Numb,
					Type = TypeOi.P,
					Name = node.Name + "_P"
				};
				allOiList.Add(node.P);
				node.Q = new OperInform
				{
					NodeNumb = node.Numb,
					Type = TypeOi.Q,
					Name = node.Name + "_Q"
				};
				allOiList.Add(node.Q);
				node.U = new OperInform
				{
					NodeNumb = node.Numb,
					Type = TypeOi.U,
					Name = node.Name + "_U"
				};
				allOiList.Add(node.U);
				node.Delta = new OperInform
				{
					NodeNumb = node.Numb,
					Type = TypeOi.Delta,
					Name = node.Name + "_Delta"
				};
				allOiList.Add(node.Delta);
			}
			foreach (var branch in branchList)
			{
				branch.Pi = new OperInform
				{
					NodeNumb = branch.Ni,
					NodeNumb2 = branch.Nj,
					Type = TypeOi.Pij,
					Name = branch.Name + "_Pi"
				};
				allOiList.Add(branch.Pi);
				branch.Pj = new OperInform
				{
					NodeNumb = branch.Nj,
					NodeNumb2 = branch.Ni,
					Type = TypeOi.Pij,
					Name = branch.Name + "_Pj"
				};
				allOiList.Add(branch.Pj);
				branch.Qi = new OperInform
				{
					NodeNumb = branch.Ni,
					NodeNumb2 = branch.Nj,
					Type = TypeOi.Qij,
					Name = branch.Name + "_Qi"
				};
				allOiList.Add(branch.Qi);
				branch.Qj = new OperInform
				{
					NodeNumb = branch.Nj,
					NodeNumb2 = branch.Ni,
					Type = TypeOi.Qij,
					Name = branch.Name + "_Qj"
				};
				allOiList.Add(branch.Qj);
				branch.Ii = new OperInform
				{
					NodeNumb = branch.Ni,
					NodeNumb2 = branch.Nj,
					Type = TypeOi.Iij,
					Name = branch.Name + "_Ii"
				};
				allOiList.Add(branch.Ii);
				branch.Ij = new OperInform
				{
					NodeNumb = branch.Nj,
					NodeNumb2 = branch.Ni,
					Type = TypeOi.Iij,
					Name = branch.Name + "_Ij"
				};
				allOiList.Add(branch.Ij);
				branch.Sigmai = new OperInform
				{
					NodeNumb = branch.Ni,
					NodeNumb2 = branch.Nj,
					Type = TypeOi.Sigma,
					Name = branch.Name + "_Sigmai"
				};
				allOiList.Add(branch.Sigmai);
				branch.Sigmaj = new OperInform
				{
					NodeNumb = branch.Nj,
					NodeNumb2 = branch.Ni,
					Type = TypeOi.Sigma,
					Name = branch.Name + "_Sigmaj"
				};
				allOiList.Add(branch.Sigmaj);
			}
			return allOiList;
		}
	}
}
