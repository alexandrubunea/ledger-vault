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
            // Services
            collection.AddSingleton<UserStateService>();
            collection.AddSingleton<CoreViewNavigatorService>();
            collection.AddSingleton<DatabaseManagerService>();
            collection.AddSingleton<AuthService>();
            collection.AddSingleton<HmacService>();

            // Security
            collection.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LedgerVault",
                        "keys")))
                .SetApplicationName("LedgerVault");

            // Functions
            collection.AddSingleton<Func<ApplicationPages, PageViewModel>>(x => name => name switch
            {
                ApplicationPages.Home => x.GetRequiredService<HomeViewModel>(),
                ApplicationPages.Settings => x.GetRequiredService<SettingsViewModel>(),
                ApplicationPages.Backups => x.GetRequiredService<BackupsViewModel>(),
                ApplicationPages.CashFlow => x.GetRequiredService<CashFlowViewModel>(),
                ApplicationPages.Income => x.GetRequiredService<IncomeViewModel>(),
                ApplicationPages.Payments => x.GetRequiredService<PaymentsViewModel>(),
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

            // Factories
            collection.AddSingleton<PageFactory>();
            collection.AddSingleton<CoreViewFactory>();

            // ViewModels
            collection.AddTransient<SetupViewModel>();
            collection.AddTransient<LoginViewModel>();
            collection.AddTransient<MainViewModel>();
            collection.AddTransient<BackupsViewModel>();
            collection.AddTransient<CashFlowViewModel>();
            collection.AddTransient<ExportViewModel>();
            collection.AddTransient<HomeViewModel>();
            collection.AddTransient<IncomeViewModel>();
            collection.AddTransient<PaymentsViewModel>();
            collection.AddTransient<SettingsViewModel>();
            collection.AddTransient<VerifyIntegrityViewModel>();

            // Window
            collection.AddSingleton<MainWindowViewModel>();
        }
    }
}