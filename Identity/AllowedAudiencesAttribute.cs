namespace JwtAuth.Identity
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AllowedAudiencesAttribute : Attribute
    {
        public string[] Audiences { get; }
        public AllowedAudiencesAttribute(params string[] audiences)
        {
            Audiences = audiences;
        }
    }

}
