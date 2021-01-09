using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Rex.Tests
{
    public static class TestTokens
    {
        public const string SigningKey = "your-256-bit-secret";

        public static readonly Guid PrincipalId = Guid.Parse("d6cf5e7f-b12a-444d-8c7f-6790b77e49a9");

        public static readonly string EmailAddress = "testy@testerson.com";

        public static readonly string EmailHash = "37b2dd1da1a74fda515b862567c422ef";

        public static string GetToken(IEnumerable<string>? roles = null, IEnumerable<string>? scopes = null)
        {
            roles ??= Array.Empty<string>();
            scopes = new[] { "user_impersonation" }.Concat(scopes ?? Array.Empty<string>());

            var tokenHandler = new JsonWebTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "tests",
                Issuer = "tests",
                Claims = new Dictionary<string, object>
                {
                    ["name"] = "Testy McTesterson",
                    ["given_name"] = "Testy",
                    ["family_name"] = "McTesterson",
                    ["unique_name"] = EmailAddress,
                    ["upn"] = EmailAddress,
                    ["acr"] = "1",
                    ["amr"] = new[] { "pwd", "mfa" },
                    ["roles"] = roles.ToArray(),
                    ["oid"] = PrincipalId.ToString(),
                    ["appid"] = Guid.NewGuid().ToString(),
                    ["deviceid"] = Guid.NewGuid().ToString(),
                    ["sub"] = "Xwq2sQJEYUbxkwV_0V9Gg_nIAW2mWX9tJnt_Gqrkdbm",
                    ["tid"] = Guid.NewGuid().ToString(),
                    ["scp"] = string.Join(" ", scopes),
                    ["ver"] = "1.0",
                },
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256Signature),
            };

            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}