using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xamarin.Templates.Wizards
{
	public class AzureEnabledToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
			{
				if ((bool)value == true)
				{
					return "Visible";
				}
				else
				{
					return "Hidden";
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value.ToString().ToLowerInvariant())
			{
				case "visible":
					return true;
				case "hidden":
					return false;
				default:
					return Binding.DoNothing;
			}
		}
	}
}
