using State_Estimation.Foundation;
using State_Estimation.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace State_Estimation
{
	class AppViewModel: AppView
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
		#endregion
	}
}
