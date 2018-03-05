//-----------------------------------------------------------------------
// <copyright file="CollectionToVisibilityConverter.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace ConsoleHoster.View.Converters
{
	public class CollectionToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (value == null || !(value is ICollection) || (value as ICollection).Count == 0) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}