// using System;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.IdentityModel.Tokens;

// public class UserAuthenticator
// {
//     private readonly string secretKey;

//     public UserAuthenticator(string secretKey)
//     {
//         this.secretKey = secretKey;
//     }

//     public string GenerateToken(string userId, string username, int expiresInMinutes = 30)
//     {
//         var tokenHandler = new JwtSecurityTokenHandler();
//         var key = Encoding.ASCII.GetBytes(secretKey);

//         var tokenDescriptor = new SecurityTokenDescriptor
//         {
//             Subject = new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, userId),
//                 new Claim(ClaimTypes.Name, username)
//             }),
//             Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
//             SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//         };

//         var token = tokenHandler.CreateToken(tokenDescriptor);
//         return tokenHandler.WriteToken(token);
//     }

//     public ClaimsPrincipal ValidateToken(string token)
//     {
//         var tokenHandler = new JwtSecurityTokenHandler();
//         var key = Encoding.ASCII.GetBytes(secretKey);

//         var validationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(key),
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             RequireExpirationTime = true,
//             ValidateLifetime = true
//         };

//         try
//         {
//             return tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
//         }
//         catch (Exception)
//         {
//             // Token validation failed
//             return null;
//         }
//     }
// }
