using System;

namespace MTCG;

public interface IAuthenticator
{
    public bool ValidateToken(string token);
}