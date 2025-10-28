using System.Security.Cryptography.X509Certificates;

namespace Verifactu.Client.Services;

public class CertificateLoader : ICertificateLoader
{
    public X509Certificate2 CargarDesdePfx(string rutaPfx, string password)
    {
        return new X509Certificate2(
            rutaPfx, password,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet);
    }
}
