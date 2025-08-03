using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class PageFactory
{
    private readonly Func<ApplicationPages, PageViewModel> _factory;
    
    public PageFactory(Func<ApplicationPages, PageViewModel> factory)
    {
        _factory = factory;    
    }
    
    public PageViewModel GetPageViewModel(ApplicationPages pageName) =>_factory.Invoke(pageName);
}