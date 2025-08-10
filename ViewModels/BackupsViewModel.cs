using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class BackupsViewModel : PageViewModel
{
    #region PUBLIC API

    public BackupsViewModel()
    {
        PageName = ApplicationPages.Backups;
    }

    #endregion
}