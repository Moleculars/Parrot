using Bb.Extensions;
using Bb.Models.Security;
using Bb.ParrotServices.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SharpYaml.Tokens;
using System.Collections;
using System.Net;
using System.Security.Claims;

namespace Bb.Middlewares.ApiKeys
{


    public class ApiKeyHandlerMiddleware
    {


        public ApiKeyHandlerMiddleware(RequestDelegate next, IApiKeyRepository apiKeyRepository, ILogger<ApiKeyHandlerMiddleware> logger)
        {
            _next = next;
            _apiKeyRepository = apiKeyRepository;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            ClaimsPrincipal principal = null;

            var apiKeysFromHeaders = _apiKeyRepository.GetApiKeysFromHeaders(context).ToList();

            if (_apiKeyRepository.ApiKeyList.Items.Count == 0)
            {

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, "no key registered"),
                    new Claim(ClaimTypes.Role, "Administrator")
                };
                principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "default", ClaimTypes.Name, ClaimTypes.Role));

            }

            else if (ApiKeyFound(apiKeysFromHeaders))
            {

                principal = new ClaimsPrincipal
                (
                    new ClaimsIdentity(apiKeysFromHeaders.Consolidate(), "default", ClaimTypes.Name, ClaimTypes.Role)
                );

            }


            if (principal != null)
            {

                context.User = principal;
                Thread.CurrentPrincipal = principal;

                using (_logger.BeginScope("{username}", context.User.Identity.Name))
                {
                    await _next.Invoke(context);
                }

            }

            else if (ApiKeyNotFound(apiKeysFromHeaders))
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            else if (ApiKeyHasDuplicates(apiKeysFromHeaders))
                throw new InvalidOperationException(CreateDuplicateApiKeyError(apiKeysFromHeaders));


        }

        private static bool ApiKeyHasDuplicates(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count > 1;
        private static bool ApiKeyNotFound(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count < 1;
        private static bool ApiKeyFound(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count == 1;

        private static string CreateDuplicateApiKeyError(IEnumerable<ApiKey> apiKeysFromHeaders)
        {
            return "The following ApiKey's share the same id.  Fix immediately!!! | " +
                   $"{string.Join(", ", apiKeysFromHeaders.Select(apiKey => apiKey.Owner))}";
        }

        private readonly RequestDelegate _next;
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly ILogger<ApiKeyHandlerMiddleware> _logger;


    }

}
