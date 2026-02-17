using System;
using System.Collections.Generic;
using System.Text;
using Cati.ADP.Common;
using Cati.ADP.Server;

namespace Cati.ADP.Client {
    /// <summary>
    /// Scope internal to the namespace;
    /// Factory used to return a IADPProvider according to given ADPProviderType
    /// </summary>
    internal static class ADPProviderFactory {
        public static IADPProvider GetProvider(ADPProviderType providerType) {
            switch (providerType) {
                case ADPProviderType.Local:
                    return new ADPProvider();
                case ADPProviderType.Remote:
                    return new ADPClient();
                default:
                    return null;
            }
        }
    }
}
