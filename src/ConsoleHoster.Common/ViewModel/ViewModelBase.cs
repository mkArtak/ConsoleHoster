//-----------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.Common.ViewModel
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private bool isBusy = false;
		private string busyStatus = null;
		private string errorMessage = null;
		private readonly bool isInDesignMode;
		private Dispatcher dispatcher;

		public ViewModelBase(Dispatcher argDispatcher)
		{
			this.dispatcher = argDispatcher ?? Dispatcher.CurrentDispatcher;

			if (this.isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			{
				this.InitializeDesignMode();
			}
			else
			{
				this.InitializeRuntimeMode();
			}
		}

		public ViewModelBase()
			: this(null)
		{

		}

		protected virtual void InitializeDesignMode()
		{

		}

		protected virtual void InitializeRuntimeMode()
		{

		}

		public void InitializeDispatcher(Dispatcher argDispatcher)
		{
			if (argDispatcher == null)
			{
				throw new ArgumentNullException("argDispatcher");
			}

			this.Dispatcher = argDispatcher;
		}

		protected virtual void NotifyPropertyChanged(string argPropertyName)
		{
			PropertyChangedEventHandler tmpEH = this.PropertyChanged;
			if (tmpEH != null)
			{
				tmpEH(this, new PropertyChangedEventArgs(argPropertyName));
			}
		}

		protected virtual void RunActionWithErrorHandling(Action argAction)
		{
			this.RunActionWithErrorHandling(argAction, null);
		}

		protected virtual void RunActionSwitchingToBusyState(Action argAction, bool argHandleErrors)
		{
			try
			{
				this.IsBusy = true;
				argAction();
			}
			catch (Exception ex)
			{
				if (argHandleErrors)
				{
					this.HandleError(null, ex);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				this.IsBusy = false;
			}
		}

		protected virtual void RunActionWithErrorHandling(Action argAction, string argErrorMessage)
		{
			try
			{
				argAction();
			}
			catch (Exception ex)
			{
				this.HandleError(argErrorMessage, ex);
			}
		}

		protected virtual void HandleError(string argErrorMessage, Exception ex)
		{
			this.ErrorMessage = argErrorMessage ?? ex.Message;
			SimpleFileLogger.Instance.LogError(argErrorMessage, ex);
		}

		protected void RunActionOnUIThread(Action argAction)
		{
			if (argAction == null)
			{
				throw new ArgumentNullException("argAction");
			}

			if (this.Dispatcher.Thread != Thread.CurrentThread)
			{
				this.Dispatcher.Invoke(argAction);
			}
			else
			{
				argAction();
			}
		}

		public bool IsBusy
		{
			get
			{
				return this.isBusy;
			}
			set
			{
				if (value != this.isBusy)
				{
					this.isBusy = value;
					this.NotifyPropertyChanged("IsBusy");
				}
			}
		}

		public string BusyStatus
		{
			get
			{
				return this.busyStatus;
			}
			protected set
			{
				if (value != this.busyStatus)
				{
					this.busyStatus = value;
					this.NotifyPropertyChanged("BusyStatus");
				}
			}
		}

		public string ErrorMessage
		{
			get
			{
				return this.errorMessage;
			}
			set
			{
				if (value != this.errorMessage)
				{
					this.errorMessage = value;
					this.NotifyPropertyChanged("ErrorMessage");
				}
			}
		}

		protected bool IsInDesignMode
		{
			get
			{
				return this.isInDesignMode;
			}
		}

		protected Dispatcher Dispatcher
		{
			get
			{
				return this.dispatcher;
			}
			private set
			{
				this.dispatcher = value;
			}
		}
	}
}