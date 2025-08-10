using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class CoreViewFactory(Func<CoreViews, CoreViewModel> factory)
{
    #region PUBLIC API

    public CoreViewModel GetCoreViewModel(CoreViews coreName) => factory.Invoke(coreName);

    #endregion
}