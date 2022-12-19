using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace State_Estimation.Command
{
	internal class StaticStateEstimationCommand : ICommand
	{
		private readonly AppViewModel _vm;

		public StaticStateEstimationCommand(AppViewModel vm)
		{
			_vm = vm;
		}

		public bool CanExecute(object parameter)
		{ return (_vm.CanSE()); }

		public void Execute(object parameter)
		{
			try
			{
				//Учет сетевых узлов
				/*var netNodes = NodeList.Where(x => x.Type == TypeNode.Net);
				foreach (var netNode in netNodes)
				{
					if (netNode.B == 0)
					{
						OperationInfo qNet = new OperationInfo { Estimation = 0, NodeNumb = netNode.Numb, Type = TypeOi.Q, Measurement = 0 };
						//netNode.Q = qNet;
						OiList.Add(qNet);
					}
					OperationInfo pNet = new OperationInfo { Estimation = 0, NodeNumb = netNode.Numb, Type = TypeOi.P, Measurement = 0 };
					//netNode.P = pNet;				
					OiList.Add(pNet);
				}*/
				var stateList = Estimation.CreateStateList(_vm.NodeList, _vm.OiList);
				var baseNode = _vm.NodeList.FirstOrDefault(x => x.Type == TypeNode.Base);

				_vm.NodeList.Move(_vm.NodeList.IndexOf(baseNode), _vm.NodeList.Count - 1);//TODO: возможно перемещение базы вниз не нужно

				var nodeCount = _vm.NodeList.Count;
				//кол-во компонентов вектора состояния
				var K = 2 * nodeCount - 1;
				var measureCount = _vm.OiList.Count;
				if (measureCount < K)
				{
					throw new ArgumentException("Режим ненаблюдаем!");

				}

				///TODO: может когда-нибудь смогу это реализовать
				//Matrix G = new Matrix(NodeList.Count, NodeList.Count);
				//Matrix B = new Matrix(NodeList.Count, NodeList.Count);

				foreach (var oi in _vm.OiList) //используется при старте рассчёта, чтобы обращаться к оценке измерения на каждой итерации
				{
					oi.Estimation = oi.Measurement;
				}

				var nomerIterac = 1;
				do
				{
					Matrix U = Estimation.GetStateVector(stateList, _vm.NodeList);
					_vm.StateVectorList.Add(U);
					Matrix Fmeas = Estimation.GetMeasVector(_vm.OiList);//матрица измерений
					Matrix Fcalc = Estimation.GetCalcVector(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);//матрица рассчёта параметров режима
					Matrix F = Fcalc - Fmeas;
					Matrix J = Estimation.GetJacobian(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);//матрица Якоби
					Matrix C = Estimation.GetWeightMatrix(J, _vm.OiList, _vm.GetRatioByJacobi);//матрица весовых коэффициентов*/	
					var maxF = Matrix.MaxElement(F);
					if (maxF < 1 && nomerIterac == 1)
					{
						_vm.Log($"Итерация №{nomerIterac} \n Целевая функция F=0 \n Погрешность e =0");
						Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
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
						var error = Matrix.MaxElement(deltaU);
						U = U + Matrix.Multiply(1, deltaU);
						stateList = Estimation.UpdateState(U, stateList);

						/*foreach (Node node in NodeList)
						{
							int j = NodeList.IndexOf(node) * 2;
							node.U.Estimation = U[j, 0];
							if (node.Type != TypeNode.Base)
							{
								node.Delta.Estimation = U[j + 1, 0];
							}

						}*/
						_vm.Log($"Итерация №{nomerIterac} \n Целевая функция F={f[0, 0]} \n Погрешность e ={error}");
						nomerIterac++;
						if ((error < _vm.MaxError) && (f[0, 0] < 3))
						{
							Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
							break;
						}
					}
					catch (MException ex)
					{

						_vm.Log($"Ошибка на итерация №{nomerIterac}: {ex.Message}");
						break;
					}
				}
				while (nomerIterac < _vm.MaxIteration);
			}
			catch (Exception ex) { _vm.Log($"Ошибка: {ex.Message}"); }
			_vm.Log($"Статическое ОС выполнено");
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
