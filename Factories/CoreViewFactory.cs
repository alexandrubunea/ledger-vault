using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class CoreViewFactory
{
    private readonly Func<CoreViews, CoreViewModel> _factory;
    
    public CoreViewFactory(Func<CoreViews, CoreViewModel> factory)
    {
        _factory = factory;    
    }
    
    public CoreViewModel GetCoreViewModel(CoreViews coreName) =>_factory.Invoke(coreName);
}