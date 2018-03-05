//-----------------------------------------------------------------------
// <copyright file="ColorUtilities.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ConsoleHoster.Common.Utilities
{
	public static class ColorUtilities
	{
		public static Color GetColorFromString(string argColor, Color argDefault)
		{
			Color tmpMediaColor;
			try
			{
				if (typeof(Colors).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(item => item.PropertyType == typeof(Color) && String.Equals(item.Name, argColor, StringComparison.InvariantCultureIgnoreCase)).Any())
				{
					System.Drawing.Color tmpColor = System.Drawing.Color.FromName(argColor);
					tmpMediaColor = Color.FromArgb(tmpColor.A, tmpColor.R, tmpColor.G, tmpColor.B);
				}
				else
				{
					object tmpParsedColor = ColorConverter.ConvertFromString(argColor.ToLower());
					if (tmpParsedColor != null)
					{
						tmpMediaColor = (Color)tmpParsedColor;
					}
					else
					{
						tmpMediaColor = argDefault;
					}
				}
			}
			catch
			{
				tmpMediaColor = argDefault;
			}

			return tmpMediaColor;
		}

		public static string GetColorNameOrCode(Color color)
		{
			return String.Format("#{0}{1}{2}{3}", GetColorComponentCode(color.A), GetColorComponentCode(color.R), GetColorComponentCode(color.G), GetColorComponentCode(color.B));
		}

		private static string GetColorComponentCode(byte argComponent)
		{
			return PrefixWithChar(String.Format("{0:X}", argComponent), '0', 2);
		}

		private static string PrefixWithChar(string argOriginalText, char argPrefixWith, int argLength)
		{
			StringBuilder sb = new StringBuilder();
			if (!String.IsNullOrEmpty(argOriginalText))
			{
				sb.Append(argOriginalText);
			}

			while (sb.Length < argLength)
			{
				sb.Insert(0, argPrefixWith);
			}

			return sb.ToString();
		}
	}
}