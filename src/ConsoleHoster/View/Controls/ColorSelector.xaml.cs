//-----------------------------------------------------------------------
// <copyright file="ColorSelector.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>29/07/2012</date>
//-----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.View.Controls
{
	/// <summary>
	/// Interaction logic for ColorSelector.xaml
	/// </summary>
	public partial class ColorSelector : UserControl
	{
		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorSelector), new PropertyMetadata(Colors.Black, new PropertyChangedCallback(OnSelectedColor_Changed)));
		public static readonly DependencyProperty ColorCodeProperty = DependencyProperty.Register("ColorCode", typeof(string), typeof(ColorSelector), new PropertyMetadata("Black", new PropertyChangedCallback(OnColorCode_Changed)));

		private static void OnSelectedColor_Changed(DependencyObject argSender, DependencyPropertyChangedEventArgs argEA)
		{
		}

		private static void OnColorCode_Changed(DependencyObject argSender, DependencyPropertyChangedEventArgs argEA)
		{
			ColorSelector tmpSender = argSender as ColorSelector;
			tmpSender.btnColor.Background = new SolidColorBrush(ColorUtilities.GetColorFromString(argEA.NewValue as String, Colors.Black));
			tmpSender.SelectedColor = ColorUtilities.GetColorFromString(argEA.NewValue as string, Colors.Black);

		}

		public ColorSelector()
		{
			InitializeComponent();
		}

		public string ColorCode
		{
			get
			{
				return this.GetValue(ColorCodeProperty) as String;
			}
			set
			{
				this.SetValue(ColorCodeProperty, value);
			}
		}

		public Color SelectedColor
		{
			get
			{
				return (Color)this.GetValue(SelectedColorProperty);
			}
			set
			{
				this.SetValue(SelectedColorProperty, value);
			}
		}
	}
}
