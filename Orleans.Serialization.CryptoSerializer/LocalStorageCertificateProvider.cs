using Orleans.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Orleans.Serialization.CryptoSerializer
{
    public class LocalStorageCertificateProvider
    {
        private Logger logger;
        public string CertficiateThumbprint { get; set; }

        public X509Certificate2 Certificate { get; private set; }

        public LocalStorageCertificateProvider()
        {

        }
        
        public LocalStorageCertificateProvider(string certificateThumbprint)
        {
            this.CertficiateThumbprint = certificateThumbprint;
        }

        public void Initialize(Logger logger)
        {
            this.logger = logger;
            X509Store store = new X509Store();
            CertficiateThumbprint = CertficiateThumbprint.Replace("\u200e", string.Empty).Replace("\u200f", string.Empty).Replace(" ", string.Empty);
            Certificate = GetByStore(StoreLocation.LocalMachine);
            Certificate = Certificate ?? GetByStore(StoreLocation.CurrentUser);
            if (Certificate == null)
            {
                logger.Error(404,$"Certifcate with thumbprint {CertficiateThumbprint} not found ");

                throw new CertificateNotFoundException($"Certifcate with thumbprint {CertficiateThumbprint} not found ");
            }
        }

        private X509Certificate2 GetByStore(StoreLocation location)
        {
            X509Store  store = new X509Store();
            ;
            try
            {
                logger.Info($"Initializing search from certificate with thumbprint {CertficiateThumbprint} in StoreLocation {location}");

                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Cast<X509Certificate2>().Where(x=> x.Thumbprint.ToUpper().Contains(CertficiateThumbprint.ToUpper()));
                if (!certCollection.Any())
                    logger.Info($"Certificate with thumbprint {CertficiateThumbprint}  cannot be found in StoreLocation {location}");
                else
                    logger.Info($"Certificate with thumbprint {CertficiateThumbprint}  was be found in StoreLocation {location}");

                return certCollection.FirstOrDefault();
            }
            finally
            {
                store.Close();
            }

        }
    }
}