using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using State_Estimation.Foundation;

namespace State_Estimation.Command
{
	internal class DynamicStateEstimationCommand: ICommand
	{
		private readonly AppViewModel _vm;
		public DynamicStateEstimationCommand(AppViewModel vm)
		{
			_vm = vm;
		}
		public bool CanExecute(object parameter) { return (_vm.CanSE()); }

		public void Execute(object parameter)
		{
			List<State> stateList = Estimation.CreateStateList(_vm.NodeList, _vm.OiList);
			/*foreach (var oi in OiList) //используется при старте рассчёта, 
									   //чтобы обращаться к оценке измерения на каждой итерации
			{
				oi.Estimation = oi.Measurement;
			}*/
			Matrix U = Estimation.GetStateVector(stateList, _vm.NodeList);//вектор состояния
			Matrix Dyn =Estimation.GetTrancMatrix(_vm.NodeList.Count);//матрица перехода	
			Matrix F = Estimation.GetMeasVector(_vm.OiList);//матрица измерений
			int nomerIterac = 1;
			do
			{
				Matrix J = Estimation.GetJacobian(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);//матрица Якоби
				Matrix C = Estimation.GetWeightMatrix(J, _vm.OiList, _vm.GetRatioByJacobi);//матрица весовых коэффициентов
				Matrix Uforecast = Dyn * U;
				Matrix errorForecast = Estimation.GetCalcVector(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);
				//Matrix errorForecast = F - Fforecast;	
				Matrix P = (Matrix.Transpose(J) * C.Invert() * J).Invert();
				double W = 2;//ковариационная матрица шума модели
				Matrix M = Dyn * P * Matrix.Transpose(Dyn);// + W; //предсказание ошибки
				Matrix KalmanKoef = M * Matrix.Transpose(J) * (C + J * M * Matrix.Transpose(J)).Invert();
				var t = KalmanKoef * errorForecast;
				Matrix newU = Uforecast + t;
				var error = Matrix.MaxElement(newU - U);
				P = KalmanKoef * J * M; //или M-KalmanKoef * J * M
				_vm.StateVectorList.Add(newU);
				foreach (Node node in _vm.NodeList)
				{
					int j = _vm.NodeList.IndexOf(node) * 2;
					node.U.Estimation = newU[j, 0];
					if (node.Type != TypeNode.Base)
					{
						node.Delta.Estimation = newU[j + 1, 0];
					}
				}
				Matrix newF = Estimation.GetCalcVector(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);
				var fi = Matrix.Transpose(newF) * C * (newF) + Matrix.Transpose(U - newU) * M * (U - newU);
				stateList = Estimation.UpdateState(newU, stateList);
				_vm.Log($"Итерация №{nomerIterac} \n Целевая функция F={fi[0, 0]} \n Погрешность e ={error}");
				nomerIterac++;
				if ((error < _vm.MaxError) && (fi[0, 0] < 3))
				{
					Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
					break;
				}
			}
			while (nomerIterac < _vm.MaxIteration);
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}
