using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SaldanhaMoveisDesktop
{
    public class TipoCorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tipo = value as string;

            if (tipo == "Entrada")
                return new SolidColorBrush(Color.FromRgb(40, 167, 69));

            if (tipo == "Saída" || tipo == "Saida")
                return new SolidColorBrush(Color.FromRgb(220, 53, 69));

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}