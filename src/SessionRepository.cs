// using System;
// using System.Data;
// using Npgsql;
// using System.Security.Cryptography;
// using System.Text;

// namespace MTCG;

// public class SessionRepository : BaseRepository<Session>, IRepository<Session>
// {
//     private const int LENGTH_AUTH_TOKEN = 10;

//     public SessionRepository() : base()
//     {
//         _Table = "authtokes";
//         _Fields = "id, userid, token";
//     }


//     [Obsolete("Gets handled by SessionManager")]
//     public override void Save(Session session)
//     {
//         throw new Exception("Not implemented. Use SessionManager to save session.");
//     }


//     private string CreateAuthToken()
//     {
//         Random random = new Random();
//         string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
//         char[] stringChars = new char[LENGTH_AUTH_TOKEN];

//         for (int i = 0; i < stringChars.Length; i++)
//         {
//             stringChars[i] = chars[random.Next(chars.Length)];
//         }

//         return new String(stringChars);
//     }

//     public bool CredentialsAreValid(string username, string password)
//     {
//         bool credentialsAreCorrect = false;
//         var encryptedPassword = CryptoHandler.Encode(password);

//         using (NpgsqlConnection? connection = this.Connect())
//         using (var command = new NpgsqlCommand($"SELECT * FROM users WHERE name=@name AND password=@password"))
//         {
//             command.Parameters.AddWithValue("@name", username);
//             command.Parameters.AddWithValue("@password", encryptedPassword);

//             IDataReader re = command.ExecuteReader();
//             credentialsAreCorrect = re.Read();

//             command.Dispose(); connection!.Dispose();

//             return credentialsAreCorrect;
//         }
//     }

//     [Obsolete("Gets handled by SessionManager")]
//     public override Session? Get(Guid id)
//     {
//         throw new Exception("Not implemented. Use SessionManager to get session.");
//     }



//     [Obsolete("Gets handled by SessionManager")]
//     public override void Fill(Session obj, IDataReader re)
//     {
//         throw new NotImplementedException();
//     }



//     [Obsolete("Gets handled by SessionManager")]
//     protected override void _Fill(Session session, IDataReader re)
//     {
//         throw new Exception("Not implemented. Gets handled by SessionManager.");
//     }


// }