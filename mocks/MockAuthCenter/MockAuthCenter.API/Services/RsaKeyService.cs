using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace MockAuthCenter.API.Services;

public class RsaKeyService
{
    public string KeyId { get; } = "mock-key-1";
    public RSA Rsa { get; }
    public RsaSecurityKey SecurityKey { get; }
    public SigningCredentials SigningCredentials { get; }

    public RsaKeyService()
    {
        Rsa = RSA.Create(2048);
        SecurityKey = new RsaSecurityKey(Rsa) { KeyId = KeyId };
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.RsaSha256);
    }

    public object GetJwks()
    {
        var parameters = Rsa.ExportParameters(false);

        return new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    kid = KeyId,
                    use = "sig",
                    alg = "RS256",
                    n = Base64UrlEncoder.Encode(parameters.Modulus!),
                    e = Base64UrlEncoder.Encode(parameters.Exponent!)
                }
            }
        };
    }
}
