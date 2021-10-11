using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TypeOi = State_Estimation.Model.OperInform.KeyType;

namespace State_Estimation
{
	class AppViewModel : AppView
	{
		#region Prop
		public ObservableCollection<Matrix> StateVectorList = new ObservableCollection<Matrix>();
		private ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();
		public ObservableCollection<Node> NodeList
		{
			get { return _nodeList; }
			set { _nodeList = value; RaisePropertyChanged(); }
		}
		private Node _selectedNode;
		public Node SelectedNode
		{
			get { return _selectedNode; }
			set { _selectedNode = value; RaisePropertyChanged(); }
		}
		private ObservableCollection<Branch> _branchList = new ObservableCollection<Branch>();
		public ObservableCollection<Branch> BranchList
		{
			get { return _branchList; }
			set { _branchList = value; RaisePropertyChanged(); }
		}
		private Branch _selectedBranch;
		public Branch SelectedBranch
		{
			get { return _selectedBranch; }
			set { _selectedBranch = value; RaisePropertyChanged(); }
		}


		private ObservableCollection<OperInform> _oiList = new ObservableCollection<OperInform>();
		/// <summary>
		/// Измеренные ПЭР
		/// </summary>
		public ObservableCollection<OperInform> OiList
		{
			get { return _oiList; }
			set { _oiList = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<OperInform> _allOiList = new ObservableCollection<OperInform>();
		/// <summary>
		/// Все ПЭР
		/// </summary>
		public ObservableCollection<OperInform> AllOiList
		{
			get { return _allOiList; }
			set { _allOiList = value; RaisePropertyChanged(); }
		}

		private OperInform _selectedOi;
		public OperInform SelectedOi
		{
			get { return _selectedOi; }
			set { _selectedOi = value; RaisePropertyChanged(); }
		}
		#endregion

		#region Command
		public ICommand ConnectCommand { get { return new RelayCommand(Load); } }
		private void Load()
		{

		}

		public ICommand ClearInfoCollect { get { return new RelayCommand(ClearLog); } }

		void ClearLog()
		{
			InfoCollect.Clear();
		}
		public ICommand SSECommand { get { return new RelayCommand(StartStaticSE, CanSE); } }
		bool CanSE() { return (OiList.Count != 0 && NodeList.Count != 0 && BranchList.Count != 0); }

		public bool flagVesCoef = true;
		private double _error = 1000;
		private double maxError = 0.1;
		void StartStaticSE()
		{
			var netNodes = NodeList.Where(x => x.Type == TypeNode.Net);
			foreach (var netNode in netNodes)
			{
				if (netNode.B==0)
				{
					OperInform qNet = new OperInform { Est = 0, NodeNumb = netNode.Numb, Type = TypeOi.Q, Meas = 0 };
					netNode.Q = qNet;
					OiList.Add(qNet);
				}
				OperInform pNet = new OperInform { Est = 0, NodeNumb = netNode.Numb, Type = TypeOi.P, Meas = 0 };				
				netNode.P = pNet;				
				OiList.Add(pNet);
				
			}
			int nomerIteracMax = 100;
			var baseNode = NodeList.FirstOrDefault(x => x.Type == TypeNode.Base);
			NodeList.Move(NodeList.IndexOf(baseNode), NodeList.Count - 1);//TODO: возможно перемещение базы вниз не нужно
			int nodeCount = NodeList.Count;
			//кол-во компонентов вектора состояния
			int K = 2 * nodeCount - 1;
			int measureCount = OiList.Count;
			if (measureCount >= K)
			{
				Matrix G = new Matrix(NodeList.Count, NodeList.Count);
				Matrix B = new Matrix(NodeList.Count, NodeList.Count);
				Matrix U = new Matrix(K, 1);
				foreach (var oi in OiList) //используется при старте рассчёта, 
										   //чтобы обращаться к оценке измерения на каждой итерации
				{
					oi.Est = oi.Meas;
				}

				foreach (Node node in NodeList)//Задается вектор состояния
				{
					int i = NodeList.IndexOf(node) * 2;
					var oiV = OiList.FirstOrDefault(x => x.NodeNumb == node.Numb && x.Type == TypeOi.U);
					var oiDelta = OiList.FirstOrDefault(x => x.NodeNumb == node.Numb && x.Type == TypeOi.Delta);
					if (oiV == null)
					{
						node.U = new OperInform
						{
							NodeNumb = node.Numb,
							Est = node.Unom,
							Type = TypeOi.U
						};
					}
					U[i, 0] = node.U.Est;
					if (node.Type != TypeNode.Base)
					{
						if (oiDelta == null)
						{
							node.Delta = new OperInform
							{
								NodeNumb = node.Numb,
								Est = 0.0001,
								Type = TypeOi.Delta
							};
						}
						U[i + 1, 0] = node.Delta.Est;
					}
				}
				StateVectorList.Add(U);
				int nomerIterac = 1;
				do
				{
					Matrix J = new Matrix(measureCount, K);
					Matrix F = new Matrix(measureCount, 1);
					Matrix C = new Matrix(measureCount, measureCount);
					int m = 0;
					foreach (var meas in OiList)
					{
						int k = 0;
						double cii = 0; //TODO: уточнить что за переменная	
						double J_Vi = 0;
						double J_Deltai = 0;
						double Fi = 0;

						var node_i = NodeList.First(x => x.Numb == meas.NodeNumb);
						var Vi = node_i.U.Est;
						var delta_i = node_i.Delta.Est * Math.PI / 180;
						if (meas.Type == TypeOi.U)
						{
							F[m, 0] = 0;
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = 1;
							if (flagVesCoef == true)
								C[m, m] = 1;
							else
							{
								//C[m, m] = _nodeVesList.Find(x => x.NumberNode == node_i).VesU;
							}
							m++;

							continue;
						}
						else if ((meas.Type == TypeOi.Delta) && (NodeList.First(x => x.Numb == meas.NodeNumb).Type != TypeNode.Base))
						{
							F[m, 0] = 0;
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k + 1] = 1;
							if (flagVesCoef == true)
								C[m, m] = 1;
							else
							{
								//C[m, m] = _nodeVesList.Find(x => x.NumberNode == node_i).VesU;
							}
							m++;

							continue;
						}
						else if (meas.Type == TypeOi.P)
						{
							var branchesNode = BranchList.Where(x => x.Ni == node_i.Numb || x.Nj == node_i.Numb);
							foreach (var branchNode in branchesNode)
							{
								Node node_j = new Node();
								if (branchNode.Ni != node_i.Numb)
									node_j = NodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = NodeList.First(x => x.Numb == branchNode.Nj);
								double gij; double bij; double gii; double bii;
								if (branchNode.Kt == 1)
								{
									gij = branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
									bij = branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
									gii = gij + branchNode.G;
									bii = bij + branchNode.B / 2 * 0.000001 + node_i.B;
								}
								else
								{
									gij = (branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
									bij = (branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
									if (node_i.Unom > node_j.Unom)
									{
										gii = gij * branchNode.Kt;
										bii = bij * branchNode.Kt;
									}
									else
									{
										gii = gij / branchNode.Kt;
										bii = bij / branchNode.Kt;
									}
								}

								var Vj = node_j.U.Est;
								var delta_j = node_j.Delta.Est * Math.PI / 180;

								var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

								Fi += Cij * Math.Sqrt(3) * Vi;
								J_Vi += 2 * Vi * (gij + gii) + Vj * Aij;
								J_Deltai += (-Vi) * Vj * Bij * Math.PI / 180;
								k = 2 * NodeList.IndexOf(node_j); ;
								//dF/dVj
								J[m, k] = Vi * Aij;
								if (node_j.Type != TypeNode.Base)
								{
									//dF/dбj
									J[m, k + 1] = Vi * Vj * Bij * Math.PI / 180;
								}
							}

							F[m, 0] = Fi - meas.Meas;
							k = 2 * NodeList.IndexOf(node_i);
							//dF/dVi
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								//dF/dбi
								J[m, k + 1] = J_Deltai;
							}
							if (flagVesCoef == true)
							{
								if (node_i.Type != TypeNode.Net)
								{
									double yacobi = 0;
									for (int j = 0; j < K; j++)
									{
										yacobi += J[m, j] * J[m, j];
									}
									C[m, m] = 1 / yacobi;
								}
								else
									C[m, m] = 10;
							}
							else
							{
								//C[m, m] = _nodeVesList.Find(x => x.NumberNode == node_i).VesP;
							}
							m++;
							continue;
						}
						else if (meas.Type == TypeOi.Q)
						{
							var branchesNode = BranchList.Where(x => x.Ni == node_i.Numb || x.Nj == node_i.Numb);
							foreach (var branchNode in branchesNode)
							{
								Node node_j = new Node();
								if (branchNode.Ni != node_i.Numb)
									node_j = NodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = NodeList.First(x => x.Numb == branchNode.Nj);
								double gij; double bij; double gii; double bii;
								if (branchNode.Kt == 1)
								{
									gij = branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
									bij = branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
									gii = gij + branchNode.G;
									bii = bij + branchNode.B / 2 * 0.000001 + node_i.B;
								}
								else
								{
									gij = (branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
									bij = (branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
									if (node_i.Unom > node_j.Unom)
									{
										gii = gij * branchNode.Kt;
										bii = bij * branchNode.Kt;
									}
									else
									{
										gii = gij / branchNode.Kt;
										bii = bij / branchNode.Kt;
									}
								}
								var Vj = node_j.U.Est;
								var delta_j = node_j.Delta.Est * Math.PI / 180;

								var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

								Fi += Dij * Math.Sqrt(3) * Vi;//TODO 

								J_Vi += 2 * Vi * (bij + bii) + Vj * Bij;
								J_Deltai += Vi * Vj * Aij * Math.PI / 180;
								k = 2 * NodeList.IndexOf(node_j);
								//dF/dVj
								J[m, k] = Vi * Bij;
								if (node_j.Type != TypeNode.Base)
								{
									//dF/dбj
									J[m, k + 1] = (-Vi) * Vj * Aij * Math.PI / 180;
								}
							}
							F[m, 0] = Fi - meas.Meas;
							k = 2 * NodeList.IndexOf(node_i);
							//dF/dVi
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								//dF/dбi
								J[m, k + 1] = J_Deltai;
							}
							if (flagVesCoef == true)
							{
								if (node_i.Type != TypeNode.Net)
								{
									double yacobi = 0;
									for (int j = 0; j < K; j++)
									{
										yacobi += J[m, j] * J[m, j];
									}
									C[m, m] = 1 / yacobi;
								}
								else
								{
									C[m, m] = 10;
								}

							}
							else
							{
								//C[m, m] = _nodeVesList.Find(x => x.NumberNode == node_i).VesQ;
							}
							m++;
							continue;
						}

						Node _node_j = NodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
						if (_node_j != null)
						{
							var _Vj = _node_j.U.Est;
							var _delta_j = _node_j.Delta.Est * Math.PI / 180;
							double _gij; double _bij; double _gii; double _bii;
							//Branch branch = BranchList.First(x => x.Pi.Id == meas.Id || x.Pj.Id == meas.Id);
							Branch branch = BranchList.First(x => (x.Ni == node_i.Numb && x.Nj == _node_j.Numb) ||
							  (x.Ni == _node_j.Numb && x.Nj == node_i.Numb));
							if (branch.Kt == 1)
							{
								_gij = branch.R / (branch.R * branch.R + branch.X * branch.X);
								_bij = branch.X / (branch.R * branch.R + branch.X * branch.X);
								_gii = _gij + branch.G;
								_bii = _bij + branch.B / 2 * 0.000001;// + node_i.B;
							}
							else
							{
								_gij = (branch.R / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
								_bij = (branch.X / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
								if (node_i.Unom > _node_j.Unom)
								{
									_gii = _gij * branch.Kt;
									_bii = _bij * branch.Kt;
								}
								else
								{
									_gii = _gij / branch.Kt;
									_bii = _bij / branch.Kt;
								}
							}

							var _Cij = (Vi * (_gii) - _Vj * (_gij * Math.Cos(delta_i - _delta_j) - _bij * Math.Sin(delta_i - _delta_j))) / Math.Sqrt(3);
							var _Dij = (Vi * (_bii) - _Vj * (_gij * Math.Sin(delta_i - _delta_j) + _bij * Math.Cos(delta_i - _delta_j))) / Math.Sqrt(3);
							if (_Cij == 0 && _Dij == 0 && nomerIterac == 1)
							{
								_Cij = 0.0001;//TODO: чтобы учесть корректно токи
								_Dij = 0.0001;
							}
							var _Aij = _bij * Math.Sin(delta_i - _delta_j) - _gij * Math.Cos(delta_i - _delta_j);
							var _Bij = -_bij * Math.Cos(delta_i - _delta_j) - _gij * Math.Sin(delta_i - _delta_j);

							if (meas.Type == TypeOi.Pij)
							{
								var t1 = _Cij * Vi * Math.Sqrt(3);
								F[m, 0] = _Cij * Vi * Math.Sqrt(3) - meas.Meas;
								k = 2 * NodeList.IndexOf(node_i);
								J[m, k] = 2 * Vi * (_gii + _gij) + _Vj * _Aij;
								cii += J[m, k] * J[m, k];
								if (node_i.Type != TypeNode.Base)
								{
									J[m, k + 1] = (-Vi) * _Vj * _Bij * Math.PI / 180;
									cii += J[m, k + 1] * J[m, k + 1];
								}
								k = 2 * NodeList.IndexOf(_node_j);
								J[m, k] = Vi * _Aij;
								cii += J[m, k] * J[m, k];
								if (_node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = Vi * _Vj * _Bij * Math.PI / 180;
									cii += J[m, k + 1] * J[m, k + 1];
								}
								if (flagVesCoef == true)
									C[m, m] = 1 / cii;
								else
								{
									//C[m, m] = _branchVesList.Find(x => x.NumberBranch == numberBranch).VesP;
								}
								m++;
							}
							else if (meas.Type == TypeOi.Qij)
							{
								var q = _Dij * Math.Sqrt(3) * Vi;
								F[m, 0] = _Dij * Math.Sqrt(3) * Vi - meas.Meas;
								k = 2 * NodeList.IndexOf(node_i);
								J[m, k] = 2 * Vi * (_bii + _bij) + _Vj * _Bij;
								cii += J[m, k] * J[m, k];
								if (node_i.Type != TypeNode.Base)
								{
									J[m, k + 1] = Vi * _Vj * _Aij * Math.PI / 180;
									cii += J[m, k + 1] * J[m, k + 1];
								}
								k = 2 * NodeList.IndexOf(_node_j);
								J[m, k] = Vi * _Bij;
								cii += J[m, k] * J[m, k];
								if (_node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = (-Vi) * _Vj * _Aij * Math.PI / 180;
									cii += J[m, k + 1] * J[m, k + 1];
								}
								if (flagVesCoef == true)
									C[m, m] = 1 / cii;
								else
								{
									//C[m, m] = _branchVesList.Find(x => x.NumberBranch == numberBranch).VesP;
								}
								m++;
							}
							else if (meas.Type == TypeOi.Iij)
							{
								F[m, 0] = Math.Sqrt(_Cij * _Cij + _Dij * _Dij) - meas.Meas;
								k = 2 * NodeList.IndexOf(node_i);
								var test1 = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (_gij + _gii) + _Dij * (_bij + _bii));
								J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (_gij + _gii) + _Dij * (_bij + _bii));
								cii += J[m, k] * J[m, k];
								if (node_i.Type != TypeNode.Base)
								{
									var test11 = ((_Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
										(Math.PI / 180) * (_Dij * _Aij - _Cij * _Bij);
									J[m, k + 1] = ((_Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
										(Math.PI / 180) * (_Dij * _Aij - _Cij * _Bij);
									cii += J[m, k + 1] * J[m, k + 1];
								}
								k = 2 * NodeList.IndexOf(_node_j);
								var test2 = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * _Aij + _Dij * _Bij);
								J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * _Aij + _Dij * _Bij);
								cii += J[m, k] * J[m, k];
								if (_node_j.Type != TypeNode.Base)
								{
									var test22 = ((_Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
										(Math.PI / 180) * (_Cij * _Bij - _Dij * _Aij);
									J[m, k + 1] = ((_Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
										(Math.PI / 180) * (_Cij * _Bij - _Dij * _Aij);
									cii += J[m, k + 1] * J[m, k + 1];
								}
								if (flagVesCoef == true)
									C[m, m] = 1 / cii;
								else
								{
									//C[m, m] = _branchVesList.Find(x => x.NumberBranch == numberBranch).VesI;
								}
								m++;
							}
							else if (meas.Type == TypeOi.Sigma)
							{
								F[m, 0] = Math.Atan(_Dij / _Cij) / (Math.PI / 180) - meas.Meas;
								k = 2 * NodeList.IndexOf(node_i);
								J[m, k] = ((_bij + _bii) * _Cij - (_gij + _gii) * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));
								cii += J[m, k] * J[m, k];
								if (node_i.Type != TypeNode.Base)
								{
									J[m, k + 1] = (_Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));
									cii += J[m, k + 1] * J[m, k + 1];
								}
								k = 2 * NodeList.IndexOf(_node_j);
								J[m, k] = (_Bij * _Cij - _Aij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));
								cii += J[m, k] * J[m, k];
								if (_node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = (-_Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));
									cii += J[m, k + 1] * J[m, k + 1];
								}
								if (flagVesCoef == true)
									C[m, m] = 1 / cii;
								else
								{
									//C[m, m] = _branchVesList.Find(x => x.NumberBranch == numberBranch).VesSigma;
								}
								m++;
							}

						}

					}
					try
					{
						//Основной рачет
						var maxF = Matrix.MaxElement(F);
						Matrix Ft = Matrix.Transpose(F);
						var f = Matrix.Multiply(0.5, Ft) * C * F;
						Matrix H = Matrix.Transpose(J) * C * J;
						Matrix grad = Matrix.Transpose(J) * C * F;
						Matrix deltaU = H.Invert() * (-grad);
						_error = Matrix.MaxElement(deltaU);
						U = U + Matrix.Multiply(1, deltaU);
						StateVectorList.Add(U);

						foreach (Node node in NodeList)
						{
							int j = NodeList.IndexOf(node) * 2;
							node.U.Est = U[j, 0];
							if (node.Type != TypeNode.Base)
							{
								node.Delta.Est = U[j + 1, 0];
							}

						}
						Log($"Итерация №{nomerIterac} \n Целевая функция F={f[0, 0]} \n Погрешность e ={_error}");
						nomerIterac++;
						if ((_error < maxError) && (f[0, 0] < 3))
						{
							GetAllOi(U);
							break;
						}
					}
					catch (MException ex)
					{
						Log($"Ошибка на итерация №{nomerIterac}");
						break;
					}
				}
				while (nomerIterac < nomerIteracMax);
			}
			else
				Log("Режим не наблюдаемый!");


		}
		private void GetAllOi(Matrix U)
		{
			foreach (Node node in NodeList)
			{
				int i = NodeList.IndexOf(node) * 2;
				node.U.Est = U[i, 0];
				if (node.Type != TypeNode.Base)
					node.Delta.Est = U[i + 1, 0];
				else
					node.Delta.Est = 0;
			}
			foreach (var node in NodeList)
			{
				double Vi = node.U.Est;
				double delta_i = node.Delta.Est * Math.PI / 180;
				double Pest = 0;
				double Qest = 0;
				var branchesNode = BranchList.Where(x => x.Ni == node.Numb || x.Nj == node.Numb);
				foreach (var branchNode in branchesNode)
				{
					Node node_j = new Node();
					if (branchNode.Ni != node.Numb)
						node_j = NodeList.First(x => x.Numb == branchNode.Ni);
					else
						node_j = NodeList.First(x => x.Numb == branchNode.Nj);
					double gij; double bij; double gii; double bii;
					if (branchNode.Kt == 1)
					{
						gij = branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
						bij = branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X);
						gii = gij + branchNode.G;
						bii = bij + branchNode.B / 2 * 0.000001 + node.B;
					}
					else
					{
						gij = (branchNode.R / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
						bij = (branchNode.X / (branchNode.R * branchNode.R + branchNode.X * branchNode.X)) / branchNode.Kt;
						if (node.Unom > node_j.Unom)
						{
							gii = gij * branchNode.Kt;
							bii = bij * branchNode.Kt;
						}
						else
						{
							gii = gij / branchNode.Kt;
							bii = bij / branchNode.Kt;
						}
					}
					var Vj = node_j.U.Est;
					var delta_j = node_j.Delta.Est * Math.PI / 180;
					var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
					Pest += Cij * Math.Sqrt(3) * Vi;
					var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
					Qest += Dij * Math.Sqrt(3) * Vi;
				}
				node.P.Est = Pest;
				node.Q.Est = Qest;

			}
			foreach (var branch in BranchList)
			{
				Node node_i = NodeList.First(x => x.Numb == branch.Ni);
				Node node_j = NodeList.First(x => x.Numb == branch.Nj);
				double gij; double bij; double gii; double bii;
				if (branch.Kt == 1)
				{
					gij = branch.R / (branch.R * branch.R + branch.X * branch.X);
					bij = branch.X / (branch.R * branch.R + branch.X * branch.X);
					gii = gij + branch.G;
					bii = bij + branch.B / 2 * 0.000001;
				}
				else
				{
					gij = (branch.R / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
					bij = (branch.X / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
					if (node_i.Unom > node_j.Unom)
					{
						gii = gij * branch.Kt;
						bii = bij * branch.Kt;
					}
					else
					{
						gii = gij / branch.Kt;
						bii = bij / branch.Kt;
					}
				}


				var Vj = node_j.U.Est;
				var delta_j = node_j.Delta.Est * Math.PI / 180;
				var Vi = node_i.U.Est;
				var delta_i = node_i.Delta.Est * Math.PI / 180;
				var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
				var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
				var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
				var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

				branch.Pi.Est = Cij * Vi * Math.Sqrt(3);
				branch.Qi.Est = Dij * Math.Sqrt(3) * Vi;
				branch.Ii.Est = Math.Sqrt(Cij * Cij + Dij * Dij);
				branch.Sigmai.Est = Math.Atan(Dij / Cij) / (Math.PI / 180);
				Vj = node_i.U.Est;
				delta_j = node_i.Delta.Est * Math.PI / 180;
				Vi = node_j.U.Est;
				delta_i = node_j.Delta.Est * Math.PI / 180;

				if (branch.Kt == 1)
				{
					gij = branch.R / (branch.R * branch.R + branch.X * branch.X);
					bij = branch.X / (branch.R * branch.R + branch.X * branch.X);
					gii = gij + branch.G;
					bii = bij + branch.B / 2 * 0.000001;
				}
				else
				{
					gij = (branch.R / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
					bij = (branch.X / (branch.R * branch.R + branch.X * branch.X)) / branch.Kt;
					if (node_j.Unom > node_i.Unom)
					{
						gii = gij * branch.Kt;
						bii = bij * branch.Kt;
					}
					else
					{
						gii = gij / branch.Kt;
						bii = bij / branch.Kt;
					}
				}
				Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
				Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
				Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
				Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

				branch.Pj.Est = Cij * Vi * Math.Sqrt(3);
				branch.Qj.Est = Dij * Math.Sqrt(3) * Vi;
				branch.Ij.Est = Math.Sqrt(Cij * Cij + Dij * Dij);
				branch.Sigmaj.Est = Math.Atan(Dij / Cij) / (Math.PI / 180);
			}
		}
		public ICommand DSECommand { get { return new RelayCommand(StartDynamicSE, CanSE); } }
		void StartDynamicSE()
		{

		}

		#endregion
	}
}
