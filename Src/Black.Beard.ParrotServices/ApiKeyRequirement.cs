using Microsoft.AspNetCore.Authorization;


namespace Bb.ParrotServices
{

    /// <summary>
    /// A requirement that an ApiKey must be present.
    /// </summary>
    public class ApiKeyRequirement : IAuthorizationRequirement
    {

        /// <summary>
        /// Get the list of api keys
        /// </summary>
        public IReadOnlyList<string> ApiKeys { get; }

        /// <summary>
        /// Get the policy name,
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// Create a new instance of the <see cref="ApiKeyRequirement"/> class.
        /// </summary>
        /// <param name="apiKeys"></param>
        /// <param name="policyName"></param>
        public ApiKeyRequirement(IEnumerable<string> apiKeys, string policyName)
        {
            ApiKeys = apiKeys?.ToList() ?? new List<string>();
            PolicyName = policyName;
        }

    }
}
