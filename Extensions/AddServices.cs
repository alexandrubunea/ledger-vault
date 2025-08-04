using System;
using Avalonia;
using ledger_vault.Data;
using ledger_vault.Factories;
using ledger_vault.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ledger_vault.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAppServices(this IServiceCollection collection)
        {
            // Services
            collection.AddSingleton<UserStateService>();

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

            // Factories
            collection.AddSingleton<PageFactory>();

            // ViewModels
            collection.AddTransient<SetupViewModel>();
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