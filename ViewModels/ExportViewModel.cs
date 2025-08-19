using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public partial class ExportViewModel : PageViewModel
{
    #region PUBLIC API

    public ExportViewModel()
    {
        PageName = ApplicationPages.Export;
    }

    #endregion

    #region PRIVATE PROPERTIES

    [ObservableProperty] private int _selectedExportFormat;
    private static readonly List<string> ExportFormats = ["PDF", "CSV", "XAML", "JSON", "XLSX"];

    #endregion
}