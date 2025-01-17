using Lagrange.Core.Utility.Crypto.Provider.Ecdh;

namespace Lagrange.Core.Utility.Crypto;

internal partial class EcdhImpl
{
    public enum CryptMethod : uint
    {
        Secp192K1 = 1,
        Prime256V1 = 0x0201 << 16 | 0x0131
    }

    public enum CryptId
    {
        Ecdh135 = 0x87,
        Ecdh7 = 0x07
    }
    
    private static readonly Dictionary<CryptMethod, EcdhInfo> CurveTable = new()
    {
        { 
            CryptMethod.Prime256V1, new EcdhInfo
            {
                Curve = EllipticCurve.Prime256V1,
                Id = CryptId.Ecdh135,

                PubKey = new byte[] // From NTQQ Binary, by hook
                {
                    0x04,
                    0x9D, 0x14, 0x23, 0x33, 0x27, 0x35, 0x98, 0x0E,
                    0xDA, 0xBE, 0x7E, 0x9E, 0xA4, 0x51, 0xB3, 0x39,
                    0x5B, 0x6F, 0x35, 0x25, 0x0D, 0xB8, 0xFC, 0x56,
                    0xF2, 0x58, 0x89, 0xF6, 0x28, 0xCB, 0xAE, 0x3E,
                    0x8E, 0x73, 0x07, 0x79, 0x14, 0x07, 0x1E, 0xEE,
                    0xBC, 0x10, 0x8F, 0x4E, 0x01, 0x70, 0x05, 0x77,
                    0x92, 0xBB, 0x17, 0xAA, 0x30, 0x3A, 0xF6, 0x52, 
                    0x31, 0x3D, 0x17, 0xC1, 0xAC, 0x81, 0x5E, 0x79
                }
            }
        },
        {
            CryptMethod.Secp192K1, new EcdhInfo
            {
                Curve = EllipticCurve.Secp192K1,
                Id = CryptId.Ecdh7,
                
                PubKey = new byte[]
                {
                    0x04, 0x92, 0x8D, 0x88, 0x50, 0x67, 0x30, 0x88,
                    0xB3, 0x43, 0x26, 0x4E, 0x0C, 0x6B, 0xAC, 0xB8,
                    0x49, 0x6D, 0x69, 0x77, 0x99, 0xF3, 0x72, 0x11,
                    0xDE, 0xB2, 0x5B, 0xB7, 0x39, 0x06, 0xCB, 0x08,
                    0x9F, 0xEA, 0x96, 0x39, 0xB4, 0xE0, 0x26, 0x04,
                    0x98, 0xB5, 0x1A, 0x99, 0x2D, 0x50, 0x81, 0x3D,
                    0xA8
                }
            }
        }
    };
    
    private readonly record struct EcdhInfo(EllipticCurve Curve, CryptId Id, byte[] PubKey, byte[] SubPubKey);
}