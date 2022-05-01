using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace State_Estimation
{
	class AppView : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Передаем свойство и вызываем событие
        /// </summary>
        /// <param name="name"></param>
        public void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Формирование лога
        /// </summary>
        private ObservableCollection<string> _infoCollect = new ObservableCollection<string>();
        public ObservableCollection<string> InfoCollect
        {
            get { return _infoCollect; }
            set { _infoCollect = value; RaisePropertyChanged(); }
        }

        private readonly string path = @"C:\temp\StateEstimation.log";
        public void Log(string message)
        {

            using (StreamWriter logFile = File.AppendText(path))
            {
                InfoCollect.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " " + message);
                logFile.WriteLine(DateTime.Now + " " + message);
            }
        }
        /// <summary>
        /// Для отображения инструментов фильтрации
        /// </summary>
        public class OppositeBooleanToVisibility : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!(bool)value)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                System.Windows.Visibility visibility = (System.Windows.Visibility)value;

                return visibility == System.Windows.Visibility.Visible ? false : true;
            }
        }
    }
}
