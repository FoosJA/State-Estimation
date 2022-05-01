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
		private int _MaxIterac = 100;
		private double _maxError = 0.1;
		private int _A = 1;
		public bool _FlagKoef = true;
		private double _error = 1000;

		private ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();
		private ObservableCollection<Branch> _branchList = new ObservableCollection<Branch>();
		private ObservableCollection<OperInform> _oiList = new ObservableCollection<OperInform>();

		/// <summary>
		/// Узлы сети
		/// </summary>
		public ObservableCollection<Node> NodeList
		{
			get { return _nodeList; }
			set { _nodeList = value; RaisePropertyChanged(); }
		}
		/// <summary>
		/// Ветви сети
		/// </summary>
		public ObservableCollection<Branch> BranchList
		{
			get { return _branchList; }
			set { _branchList = value; RaisePropertyChanged(); }
		}
		/// <summary>
		/// Измеренные ПЭР
		/// </summary>
		public ObservableCollection<OperInform> OiList
		{
			get { return _oiList; }
			set { _oiList = value; RaisePropertyChanged(); }
		}
		/// <summary>
		/// Хранит матрицы состояния
		/// </summary>
		public ObservableCollection<Matrix> StateVectorList = new ObservableCollection<Matrix>();

		private Node _selectedNode;
		public Node SelectedNode
		{
			get { return _selectedNode; }
			set { _selectedNode = value; RaisePropertyChanged(); }
		}
		private Branch _selectedBranch;
		public Branch SelectedBranch
		{
			get { return _selectedBranch; }
			set { _selectedBranch = value; RaisePropertyChanged(); }
		}
		private OperInform _selectedOi;
		public OperInform SelectedOi
		{
			get { return _selectedOi; }
			set { _selectedOi = value; RaisePropertyChanged(); }
		}
		#endregion

		/*private ObservableCollection<State> _stateList = new ObservableCollection<State>();
		public ObservableCollection<State> StateList
		{
			get { return _stateList; }
			set { _stateList = value; RaisePropertyChanged(); }
		}*/
		#region Command	

		public ICommand SettingsCommand { get { return new RelayCommand(SettingsExecute); } }
		public void SettingsExecute()
		{
			Settings settingstWindow = new Settings(_FlagKoef, _maxError, _MaxIterac, _A) { Owner = App.Current.MainWindow };
			settingstWindow.ShowDialog();
			if (settingstWindow.SaveChange == true)
			{
				_MaxIterac = settingstWindow.MaxIterac;
				_maxError = settingstWindow.MaxError;
				_A = settingstWindow.A;
				_FlagKoef = settingstWindow.FlagVesCoef;
			}
		}

		public ICommand ConnectCommand { get { return new RelayCommand(Load); } }
		private void Load() { }
		public ICommand ClearInfoCollect { get { return new RelayCommand(ClearLog); } }

		void ClearLog() { InfoCollect.Clear(); }

		public ICommand SSECommand { get { return new RelayCommand(StartStaticSE, CanSE); } }
		bool CanSE() { return (OiList.Count != 0 && NodeList.Count != 0 && BranchList.Count != 0); }
		/// <summary>
		/// Статическое ОС
		/// </summary>
		void StartStaticSE()
		{
			try
			{
				//Учет сетевых узлов
				/*var netNodes = NodeList.Where(x => x.Type == TypeNode.Net);
				foreach (var netNode in netNodes)
				{
					if (netNode.B == 0)
					{
						OperInform qNet = new OperInform { Est = 0, NodeNumb = netNode.Numb, Type = TypeOi.Q, Meas = 0 };
						//netNode.Q = qNet;
						OiList.Add(qNet);
					}
					OperInform pNet = new OperInform { Est = 0, NodeNumb = netNode.Numb, Type = TypeOi.P, Meas = 0 };
					//netNode.P = pNet;				
					OiList.Add(pNet);
				}*/
				List<State> stateList = CreateState();
				var baseNode = NodeList.FirstOrDefault(x => x.Type == TypeNode.Base);

				NodeList.Move(NodeList.IndexOf(baseNode), NodeList.Count - 1);//TODO: возможно перемещение базы вниз не нужно

				int nodeCount = NodeList.Count;
				//кол-во компонентов вектора состояния
				int K = 2 * nodeCount - 1;
				int measureCount = OiList.Count;
				if (measureCount < K)
				{
					throw new ArgumentException("Режим ненаблюдаем!");

				}

				///TODO: может когда-нибудь смогу это реализовать
				//Matrix G = new Matrix(NodeList.Count, NodeList.Count);
				//Matrix B = new Matrix(NodeList.Count, NodeList.Count);

				foreach (var oi in OiList) //используется при старте рассчёта, чтобы обращаться к оценке измерения на каждой итерации
				{
					oi.Est = oi.Meas;
				}

				int nomerIterac = 1;
				do
				{
					Matrix U = GetStateVector(stateList);
					StateVectorList.Add(U);
					Matrix Fmeas = GetMeasVector();//матрица измерений
					Matrix Fcalc = GetCalcVector(stateList);//матрица рассчёта параметров режима
					Matrix F = Fcalc - Fmeas;
					Matrix J = GetJacobian(stateList);//матрица Якоби
					Matrix C = GetWeightMatrix(J);//матрица весовых коэффициентов*/	
					var maxF = Matrix.MaxElement(F);
					if (maxF < 1 && nomerIterac == 1)
					{
						Log($"Итерация №{nomerIterac} \n Целевая функция F=0 \n Погрешность e =0");
						GetAllOi(stateList);
						break;
					}
					try
					{
						//Основной рачет

						Matrix Ft = Matrix.Transpose(F);
						var f = Matrix.Multiply(0.5, Ft) * C * F;
						Matrix H = Matrix.Transpose(J) * C * J;
						Matrix grad = Matrix.Transpose(J) * C * F;
						Matrix deltaU = H.Invert() * (-grad);
						_error = Matrix.MaxElement(deltaU);
						U = U + Matrix.Multiply(1, deltaU);
						stateList = CreateState(U, stateList);

						/*foreach (Node node in NodeList)
						{
							int j = NodeList.IndexOf(node) * 2;
							node.U.Est = U[j, 0];
							if (node.Type != TypeNode.Base)
							{
								node.Delta.Est = U[j + 1, 0];
							}

						}*/
						Log($"Итерация №{nomerIterac} \n Целевая функция F={f[0, 0]} \n Погрешность e ={_error}");
						nomerIterac++;
						if ((_error < _maxError) && (f[0, 0] < 3))
						{
							GetAllOi(stateList);
							break;
						}
					}
					catch (MException ex)
					{
						
						Log($"Ошибка на итерация №{nomerIterac}: {ex.Message}");
						break;
					}
				}
				while (nomerIterac < _MaxIterac);
			}
			catch (Exception ex) { Log($"Ошибка: {ex.Message}"); }
		}

		public ICommand DSECommand { get { return new RelayCommand(StartDynamicSE, CanSE); } }
		/// <summary>
		/// Динамическое ОС
		/// </summary>
		void StartDynamicSE()
		{
			List<State> stateList = CreateState();
			/*foreach (var oi in OiList) //используется при старте рассчёта, 
									   //чтобы обращаться к оценке измерения на каждой итерации
			{
				oi.Est = oi.Meas;
			}*/
			Matrix U = GetStateVector(stateList);//вектор состояния
			Matrix Dyn = GetTrancMatrix();//матрица перехода	
			Matrix F = GetMeasVector();//матрица измерений
			int nomerIterac = 1;
			do
			{
				Matrix J = GetJacobian(stateList);//матрица Якоби
				Matrix C = GetWeightMatrix(J);//матрица весовых коэффициентов
				Matrix Uforecast = Dyn * U;
				Matrix errorForecast = GetCalcVector(stateList);
				//Matrix errorForecast = F - Fforecast;	
				Matrix P = (Matrix.Transpose(J) * C.Invert() * J).Invert();
				double W = 2;//ковариационная матрица шума модели
				Matrix M = Dyn * P * Matrix.Transpose(Dyn);// + W; //предсказание ошибки
				Matrix KalmanKoef = M * Matrix.Transpose(J) * (C + J * M * Matrix.Transpose(J)).Invert();
				var t = KalmanKoef * errorForecast;
				Matrix newU = Uforecast + t;
				_error = Matrix.MaxElement(newU - U);
				P = KalmanKoef * J * M; //или M-KalmanKoef * J * M
				StateVectorList.Add(newU);
				foreach (Node node in NodeList)
				{
					int j = NodeList.IndexOf(node) * 2;
					node.U.Est = newU[j, 0];
					if (node.Type != TypeNode.Base)
					{
						node.Delta.Est = newU[j + 1, 0];
					}
				}
				Matrix newF = GetCalcVector(stateList);
				var fi = Matrix.Transpose(newF) * C * (newF) + Matrix.Transpose(U - newU) * M * (U - newU);
				CreateState(newU, stateList);
				Log($"Итерация №{nomerIterac} \n Целевая функция F={fi[0, 0]} \n Погрешность e ={_error}");
				nomerIterac++;
				if ((_error < _maxError) && (fi[0, 0] < 3))
				{
					GetAllOi(stateList);
					break;
				}
			}
			while (nomerIterac < _MaxIterac);
		}
		#endregion

		#region Methods

		/// <summary>
		/// Составить вектор измерений
		/// </summary>
		/// <returns>Матрица F</returns>
		private Matrix GetMeasVector()
		{
			Matrix F = new Matrix(OiList.Count, 1);
			for (int i = 0; i < OiList.Count; i++)
			{
				F[i, 0] = OiList[i].Meas;
			}
			return F;
		}

		//TODO: проверить stateList не должен быть глобальным. 

		#region Вектор и лист состояния
		/// <summary>
		/// Создать лист состояния. Создается в начале расчёта
		/// </summary>
		/// <returns>Лист узлов со значениями U и дельта</returns>
		private List<State> CreateState()
		{
			List<State> stateList = new List<State>();
			foreach (var node in NodeList)
			{
				State st = new State { Node = node };
				var u = OiList.SingleOrDefault(x => x.Type == TypeOi.U && x.NodeNumb == node.Numb);
				if (u == null)
					st.U = node.Unom;
				else
					st.U = u.Meas;

				var delta = OiList.SingleOrDefault(x => x.Type == TypeOi.Delta && x.NodeNumb == node.Numb);
				if (delta == null)
					st.Delta = 0.0001;
				else
					st.Delta = delta.Meas;

				stateList.Add(st);
			}
			return stateList;
		}

		/// <summary>
		/// Обновить лист состояния. Обновляется на каждой итерации
		/// </summary>
		/// <param name="U"></param>
		/// <param name="stateList"></param>
		private List<State> CreateState(Matrix U, List<State> stateList)
		{
			int i = 0;
			for (int j = 0; j < stateList.Count(); j++)
			{
				stateList[j].U = U[i, 0];
				i++;
				if (stateList[j].Node.Type != TypeNode.Base)
				{
					stateList[j].Delta = U[i, 0];
					i++;
				}
			}
			return stateList;
		}

		/// <summary>
		/// Составить вектор состояния. Выполняется на каждой итерации
		/// </summary>
		/// <param name="stateList"></param>
		/// <returns>Матрица U</returns>
		private Matrix GetStateVector(IEnumerable<State> stateList)
		{
			int K = 2 * NodeList.Count - 1;
			Matrix U = new Matrix(K, 1);
			foreach (State st in stateList)
			{
				int i = NodeList.IndexOf(st.Node) * 2;
				U[i, 0] = st.U;
				if (st.Node.Type != TypeNode.Base)
				{
					U[i + 1, 0] = st.Delta;
				}
			}
			return U;
		}
		#endregion


		/// <summary>
		/// Расчёт якоби по State.Est
		/// </summary>
		/// <returns></returns>
		private Matrix GetJacobian(IEnumerable<State> stateList)
		{
			Matrix J = new Matrix(OiList.Count, 2 * NodeList.Count - 1);
			int m = 0;
			foreach (var meas in OiList)
			{
				int k = 0;
				double J_Vi = 0;
				double J_Deltai = 0;
				Node node_i = NodeList.First(x => x.Numb == meas.NodeNumb);
				var branchesNode = BranchList.Where(x => x.Ni == node_i.Numb || x.Nj == node_i.Numb);
				Node node_j = NodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
				TypeOi type = meas.Type;
				if (node_j == null)
				{
					switch (type)
					{
						case TypeOi.U:
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = 1;
							m++;
							break;

						case TypeOi.Delta:
							if (node_i.Type != TypeNode.Base)
							{
								k = 2 * NodeList.IndexOf(node_i);
								J[m, k + 1] = 1;
								m++;
							}
							break;

						case TypeOi.P:
							foreach (var branchNode in branchesNode)
							{
								if (branchNode.Ni != node_i.Numb)
									node_j = NodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = NodeList.First(x => x.Numb == branchNode.Nj);

								var tuple = branchNode.GetBranchParam(node_i, node_j);
								double gij = tuple.gij;
								double bij = tuple.bij;
								double gii = tuple.gii;
								double bii = tuple.bii;

								double Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
								double delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
								double Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
								double delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

								var Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);
								J_Vi += 2 * Vi * (gii) + Vj * Aij;
								J_Deltai += (-Vi) * Vj * Bij * Math.PI / 180;
								k = 2 * NodeList.IndexOf(node_j);
								J[m, k] = Vi * Aij;
								if (node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = Vi * Vj * Bij * Math.PI / 180;
								}
							}
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = J_Deltai;
							}
							m++;
							break;

						case TypeOi.Q:
							foreach (var branchNode in branchesNode)
							{
								if (branchNode.Ni != node_i.Numb)
									node_j = NodeList.First(x => x.Numb == branchNode.Ni);
								else
									node_j = NodeList.First(x => x.Numb == branchNode.Nj);

								var tuple = branchNode.GetBranchParam(node_i, node_j);
								double gij = tuple.gij;
								double bij = tuple.bij;
								double gii = tuple.gii;
								double bii = tuple.bii;

								double Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
								double delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
								double Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
								double delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

								var Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
								var Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
								var Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

								J_Vi += 2 * Vi * (bii) + Vj * Bij;
								J_Deltai += Vi * Vj * Aij * Math.PI / 180;
								k = 2 * NodeList.IndexOf(node_j);
								J[m, k] = Vi * Bij;
								if (node_j.Type != TypeNode.Base)
								{
									J[m, k + 1] = (-Vi) * Vj * Aij * Math.PI / 180;
								}
							}
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = J_Vi;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = J_Deltai;
							}
							m++;
							break;
					};
				}
				else if (node_j != null)
				{
					double Vi = stateList.FirstOrDefault(x => x.Node == node_i).U;
					double delta_i = stateList.FirstOrDefault(x => x.Node == node_i).Delta * Math.PI / 180;
					double Vj = stateList.FirstOrDefault(x => x.Node == node_j).U;
					double delta_j = stateList.FirstOrDefault(x => x.Node == node_j).Delta * Math.PI / 180;

					Branch branch = BranchList.First(x => (x.Ni == node_i.Numb && x.Nj == node_j.Numb) ||
					  (x.Ni == node_j.Numb && x.Nj == node_i.Numb));

					var tuple = branch.GetBranchParam(node_i, node_j);
					double gij = tuple.gij;
					double bij = tuple.bij;
					double gii = tuple.gii;
					double bii = tuple.bii;

					var _Cij = (Vi * (gii) - Vj * (gij * Math.Cos(delta_i - delta_j) - bij * Math.Sin(delta_i - delta_j))) / Math.Sqrt(3);
					var _Dij = (Vi * (bii) - Vj * (gij * Math.Sin(delta_i - delta_j) + bij * Math.Cos(delta_i - delta_j))) / Math.Sqrt(3);
					if (_Cij == 0 && _Dij == 0)
					{
						_Cij = 0.0001;//TODO: чтобы учесть корректно токи
						_Dij = 0.0001;
					}
					var _Aij = bij * Math.Sin(delta_i - delta_j) - gij * Math.Cos(delta_i - delta_j);
					var _Bij = -bij * Math.Cos(delta_i - delta_j) - gij * Math.Sin(delta_i - delta_j);

					switch (type)
					{
						case TypeOi.Pij:
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = 2 * Vi * (gii) + Vj * _Aij;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vi) * Vj * _Bij * Math.PI / 180;
							}
							k = 2 * NodeList.IndexOf(node_j);
							J[m, k] = Vi * _Aij;
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = Vi * Vj * _Bij * Math.PI / 180;
							}
							m++;
							break;
						case TypeOi.Qij:
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = 2 * Vi * (bii) + Vj * _Bij;
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = Vi * Vj * _Aij * Math.PI / 180;
							}
							k = 2 * NodeList.IndexOf(node_j);
							J[m, k] = Vi * _Bij;
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vi) * Vj * _Aij * Math.PI / 180;
							}
							m++;
							break;
						case TypeOi.Iij:
							k = 2 * NodeList.IndexOf(node_i);
							var test1 = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (gij + gii) + _Dij * (bij + bii));
							J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * (gij + gii) + _Dij * (bij + bii));
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = ((Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
									(Math.PI / 180) * (_Dij * _Aij - _Cij * _Bij);
							}
							k = 2 * NodeList.IndexOf(node_j);
							J[m, k] = (Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5)) / Math.Sqrt(3)) * (_Cij * _Aij + _Dij * _Bij);
							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = ((Vj * Math.Pow(_Cij * _Cij + _Dij * _Dij, (-0.5))) / Math.Sqrt(3)) *
									(Math.PI / 180) * (_Cij * _Bij - _Dij * _Aij);
							}
							m++;
							break;
						case TypeOi.Sigma:
							k = 2 * NodeList.IndexOf(node_i);
							J[m, k] = ((bij + bii) * _Cij - (gij + gii) * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));
							if (node_i.Type != TypeNode.Base)
							{
								J[m, k + 1] = (Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));

							}
							k = 2 * NodeList.IndexOf(node_j);
							J[m, k] = (_Bij * _Cij - _Aij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij) * (Math.PI / 180));

							if (node_j.Type != TypeNode.Base)
							{
								J[m, k + 1] = (-Vj * (_Aij * _Cij + _Bij * _Dij) / (Math.Sqrt(3) * (_Cij * _Cij + _Dij * _Dij)));
							}
							m++;
							break;
					}
				}
			}
			return J;
		}

		/// <summary>
		/// Составить вектор рассчитанных параметров режима
		/// </summary>
		/// <param name="stateList"></param>
		/// <returns></returns>
		private Matrix GetCalcVector(IEnumerable<State> stateList)
		{
			GetAllOi(stateList);
			Matrix fErr = new Matrix(OiList.Count, 1);
			int m = 0;
			foreach (var meas in OiList)
			{
				Node node_i = NodeList.First(x => x.Numb == meas.NodeNumb);
				Node node_j = NodeList.FirstOrDefault(x => x.Numb == meas.NodeNumb2);
				TypeOi type = meas.Type;
				if (node_j == null)
				{
					switch (type)
					{
						case TypeOi.U:
							fErr[m, 0] = node_i.U.Est;
							m++;
							break;
						case TypeOi.Delta:
							//if (node_i.Type != TypeNode.Base)
							//{
							fErr[m, 0] = node_i.Delta.Est;
							m++;
							//}
							break;
						case TypeOi.P:
							fErr[m, 0] = node_i.P.Est;
							m++;
							break;
						case TypeOi.Q:
							fErr[m, 0] = node_i.Q.Est;
							m++;
							break;
					};
				}
				else
				{
					Branch branch = BranchList.First(x => (x.Ni == node_i.Numb && x.Nj == node_j.Numb) ||
					  (x.Ni == node_j.Numb && x.Nj == node_i.Numb));
					switch (type)
					{
						case TypeOi.Pij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Pi.Est; }
							else { fErr[m, 0] = branch.Pj.Est; }
							m++;
							break;
						case TypeOi.Qij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Qi.Est; }
							else { fErr[m, 0] = branch.Qj.Est; }
							m++;
							break;
						case TypeOi.Iij:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Ii.Est; }
							else { fErr[m, 0] = branch.Ij.Est; }
							m++;
							break;
						case TypeOi.Sigma:
							if (branch.Ni == node_i.Numb) { fErr[m, 0] = branch.Sigmai.Est; }
							else { fErr[m, 0] = branch.Sigmaj.Est; }
							m++;
							break;
					}
				}
			}
			return fErr;
		}
		private Matrix GetWeightMatrix(Matrix J)
		{
			Matrix C = Matrix.IdentityMatrix(OiList.Count, OiList.Count);
			if (_FlagKoef)
			{
				for (int i = 0; i < OiList.Count; i++)
				{

					double cii = 0;
					for (int j = 0; j < J.cols; j++)
					{
						cii += J[i, j] * J[i, j];
					}
					C[i, i] = 1 / cii;
				}
			}

			else
			{
				for (int i = 0; i < OiList.Count; i++)
				{
					C[i, i] = 5;
				}
			}
			return C;
		}


		/// <summary>
		/// Расчёт всех параметров режима 
		/// </summary>
		/// <param name="U"></param>
		private void GetAllOi(IEnumerable<State> StateList)
		{
			foreach (Node node in NodeList)
			{
				var stateNode = StateList.FirstOrDefault(x => x.Node == node);

				int i = NodeList.IndexOf(node) * 2;
				node.U.Est = stateNode.U;
				if (node.Type != TypeNode.Base)
					node.Delta.Est = stateNode.Delta;
				else
					node.Delta.Est = 0;
			}
			foreach (var branch in BranchList)
			{
				Node node_i = NodeList.First(x => x.Numb == branch.Ni);
				Node node_j = NodeList.First(x => x.Numb == branch.Nj);
				branch.GetBranchOi(node_i, node_j);
			}
			foreach (var node in NodeList)
			{
				double Vi = node.U.Est;
				double delta_i = node.Delta.Est * Math.PI / 180;
				var branchesNode = BranchList.Where(x => x.Ni == node.Numb || x.Nj == node.Numb);
				node.GetNodeOi(branchesNode);
			}

		}

		/// <summary>
		/// Составить матрицу D для ДиОС
		/// </summary>
		/// <returns>Матрица D</returns>
		private Matrix GetTrancMatrix()
		{
			int K = 2 * NodeList.Count - 1;
			Matrix D = Matrix.IdentityMatrix(K, K);
			//TODO: Пока генерируется единичная матрица. Далее вместо единиц необходимо рассчитывать коэф а=(х\х)/z
			return D;
		}
		#endregion
	}
}
