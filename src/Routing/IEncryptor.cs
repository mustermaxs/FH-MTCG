using System.Security.Cryptography;
using System.Text;

namespace MTCG;

/// <summary>
/// Represents a cryptographic handler for encoding and validating data.
/// </summary>
public class CryptoHandler 
{
    /// <summary>
    /// Encodes the specified data using SHA256 hashing algorithm.
    /// </summary>
    /// <param name="data">The data to be encoded.</param>
    /// <returns>The encoded data as a hexadecimal string.</returns>
    public static string Encode(string data)
    {
        return ToHex(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    public static string Decode(string data)
    {
        return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(data)));
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Checks if the encrypted data matches the encoded version of the decrypted data.
    /// </summary>
    /// <param name="encryptedData">The encrypted data.</param>
    /// <param name="decryptedData">The decrypted data.</param>
    /// <returns>True if the encrypted data matches the encoded version of the decrypted data, otherwise false.</returns>
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