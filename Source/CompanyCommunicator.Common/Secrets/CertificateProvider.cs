// <copyright file="CertificateProvider.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Secrets
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.CommonBot;

    /// <summary>
    /// This class implements ICertficateProvider, which is used to retrieve certificates.
    /// </summary>
    public class CertificateProvider : ICertificateProvider
    {
        private readonly Dictionary<string, string> certificateNameMap;
        private readonly bool useCertificate;
        private readonly ILogger<CertificateProvider> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateProvider"/> class.
        /// A constructor that accepts a map of bot id list and credentials.
        /// </summary>
        /// <param name="botOptions">bot options.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public CertificateProvider(
            IOptions<BotOptions> botOptions,
            ILoggerFactory loggerFactory)
        {
            botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.logger = loggerFactory?.CreateLogger<CertificateProvider>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.useCertificate = botOptions.Value.UseCertificate;
            if (this.useCertificate)
            {
                this.certificateNameMap = this.CreateCertificateNameMap(botOptions.Value);
            }
        }

        /// <inheritdoc/>
        public Task<X509Certificate2> GetCertificateAsync(string appId)
        {
            appId = appId ?? throw new ArgumentNullException(nameof(appId));

            var certificateName = this.certificateNameMap.ContainsKey(appId) ? this.certificateNameMap[appId] : null;

            if (string.IsNullOrEmpty(certificateName))
            {
                throw new InvalidOperationException("Certificate name not found.");
            }

            try
            {
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateName, false);
                if (certificates.Count > 0)
                {
                    return Task.FromResult(certificates[0]);
                }

                this.logger.LogError($"Certificate not found in store. Thumbprint: {certificateName} ");
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Failed to fetch certificate. Thumbprint: {certificateName}.");
            }

            throw new Exception($"Certificate not found. Thumbprint: {certificateName} ");
        }

        /// <inheritdoc/>
        public bool IsCertificateAuthenticationEnabled()
        {
            return this.useCertificate;
        }

        private Dictionary<string, string> CreateCertificateNameMap(BotOptions botOptions)
        {
            var certificateNameMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(botOptions.UserAppId))
            {
                throw new Exception("User app id not found.");
            }
            else
            {
                certificateNameMap.Add(botOptions.UserAppId, botOptions.UserAppCertName);
            }

            if (string.IsNullOrEmpty(botOptions.AuthorAppId))
            {
                throw new Exception("Author app id not found.");
            }
            else
            {
                certificateNameMap.Add(botOptions.AuthorAppId, botOptions.AuthorAppCertName);
            }

            if (string.IsNullOrEmpty(botOptions.GraphAppId))
            {
                throw new Exception("Graph app id not found.");
            }
            else
            {
                certificateNameMap.Add(botOptions.GraphAppId, botOptions.GraphAppCertName);
            }

            return certificateNameMap;
        }
    }
}
