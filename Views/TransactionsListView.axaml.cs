using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ledger_vault.Views;

public partial class TransactionsListView : UserControl
{
    public TransactionsListView()
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