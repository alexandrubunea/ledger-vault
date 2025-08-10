using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class PageFactory(Func<ApplicationPages, PageViewModel> factory)
{
    #region PUBLIC API

    public PageViewModel GetPageViewModel(ApplicationPages pageName) => factory.Invoke(pageName);

    #endregion
}