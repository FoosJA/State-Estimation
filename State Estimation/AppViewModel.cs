using State_Estimation.Foundation;
using State_Estimation.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using State_Estimation.Command;

namespace State_Estimation
{
	internal class AppViewModel : AppView
	{
		public AppViewModel()
		{
			SSECommand = new StaticStateEstimationCommand(this);
			DSECommand = new DynamicStateEstimationCommand(this);
			SettingsCommand = new SettingsCommand(this);
			LoadCommand = new LoadCommand(this);
		}

		public int MaxIteration = 100;
		public double MaxError = 0.1;
		public int A = 1;
		public bool GetRatioByJacobi = true;

		private ObservableCollection<Node> _nodeList;
		public ObservableCollection<Node> NodeList
		{
			get => _nodeList;
			set { _nodeList = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<Branch> _branchList;
		public ObservableCollection<Branch> BranchList
		{
			get => _branchList;
			set { _branchList = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<OperationInfo> _oiList;
		public ObservableCollection<OperationInfo> OiList
		{
			get => _oiList;
			set { _oiList = value; RaisePropertyChanged(); }
		}

		/// <summary>
		/// Хранит матрицы состояния
		/// </summary>
		public ObservableCollection<Matrix> StateVectorList = new ObservableCollection<Matrix>();

		private Node _selectedNode;
		public Node SelectedNode
		{
			get => _selectedNode;
			set { _selectedNode = value; RaisePropertyChanged(); }
		}

		private Branch _selectedBranch;
		public Branch SelectedBranch
		{
			get => _selectedBranch;
			set { _selectedBranch = value; RaisePropertyChanged(); }
		}

		private OperationInfo _selectedOi;
		public OperationInfo SelectedOi
		{
			get => _selectedOi;
			set { _selectedOi = value; RaisePropertyChanged(); }
		}

		public bool CanSE() { return (OiList?.Count != 0 && NodeList?.Count != 0 && BranchList?.Count != 0); }
		public ICommand SettingsCommand { get; }
		public ICommand SSECommand { get; }
		public ICommand DSECommand { get; }
		public ICommand LoadCommand { get; }
	}
}
