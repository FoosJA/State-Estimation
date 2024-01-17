using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using State_Estimation.Foundation;

namespace State_Estimation.Command
{
    public class DynamicStateEstimationCommand : ICommand
    {
        private readonly AppViewModel _vm;

        public DynamicStateEstimationCommand(AppViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter) => _vm.CanSE();

        public void Execute(object parameter)
        {
            var stateList = Estimation.CreateStateList(_vm.NodeList, _vm.OiList);
            var baseNode = _vm.NodeList.FirstOrDefault(x => x.Type == TypeNode.Base);
            _vm.NodeList.Move(_vm.NodeList.IndexOf(baseNode),
                _vm.NodeList.Count - 1); //TODO: возможно перемещение базы вниз не нужно, нужно. Надо оптимизировать

            var nodeCount = _vm.NodeList.Count;
            //кол-во компонентов вектора состояния
            var k = 2 * nodeCount - 1;
            var measureCount = _vm.OiList.Count;
            if (measureCount < k)
            {
                throw new ArgumentException("Режим ненаблюдаем!");
            }

            //TODO: может когда-нибудь смогу это реализовать
            //Matrix G = new Matrix(NodeList.Count, NodeList.Count);
            //Matrix B = new Matrix(NodeList.Count, NodeList.Count);

            //используется при старте рассчёта, чтобы обращаться к оценке измерения на каждой итерации
            foreach (var oi in _vm.OiList)
            {
                oi.Estimation = oi.Measurement;
            }

            var stateVector = Estimation.GetStateVector(stateList, _vm.NodeList); //вектор состояния

            var b = Matrix.ZeroMatrix(k, 1); // УВ нет поэтому 0

            var f = Matrix.IdentityMatrix(k, k);

            var c = Matrix.DiagonalMatrix(k, k, 1 /*0.02 * 0.02*/);
            
            //Квадратная матрица, порядок матрицы равен размеру вектора состояния
            var p = Matrix.ZeroMatrix(k, k); 

            var kalman = new KalmanFilter(b, c, f, stateVector, p);

            var measVector = Estimation.GetMeasVector(_vm.OiList);
            var measure = new List<Matrix>
            {
                measVector,
                measVector
            };
            
            foreach (var meas in measure)
            {
                var jacobian =
                    Estimation.GetJacobian(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);
                var weightMatrix = //Matrix.DiagonalMatrix(measureCount, measureCount, 1);
                    Estimation.GetWeightMatrix(jacobian, _vm.OiList,
                        _vm.GetRatioByJacobi); //матрица весовых коэффициентов
                kalman.Correct(meas, jacobian, weightMatrix);
                stateList = Estimation.UpdateState(kalman.State, stateList);
                _vm.StateVectorList.Add(kalman.State);
            }


            Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}