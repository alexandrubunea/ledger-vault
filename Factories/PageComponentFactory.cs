using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class PageComponentFactory(Func<PageComponents, PageComponentViewModel> factory)
{
    #region PUBLIC API

    public PageComponentViewModel GetComponentPageViewModel(PageComponents pageComponent) =>
        factory.Invoke(pageComponent);

    #endregion
}