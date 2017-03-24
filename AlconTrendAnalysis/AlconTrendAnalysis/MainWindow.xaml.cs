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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;

namespace AlconTrendAnalysis
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public IList<DataPoint> Points { get; private set; }
		//public string Title { get; private set; }
		List<TagData> Data;
		public MainViewModel MainViewModel = new MainViewModel();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Data = CSVReader.LoadFromCSV("data.csv");

			foreach (var item in Data)
			{
				ListViewItem itm = new ListViewItem();
				itm.Content = item.Name;
				itm.Selected +=   (sender2, e2) =>
					{
						var points = new List<DataPoint>();
						chart.Title = item.Name;
						foreach (var val in item.Values)
						{
							points.Add(new DataPoint(val.Time, val.Value));
						}
						chart.Series[0].ItemsSource = points;
						chart.InvalidatePlot(true);						
					};				
				listView.Items.Add(itm);
				
			} 
		}  
	}
}
