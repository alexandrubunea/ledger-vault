using ledger_vault.Data;

namespace ledger_vault.ViewModels;

public class PaymentsViewModel : PageViewModel
{
    #region PUBLIC API

    public PaymentsViewModel()
    {
        PageName = ApplicationPages.Payments;
    }

    #endregion
}