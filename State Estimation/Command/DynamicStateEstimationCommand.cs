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
            var K = 2 * nodeCount - 1;
            var measureCount = _vm.OiList.Count;
            if (measureCount < K)
            {
                throw new ArgumentException("Режим ненаблюдаем!");
            }

            ///TODO: может когда-нибудь смогу это реализовать
            //Matrix G = new Matrix(NodeList.Count, NodeList.Count);
            //Matrix B = new Matrix(NodeList.Count, NodeList.Count);

            foreach (var oi in
                     _vm.OiList) //используется при старте рассчёта, чтобы обращаться к оценке измерения на каждой итерации
            {
                oi.Estimation = oi.Measurement;
            }

            Matrix U = Estimation.GetStateVector(stateList, _vm.NodeList); //вектор состояния
            Matrix J = Estimation.GetJacobian(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList); //матрица Якоби

            var b = Matrix.ZeroMatrix(K, 1); // УВ нет поэтому 0
            var q = Estimation.GetWeightMatrix(J, _vm.OiList, _vm.GetRatioByJacobi); //матрица весовых коэффициентов
            var f = Matrix.IdentityMatrix(K, K);

            var c = Matrix.DiagonalMatrix(K, K, 0.02 * 0.02);
            var p = Matrix.ZeroMatrix(K, K);//Квадратная матрица, порядок матрицы равен размеру вектора состояния


            var kalman = new KalmanFilter(b, q, f, J, c);
            kalman.SetState(U, p);

            Matrix F = Estimation.GetMeasVector(_vm.OiList); //матрица измерений
            var measure = new List<Matrix>();
            measure.Add(F);
            measure.Add(F);
            measure.Add(F);
            measure.Add(F);
            measure.Add(F);
            measure.Add(F);
            foreach (var meas in measure)
            {
                kalman.Correct(meas);
                _vm.StateVectorList.Add(kalman.State);
            }

            stateList = Estimation.UpdateState(kalman.State, stateList);
            Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}