using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

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