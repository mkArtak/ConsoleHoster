//-----------------------------------------------------------------------
// <copyright file="SearchViewModel.cs" author="Artak Mkrtchyan">
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
using System.Threading.Tasks;
using System.Windows.Input;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model;

namespace ConsoleHoster.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        public event Action<string> FindNext;

        private bool searchEnabled = false;
        private string textToFind = null;
        private ICommand closeSearchCommand = null;
        private ICommand findNextCommand = null;

        public SearchViewModel()
        {

        }

        protected override void InitializeDesignMode()
        {
            base.InitializeDesignMode();
            this.SearchEnabled = true;
        }

        protected override void InitializeRuntimeMode()
        {
            base.InitializeRuntimeMode();
            this.SearchEnabled = false;
        }

        public void CloseSearch()
        {
            this.SearchEnabled = false;
            this.TextToFind = String.Empty;
        }

        private void OnFind()
        {
            Action<string> temp = this.FindNext;
            if (temp != null)
            {
                temp(this.TextToFind);
            }
        }

        public bool SearchEnabled
        {
            get
            {
                return this.searchEnabled;
            }
            set
            {
                if (value != this.searchEnabled)
                {
                    this.searchEnabled = value;
                    this.NotifyPropertyChanged("SearchEnabled");
                }
            }
        }

        public string TextToFind
        {
            get
            {
                return this.textToFind;
            }
            set
            {
                if (value != this.textToFind)
                {
                    this.textToFind = value;
                    this.NotifyPropertyChanged("TextToFind");
                }
            }
        }

        public ICommand CloseSearchCommand
        {
            get
            {
                if (this.closeSearchCommand == null)
                {
                    this.closeSearchCommand = new RelayCommand(o => this.CloseSearch());
                }
                return this.closeSearchCommand;
            }
        }

        public ICommand FindNextCommand
        {
            get
            {
                if (this.findNextCommand == null)
                {
                    this.findNextCommand = new RelayCommand(o => this.OnFind());
                }
                return this.findNextCommand;
            }
        }
    }
}