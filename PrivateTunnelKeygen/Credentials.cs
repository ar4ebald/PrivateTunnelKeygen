using System;

namespace PrivateTunnelKeygen
{
    [Serializable]
    internal class Credentials
    {
        public Credentials(string email, string password, string passwordHash)
        {
            Email = email;
            Password = password;
            PasswordHash = passwordHash;
        }

        public string Email { get; }
        public string Password { get; }
        public string PasswordHash { get; }
    }
}
