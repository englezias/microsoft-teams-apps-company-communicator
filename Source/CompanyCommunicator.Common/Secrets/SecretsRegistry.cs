// <copyright file="SecretsRegistry.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Secrets
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Register secrets dependencies.
    /// </summary>
    public static class SecretsRegistry
    {
        /// <summary>
        /// Service Collection extension.
        ///
        /// Injects secrets provider.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <returns>the service collection.</returns>
        public static IServiceCollection AddSecretsProvider(this IServiceCollection services)
        {
            services.AddSingleton<ICertificateProvider, CertificateProvider>();

            return services;
        }
    }
}
