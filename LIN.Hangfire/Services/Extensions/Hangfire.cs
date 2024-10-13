﻿using Hangfire;

namespace LIN.Hangfire.Services.Extensions;

public static class Hangfire
{

    /// <summary>
    /// Agregar servicios de Hangfire.
    /// </summary>
    /// <param name="services">Servicios.</param>
    /// <param name="manager">Manager.</param>
    public static IServiceCollection AddSettingsHangfire(this IServiceCollection services, IConfigurationManager manager)
    {

        // Add Hangfire services.
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(manager.GetConnectionString("hangfire"), new()
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
            });
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
        });

        services.AddHangfireServer();

        // Jobs.
        services.AddSingleton<Jobs.ServicesOnlineJob, Jobs.ServicesOnlineJob>();
        return services;
    }


    /// <summary>
    /// Usar servicios de Hangfire.
    /// </summary>
    /// <param name="app">App.</param>
    public static IApplicationBuilder UseSettingsHangfire(this IApplicationBuilder app)
    {
        // Configuración del tablero.
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AsyncAuthorization = [new Authorization.IdentityAuthorization()],
            DarkModeEnabled = true,
            DashboardTitle = "LIN Hangfire"
        });

        // Agregar job recurrente.
        RecurringJob.AddOrUpdate<Jobs.ServicesOnlineJob>("servicesJob", (v) => v.Run(), $"*/{2} * * * *");

        return app;
    }

}