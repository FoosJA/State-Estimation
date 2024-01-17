using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace State_Estimation.Command
{
    public class StaticStateEstimationCommand : ICommand
    {
        private readonly AppViewModel _vm;

        public StaticStateEstimationCommand(AppViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return (_vm.CanSE());
        }

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

                var nomerIterac = 1;
                var stateVector = Estimation.GetStateVector(stateList, _vm.NodeList);
                var measVector = Estimation.GetMeasVector(_vm.OiList);
                var gn = new GaussNewton(stateVector, measVector);
                do
                {
                    var calcVector = Estimation.GetCalcVector(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);

                    if (calcVector.Equals(measVector))
                    {
                        _vm.Log($"Итерация №{nomerIterac} \n Целевая функция F=0 \n Погрешность e =0");
                        Estimation.GetAllOi(stateList, _vm.NodeList, _vm.BranchList);
                        _vm.StateVectorList.Add(stateVector);
                        break;
                    }

                    var jacobian = Estimation.GetJacobian(stateList, _vm.NodeList, _vm.BranchList, _vm.OiList);
                    var weightMatrix = Estimation.GetWeightMatrix(jacobian, _vm.OiList, _vm.GetRatioByJacobi);
                    try
                    {
                        var (target, error) = gn.Calculate(calcVector, weightMatrix, jacobian);

                        var f = target[0, 0];

                        stateList = Estimation.UpdateState(gn.State, stateList);
                        _vm.StateVectorList.Add(gn.State);

                        _vm.Log($"Итерация №{nomerIterac} \n Целевая функция F={f} \n Погрешность e ={error}");
                        nomerIterac++;
                        if (error < _vm.MaxError && f < 3)
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
                } while (nomerIterac < _vm.MaxIteration);
            }
            catch (Exception ex)
            {
                _vm.Log($"Ошибка: {ex.Message}");
            }

            _vm.Log($"Статическое ОС выполнено");
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}