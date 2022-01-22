using State_Estimation.Model;
using System.Collections.ObjectModel;
using System.Linq;
using ASTRALib;
using TypeOi = State_Estimation.Model.OperInform.KeyType;

namespace State_Estimation.Foundation
{
	public class RastrWinReader
	{
		/// <summary>
		/// Чтение из .rg2 параметры узлов
		/// </summary>
		/// <param name="rastr"></param>
		public static ObservableCollection<Node>  ReadRastrNode(IRastr rastr)
		{
			ObservableCollection<Node> nodeList = new ObservableCollection<Node>();

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
			
			for (int NumbBus = 0; NumbBus < NodeRastr.Count; NumbBus++)
			{
				Node node = new Node
				{
					Sta = staBus.get_ZN(NumbBus),
					Numb = numberBus.get_ZN(NumbBus),
					Type = (TypeNode)typeBus.get_ZN(NumbBus),
					Name = nameBus.get_ZN(NumbBus),
					Unom = Unom.get_ZN(NumbBus),
					B = Bsh.get_ZN(NumbBus) * 0.000001					
				};
				
				double p = powerActiveGen.get_ZN(NumbBus) - powerActiveLoad.get_ZN(NumbBus);
				double q = powerRectiveGen.get_ZN(NumbBus) - powerRectiveLoad.get_ZN(NumbBus);
				if (p == 0 && q == 0)
					node.Type = (TypeNode)5;
				nodeList.Add(node);
			}			
			return nodeList;
		}

		/// <summary>
		/// Чтение из .rg2 параметры ветвей
		/// </summary>
		/// <param name="rastr"></param>
		/// <returns></returns>
		public static ObservableCollection<Branch> ReadRastrBranch(IRastr rastr)
		{
			ObservableCollection<Branch> branchList = new ObservableCollection<Branch>();
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
					var doubleBranch = branchList.FirstOrDefault(x => x.Ni == branch.Ni && x.Nj == branch.Nj);
					if (doubleBranch != null)
					{
						doubleBranch.R = (doubleBranch.R * branch.R) / (doubleBranch.R + branch.R);
						doubleBranch.X = (doubleBranch.X * branch.X) / (doubleBranch.X + branch.X);
						doubleBranch.B = doubleBranch.B + branch.B;
						doubleBranch.G = doubleBranch.G + branch.G;
					}
					else
					{
						branchList.Add(branch);
					}
				}
				else
				{
					branchList.Add(branch);
				}
			}
			return  branchList;
		}

		/// <summary>
		/// Чтение ТМ из файла .rg2
		/// </summary>
		/// <param name="rastr"></param>
		/// <param name="oiList"></param>
		public static ObservableCollection<OperInform>  ReadRastrTM(IRastr rastr)
		{
			ObservableCollection<OperInform> oiList = new ObservableCollection<OperInform>();
			ASTRALib.ITable NodeRastr = rastr.Tables.Item("node");
			ASTRALib.ICol numberBus = NodeRastr.Cols.Item("ny");
			ASTRALib.ICol Qsh = NodeRastr.Cols.Item("qsh");
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
				//if (P != 0)
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb,
					Type = TypeOi.P,
					Meas = P
				});

				double Q = powerRectiveGen.get_ZN(NumbBus) - powerRectiveLoad.get_ZN(NumbBus) + Qsh.get_ZN(NumbBus);
				//if (Q != 0)
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb,
					Type = TypeOi.Q,
					Meas = Q
				});
				double U = voltageBus.get_ZN(NumbBus);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb,
					Type = TypeOi.U,
					Meas = U
				});
				double Delta = DeltaBus.get_ZN(NumbBus);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb,
					Type = TypeOi.Delta,
					Meas = Delta
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
				int nodeNumb1 = niVetv.get_ZN(NumbVetv);
				int nodeNumb2 = njVetv.get_ZN(NumbVetv);

				double Pi = -piVetv.get_ZN(NumbVetv);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb1,
					NodeNumb2 = nodeNumb2,
					Type = TypeOi.Pij,
					Meas = Pi
				});

				double Qi = -qiVetv.get_ZN(NumbVetv);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb1,
					NodeNumb2 = nodeNumb2,
					Type = TypeOi.Qij,
					Meas = Qi
				});

				double Pj = pjVetv.get_ZN(NumbVetv);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb2,
					NodeNumb2 = nodeNumb1,
					Type = TypeOi.Pij,
					Meas = Pj
				});

				double Qj = qjVetv.get_ZN(NumbVetv);
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb2,
					NodeNumb2 = nodeNumb1,
					Type = TypeOi.Qij,
					Meas = Qj
				});

				double Ii = iiVetv.get_ZN(NumbVetv) / 1000;
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb1,
					NodeNumb2 = nodeNumb2,
					Type = TypeOi.Iij,
					Meas = Ii
				});

				double Ij = ijVetv.get_ZN(NumbVetv) / 1000;
				oiList.Add(new OperInform
				{
					NodeNumb = nodeNumb2,
					NodeNumb2 = nodeNumb1,
					Type = TypeOi.Iij,
					Meas = Ij
				});
				//TODO: необходимо проверить параллельность ветвей
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
