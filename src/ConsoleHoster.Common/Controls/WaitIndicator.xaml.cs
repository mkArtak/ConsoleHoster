//-----------------------------------------------------------------------
// <copyright file="WaitIndicator.xaml.cs" author="Artak Mkrtchyan">
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConsoleHoster.Common.Controls
{
	/// <summary>
	/// Interaction logic for WaitIndicator.xaml
	/// </summary>
	public partial class WaitIndicator : UserControl
	{
		private DoubleAnimationUsingKeyFrames waitAnimation;
		private Storyboard animationStoryboard;

		public WaitIndicator()
		{
			InitializeComponent();

			this.InitializeWaitAnimation();
			this.IsVisibleChanged += this.WaitIndicator_IsVisibleChanged;
		}

		private void InitializeWaitAnimation()
		{
			this.waitAnimation = new DoubleAnimationUsingKeyFrames();
			Storyboard.SetTargetProperty(this.waitAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
			Storyboard.SetTargetName(this.waitAnimation, "ucWaitingControl");
			this.waitAnimation.RepeatBehavior = RepeatBehavior.Forever;
			this.waitAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0.0)));
			this.waitAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(405.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
			this.animationStoryboard = new Storyboard();
			this.animationStoryboard.Children.Add(this.waitAnimation);
		}

		private void WaitIndicator_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
			{
				this.animationStoryboard.Begin(this.LayoutRoot, true);
			}
			else
			{
				this.animationStoryboard.Stop(this.LayoutRoot);
			}
		}
	}
}