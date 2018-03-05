//-----------------------------------------------------------------------
// <copyright file="HeaderToImageConverter.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using ConsoleHoster.Model.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ConsoleHoster.View.Converters
{
	[ValueConversion(typeof(string), typeof(bool))]
	public class HeaderToImageConverter : IValueConverter
	{
		private static IDictionary<string, BitmapImage> imageCache = new Dictionary<string, BitmapImage>();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ExplorerItemType tmpItemType = (ExplorerItemType)value;

			string tmpPath;
			switch (tmpItemType)
			{
				case ExplorerItemType.Drive:
					tmpPath = "pack://application:,,,/ConsoleHoster;component/View/Images/diskdrive.png";
					break;

				case ExplorerItemType.Folder:
					tmpPath = "pack://application:,,,/ConsoleHoster;component/View/Images/folder.png";
					break;

				case ExplorerItemType.File:
					tmpPath = null;
					break;

				default:
					tmpPath = null;
					break;
			}

			BitmapImage tmpResult = null;
			if (tmpPath != null)
			{
				if (imageCache.ContainsKey(tmpPath))
				{
					tmpResult = imageCache[tmpPath];
				}
				else
				{
					tmpResult = new BitmapImage(new Uri(tmpPath));
					imageCache[tmpPath] = tmpResult;
				}
			}
			return tmpResult;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Cannot convert back");
		}
	}
}