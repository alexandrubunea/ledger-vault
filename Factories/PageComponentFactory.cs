using System;
using ledger_vault.Data;
using ledger_vault.ViewModels;

namespace ledger_vault.Factories;

public class PageComponentFactory
{
    private readonly Func<PageComponents, PageComponentViewModel> _factory;

    public PageComponentFactory(Func<PageComponents, PageComponentViewModel> factory)
    {
        _factory = factory;
    }

    public PageComponentViewModel GetComponentPageViewModel(PageComponents pageComponent) =>
        _factory.Invoke(pageComponent);
}