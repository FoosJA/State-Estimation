using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace State_Estimation
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Window
	{
		public bool FlagVesCoef { get; set; }
		public double MaxError { get; set; }
		public int MaxIterac { get; set; }
		public int A { get; set; }
		public Settings()
		{
			InitializeComponent();
			FlagVesCoef = true;
			//MaxError = 0.1;
			//MaxIteration = 100;
			//A = 0;
		}
		
		public Settings(bool flagKoef, double maxError, int maxIter, int a)
		{
			InitializeComponent();
			maxIterTB.Text= maxIter.ToString();
			maxErrorTB.Text= maxError.ToString();
			if (flagKoef)
				koefCB.SelectedItem = koefCB.Items[0];
			else
				koefCB.SelectedItem = koefCB.Items[1];
			aTB.Text= a.ToString();
		}

		private void maxIterTB_TextChanged(object sender, TextChangedEventArgs e)
		{
			MaxIterac =Convert.ToInt32(maxIterTB.Text);
		}

		private void maxErrorTB_TextChanged(object sender, TextChangedEventArgs e)
		{
			MaxError= Convert.ToDouble(maxErrorTB.Text);
		}
		private void koefCB_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			
		}
		private void aTB_TextChanged(object sender, TextChangedEventArgs e)
		{
			A=Convert.ToInt32(aTB.Text);
		}

		public bool SaveChange=false;
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SaveChange = true;
			this.Close();
		}

		private void koefCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (koefCB.SelectedItem == koefCB.Items[0])
				FlagVesCoef = true;
			else
				FlagVesCoef = false;
		}
	}
}
