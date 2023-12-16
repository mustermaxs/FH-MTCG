using System.Security.Cryptography;
using System.Text;

namespace MTCG;

// public interface IKeyManager
// {
//     public string Encode(string password);
//     protected string EncodeImplementation(string password);
//     public bool IsValid(string password, string encryptedPassword);
// }

public class CryptoHandler 
{
    public static string Encode(string data)
    {
        return ToHex(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    public bool IsValid(string encryptedData, string decryptedData)
    {
        return encryptedData == Encode(decryptedData);
    }

    private static string ToHex(byte[] bytes)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++) result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }
}