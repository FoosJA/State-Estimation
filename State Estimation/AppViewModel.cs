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
		public ObservableCollection<OperInform> OiList
		{
			get { return _oiList; }
			set { _oiList = value; RaisePropertyChanged(); }
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
		public ICommand SSECommand { get { return new RelayCommand(StartStaticSE, CanSE); } }
		bool CanSE() { return (OiList.Count != 0 && NodeList.Count != 0 && BranchList.Count != 0); }
		void StartStaticSE()
		{
			int nomerIteracMax = 100;
			var baseNode = NodeList.FirstOrDefault(x => x.Type == TypeNode.Base);
			NodeList.Move(NodeList.IndexOf(baseNode), NodeList.Count - 1);//TODO: возможно перемещение базы вниз не нужно
			int nodeCount = NodeList.Count;
			//кол-во компонентов вектора состояния
			int K = 2 * nodeCount - 1;
			int measureCount = OiList.Count;
			if (measureCount >= K)
			{
				// Цикл алгоритма Гаусса-Ньютона
				int nomerIterac = 1;
				Matrix G = new Matrix(NodeList.Count, NodeList.Count);
				Matrix B = new Matrix(NodeList.Count, NodeList.Count);
				do
				{
					Matrix J = new Matrix(measureCount, K);
					Matrix F = new Matrix(measureCount, 1);
					Matrix C = new Matrix(measureCount, measureCount);
					Matrix U = new Matrix(K, 1);
					int i = 0;
					foreach (Node node in NodeList)
					{
						var oiV = OiList.FirstOrDefault(x => x.NodeNumb == node.Numb && x.Type == TypeOi.U);
						var oiDelta = OiList.FirstOrDefault(x => x.NodeNumb == node.Numb && x.Type == TypeOi.Delta);
						U[i, 0] = oiV == null ? node.Unom : oiV.Meas;
						i++;
						if (node.Type != TypeNode.Base)
						{
							U[i, 0] = oiDelta == null ? 0 : oiDelta.Meas;
							i++;
						}
					}
					foreach (var meas in OiList)
					{
						switch (meas.Type)
						{
							case TypeOi.Pij:

								double J_Vi = 0;
								double J_Deltai = 0;
								double Fi = 0;
								var branchesNode = BranchList.Where(x => ((x.Ni == meas.NodeNumb) || (x.Nj == meas.NodeNumb)));
								
								break;

						}
						
						
					}

					//for (int i = 0; i < NodeList.Count; i++)
					//{
					//    U[i, 0] = NodeList[i].U;
					//    if (NodeList[i] != baseNode)
					//    {
					//        U[i + 1, 0] = NodeList[i].Delta;
					//    }
					//}
				}
				while (nomerIterac < nomerIteracMax);
			}
			else
				Log("Режим не наблюдаемый!");


		}
		public ICommand DSECommand { get { return new RelayCommand(StartDynamicSE, CanSE); } }
		void StartDynamicSE()
		{

		}

		#endregion
	}
}
