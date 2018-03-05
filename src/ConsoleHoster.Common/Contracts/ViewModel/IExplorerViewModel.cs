//-----------------------------------------------------------------------
// <copyright file="IExplorerViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using ConsoleHoster.Common.Model;
using ConsoleHoster.Common.ViewModel;

namespace ConsoleHoster.Common.Contracts.ViewModel
{
    public interface IExplorerViewModel
    {
        void OnItemChosen(ExplorerItem argItem);

        void OnOpenExplorer(ExplorerItem argItem);

        void OnGoToItem(ExplorerItem argItem);

        void RunItem(ExplorerItem argItem);
    }
}