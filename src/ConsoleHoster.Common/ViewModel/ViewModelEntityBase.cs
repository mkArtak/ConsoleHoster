//-----------------------------------------------------------------------
// <copyright file="ViewModelEntityBase.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>
// <date>22/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using ConsoleHoster.Common.Model;

namespace ConsoleHoster.Common.ViewModel
{
	public abstract class ViewModelEntityBase<T> : INotifyPropertyChanged where T : IModelEntity
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private readonly T model;

		public ViewModelEntityBase(T argModel)
		{
			if (argModel == null)
			{
				throw new ArgumentNullException("argModel");
			}

			this.model = argModel;
		}

		protected virtual void NotifyPropertyChanged(string argPropertyName)
		{
			PropertyChangedEventHandler tmpEH = this.PropertyChanged;
			if (tmpEH != null)
			{
				tmpEH(this, new PropertyChangedEventArgs(argPropertyName));
			}
		}

		protected virtual void ApplyChangesToModel(T argModel)
		{
		}

		public T Model
		{
			get
			{
				this.ApplyChangesToModel(this.model);
				return this.model;
			}
		}
	}
}
