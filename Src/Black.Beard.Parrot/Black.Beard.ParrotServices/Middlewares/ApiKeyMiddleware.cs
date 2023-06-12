

using Bb.Models.Security;
using System.Collections;
using System.Net;

namespace Bb.ParrotServices.Middlewares
{


    public class ApiKeyHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeyHandlerMiddleware(RequestDelegate next, IApiKeyRepository apiKeyRepository)
        {
            _next = next;
            _apiKeyRepository = apiKeyRepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (_apiKeyRepository.ApiKeyList.Items.Count == 0)
                await _next.Invoke(context);

            else
            {

                var apiKeysFromHeaders = _apiKeyRepository.GetApiKeysFromHeaders(context).ToList();

                if (ApiKeyFound(apiKeysFromHeaders))
                    await _next.Invoke(context);

                else if (ApiKeyNotFound(apiKeysFromHeaders))
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                else if (ApiKeyHasDuplicates(apiKeysFromHeaders))
                    throw new InvalidOperationException(CreateDuplicateApiKeyError(apiKeysFromHeaders));

            }
        }

        private static bool ApiKeyHasDuplicates(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count > 1;
        private static bool ApiKeyNotFound(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count < 1;
        private static bool ApiKeyFound(ICollection apiKeysFromHeaders) => apiKeysFromHeaders.Count == 1;

        private static string CreateDuplicateApiKeyError(IEnumerable<ApiKey> apiKeysFromHeaders)
        {
            return "The following ApiKey's share the same id.  Fix immediately!!! | " +
                   $"{string.Join(", ", apiKeysFromHeaders.Select(apiKey => apiKey.Owner))}";
        }
    }


    //public class ApiKeyMiddleware
    //{

    //    private readonly RequestDelegate _next;
    //    private ApiKeyReferential? _service;
    //    private const string APIKEYNAME = null; // "ApiKey";

    //    public ApiKeyMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    public async Task InvokeAsync(HttpContext context)
    //    {

    //        if (_service == null)
    //        {

    //            _service = (ApiKeyReferential)context.RequestServices.GetService(typeof(ApiKeyReferential));
    //            if (_service != null && _service.Datas.IsEmpty)
    //            {
    //                var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
    //                appSettings.Bind("apikeys", _service.Datas);

    //            }

    //        }

    //        var result = _service.Evaluate(context.Request.Headers, out HashSet<RoleEnum> roles);
    //        if (result.HasValue)
    //        {
    //            context.Response.StatusCode = 401;
    //            await context.Response.WriteAsync("Api Key was not provided.");
    //        }

    //        var p1 = Thread.CurrentPrincipal;
    //        var p2 = context.User;

    //        //KeyValuePair<string, string> authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
    //        //if (authCookie == null)
    //        //    return;
    //        //FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
    //        //string[] customData = authTicket.UserData.Split(new Char[] { '|' });

    //        //if (context.User.Identity.IsAuthenticated == true)
    //        //{
    //        //    if (context.User.Identity.AuthenticationType == "Forms")
    //        //    {
    //        //        context.User = new CustomPrincipal(customData, context.User);
    //        //        Thread.CurrentPrincipal = context.User;
    //        //    }
    //        //}

    //        await _next(context);
    //    }
    //}

}
