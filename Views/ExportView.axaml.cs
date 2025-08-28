using System.Collections.Generic;
using Avalonia.Controls;

namespace ledger_vault.Views;

public partial class ExportView : UserControl
{
    private static readonly List<string> ExportFormats = ["PDF", "CSV", "XML", "JSON", "XLSX"];
    
    public ExportView()
    {
        InitializeComponent();
        FormatsComboBox.ItemsSource = ExportFormats;
    }
}
