using System;
using System.IO;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace ledger_vault.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAppServices(this IServiceCollection collection)
        {
            #region SERVICES

            collection.AddSingleton<UserStateService>();
            collection.AddSingleton<CoreViewNavigatorService>();
            collection.AddSingleton<DatabaseManagerService>();
            collection.AddSingleton<AuthService>();
            collection.AddSingleton<HmacService>();
            collection.AddSingleton<UserRepository>();
            collection.AddSingleton<TransactionRepository>();
            collection.AddSingleton<TransactionService>();
            collection.AddSingleton(typeof(MediatorService<>));

            #endregion

            #region SECURITY

            collection.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LedgerVault",
                        "keys")))
                .SetApplicationName("LedgerVault");

            #endregion

            #region FUNCTIONS

            collection.AddSingleton<Func<ApplicationPages, PageViewModel>>(x => name => name switch
            {
                ApplicationPages.Home => x.GetRequiredService<HomeViewModel>(),
                ApplicationPages.Settings => x.GetRequiredService<SettingsViewModel>(),
                ApplicationPages.Backups => x.GetRequiredService<BackupsViewModel>(),
                ApplicationPages.CashFlow => x.GetRequiredService<CashFlowViewModel>(),
                ApplicationPages.Transaction => x.GetRequiredService<TransactionsViewModel>(),
                ApplicationPages.Export => x.GetRequiredService<ExportViewModel>(),
                ApplicationPages.VerifyIntegrity => x.GetRequiredService<VerifyIntegrityViewModel>(),
                _ => throw new InvalidOperationException(),
            });
            collection.AddSingleton<Func<CoreViews, CoreViewModel>>(x => name => name switch
            {
                CoreViews.Main => x.GetRequiredService<MainViewModel>(),
                CoreViews.Login => x.GetRequiredService<LoginViewModel>(),
                CoreViews.Setup => x.GetRequiredService<SetupViewModel>(),
                _ => throw new InvalidOperationException(),
            });
            collection.AddSingleton<Func<PageComponents, PageComponentViewModel>>(x => name => name switch
            {
                PageComponents.TransactionForm => x.GetRequiredService<TransactionFormViewModel>(),
                PageComponents.TransactionList => x.GetRequiredService<TransactionsListViewModel>(),
                _ => throw new InvalidOperationException(),
            });

            #endregion

            #region FACTORIES

            collection.AddSingleton<PageFactory>();
            collection.AddSingleton<CoreViewFactory>();
            collection.AddSingleton<PageComponentFactory>();

            #endregion

            #region VIEWMODELS

            collection.AddTransient<SetupViewModel>();
            collection.AddTransient<LoginViewModel>();
            collection.AddTransient<MainViewModel>();
            collection.AddTransient<BackupsViewModel>();
            collection.AddTransient<CashFlowViewModel>();
            collection.AddTransient<ExportViewModel>();
            collection.AddTransient<HomeViewModel>();
            collection.AddTransient<TransactionsViewModel>();
            collection.AddTransient<SettingsViewModel>();
            collection.AddTransient<VerifyIntegrityViewModel>();
            collection.AddTransient<TransactionFormViewModel>();
            collection.AddTransient<TransactionsListViewModel>();

            #endregion

            #region WINDOW

            collection.AddSingleton<MainWindowViewModel>();

            #endregion
        }
    }
}