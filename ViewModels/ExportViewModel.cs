using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class ExportViewModel : PageViewModel
{
    #region PUBLIC API

    public ExportViewModel()
    {
        PageName = ApplicationPages.Export;
    }

    #endregion
}