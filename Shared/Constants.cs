namespace Shared
{
    /// <summary>
    /// Provides constant values like the authority infos and credentials.
    /// </summary>
    public static class Constants
    {
        public const string Authority = "https://demo.identityserver.io/"; // Authority url
        public const string MvcClientId = "server.hybrid"; // Client id for the Mvc client
        public const string MvcClientSecret = "secret"; // Client secret
        public const string MobileClientId = "native.hybrid"; // Client id for the mobile client
        public const string Issuer = "https://demo.identityserver.io/"; // Token issuer
        public const string SampleApiScope = "api"; // Scope name of the sample API known by authority
        public const string SampleApiUrl = "https://xamexpertopenidapi.azurewebsites.net";
    }
}
