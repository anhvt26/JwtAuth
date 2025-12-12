using JwtAuth.Constants;

namespace JwtAuth.Identity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorisedAttribute : Attribute
    {
        public bool Required { get;}

        public AuthGroup Group { get; }

        public AuthorisedAttribute()
        {
            Required = true;
            Group = AuthGroup.All;
        }

        public AuthorisedAttribute(bool required)
        {
            Required = required;
            Group = AuthGroup.All;
        }

        public AuthorisedAttribute(AuthGroup group)
        {
            Required = true;
            Group = group;
        }

        public bool IsAccessable (int accountType)
        {
            return Group switch
            {
                AuthGroup.All => true,
                AuthGroup.User => accountType == AccountType.User,
                AuthGroup.Admin=> accountType == AccountType.Admin,
                _ => throw new NotImplementedException("Unknown AuthGroup value."),
            };
        }
    }

    public enum AuthGroup
    {
        All,
        User,
        Admin
    }
}
