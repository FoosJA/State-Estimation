﻿using Microsoft.Win32;
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
						ASTRALib.IRastr Rastr = new ASTRALib.Rastr();
						Rastr.Load(ASTRALib.RG_KOD.RG_REPL, path, "");
						var reader = ReadRastrStruct(Rastr);
						viewModel.NodeList = reader.Item1;
						viewModel.BranchList = reader.Item2;
						if (flagReadTM)//TODO: записывать данные ТМ или нет
						{
							ObservableCollection<OperInform> oiList = new ObservableCollection<OperInform>();
							foreach (var node in viewModel.NodeList)
							{
								if (node.P.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = node.Numb, TimeMeas = DateTime.Now, Name = "-", Id = node.Numb, Type = TypeOi.P, Meas = node.P.Meas });
								if (node.Q.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = node.Numb, TimeMeas = DateTime.Now, Name = "-", Id = node.Numb, Type = TypeOi.Q, Meas = node.Q.Meas });
								if (node.U.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = node.Numb, TimeMeas = DateTime.Now, Name = "-", Id = node.Numb, Type = TypeOi.U, Meas = node.U.Meas });
								if (node.Delta.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = node.Numb, TimeMeas = DateTime.Now, Name = "-", Id = node.Numb, Type = TypeOi.Delta, Meas = node.Delta.Meas });
							}
							foreach (var branch in viewModel.BranchList)
							{
								if (branch.Pi.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Ni, NodeNumb2 = branch.Nj, TimeMeas = DateTime.Now, Name = "-", Id = branch.Ni, Type = TypeOi.Pij, Meas = branch.Pi.Meas });
								if (branch.Pj.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Nj, NodeNumb2 = branch.Ni, TimeMeas = DateTime.Now, Name = "-", Id = branch.Nj, Type = TypeOi.Pij, Meas = branch.Pj.Meas });
								if (branch.Qi.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Ni, NodeNumb2 = branch.Nj, TimeMeas = DateTime.Now, Name = "-", Id = branch.Ni, Type = TypeOi.Qij, Meas = branch.Qi.Meas });
								if (branch.Qj.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Nj, NodeNumb2 = branch.Ni, TimeMeas = DateTime.Now, Name = "-", Id = branch.Nj, Type = TypeOi.Qij, Meas = branch.Qj.Meas });
								if (branch.Ii.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Ni, NodeNumb2 = branch.Nj, TimeMeas = DateTime.Now, Name = "-", Id = branch.Ni, Type = TypeOi.Iij, Meas = branch.Ii.Meas });
								if (branch.Ij.Meas != 0)
									oiList.Add(new OperInform { NodeNumb = branch.Nj, NodeNumb2 = branch.Ni, TimeMeas = DateTime.Now, Name = "-", Id = branch.Nj, Type = TypeOi.Iij, Meas = branch.Ij.Meas });
							}
							viewModel.OiList = oiList;
						}
						break;
					default:
						break;
				}
				viewModel.Log("Чтение выполнено");
			}
		}

		/// <summary>
		/// Чтение файла .rg2
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

		private ObservableCollection<OperInform> ReadRastrTM(IRastr rastr)
		{
			ObservableCollection<OperInform> oiList = new ObservableCollection<OperInform>();
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
				double P = powerActiveGen.get_ZN(NumbBus) - powerActiveLoad.get_ZN(NumbBus);
				if (P != 0)
					oiList.Add(new OperInform
					{
						Id = numberBus.get_ZN(NumbBus),
						Type = TypeOi.P,
						Meas = P,
						Name = $"P узла",
						NodeNumb = numberBus.get_ZN(NumbBus),
						TimeMeas = DateTime.Now
					});

				double Q = powerRectiveGen.get_ZN(NumbBus) - powerRectiveLoad.get_ZN(NumbBus);
				if (Q != 0)
					oiList.Add(new OperInform
					{
						Id = numberBus.get_ZN(NumbBus),
						Type = TypeOi.Q,
						Meas = Q,
						Name = $"Q узла",
						NodeNumb = numberBus.get_ZN(NumbBus),
						TimeMeas = DateTime.Now
					});

				double U = voltageBus.get_ZN(NumbBus);
				if (U != 0)
					oiList.Add(new OperInform
					{
						Id = numberBus.get_ZN(NumbBus),
						Type = TypeOi.U,
						Meas = U,
						Name = $"U узла",
						NodeNumb = numberBus.get_ZN(NumbBus),
						TimeMeas = DateTime.Now
					});

				double Delta = DeltaBus.get_ZN(NumbBus);
				if (Delta != 0)

					oiList.Add(new OperInform
					{
						Id = numberBus.get_ZN(NumbBus),
						Type = TypeOi.Delta,
						Meas = Delta,
						Name = $"Delta узла",
						NodeNumb = numberBus.get_ZN(NumbBus),
						TimeMeas = DateTime.Now
					});
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
				double Pi = piVetv.get_ZN(NumbVetv);
				if (Pi != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = niVetv.get_ZN(NumbVetv),
						NodeNumb2 = njVetv.get_ZN(NumbVetv),
						Meas = Pi,
						Name = "Pнач",
						Type = TypeOi.Pij,
						TimeMeas = DateTime.Now
					});
				double Qi = qiVetv.get_ZN(NumbVetv);
				if (Qi != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = niVetv.get_ZN(NumbVetv),
						NodeNumb2 = njVetv.get_ZN(NumbVetv),
						Meas = Qi,
						Name = "Qнач",
						Type = TypeOi.Qij,
						TimeMeas = DateTime.Now
					});

				double Pj = -pjVetv.get_ZN(NumbVetv);
				if (Pj != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = njVetv.get_ZN(NumbVetv),
						NodeNumb2 = niVetv.get_ZN(NumbVetv),
						Meas = Pj,
						Name = "Pкон",
						Type = TypeOi.Pij,
						TimeMeas = DateTime.Now
					});
				double Qj = -qjVetv.get_ZN(NumbVetv);
				if (Qj != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = njVetv.get_ZN(NumbVetv),
						NodeNumb2 = niVetv.get_ZN(NumbVetv),
						Meas = Qj,
						Name = "Qкон",
						Type = TypeOi.Qij,
						TimeMeas = DateTime.Now
					});
				double Ii = iiVetv.get_ZN(NumbVetv);
				if (Ii != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = njVetv.get_ZN(NumbVetv),
						NodeNumb2 = niVetv.get_ZN(NumbVetv),
						Meas = Pi > 0 ? Ii : Ii * (-1),//TODO: В растре ток всегда по модулю
						Name = "Iнач",
						Type = TypeOi.Iij,
						TimeMeas = DateTime.Now
					});
				double Ij = ijVetv.get_ZN(NumbVetv);
				if (Ij != 0)
					oiList.Add(new OperInform
					{
						Id = niVetv.get_ZN(NumbVetv) + njVetv.get_ZN(NumbVetv),
						NodeNumb = njVetv.get_ZN(NumbVetv),
						NodeNumb2 = niVetv.get_ZN(NumbVetv),
						Meas = Pj > 0 ? Ij : Ij * (-1),//TODO: В растре ток всегда по модулю
						Name = "Iкон",
						Type = TypeOi.Iij,
						TimeMeas = DateTime.Now
					});
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
			return oiList;
		}
	}
}
