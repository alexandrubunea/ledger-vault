using System;
using Avalonia.Controls;

namespace ledger_vault.Views;

public partial class TransactionFormView : UserControl
{
    public TransactionFormView()
    {
        InitializeComponent();
    }

    private void OnUnloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
