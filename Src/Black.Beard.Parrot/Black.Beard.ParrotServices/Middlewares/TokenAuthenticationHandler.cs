//using Bb.ParrotServices;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.Options;
//using Microsoft.Extensions.Primitives;
//using System.Net.Sockets;
//using System.Security.Claims;
//using System.Text.Encodings.Web;

//namespace Bb.Middlewares
//{


//    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
//    {
//        public IServiceProvider ServiceProvider { get; set; }

//        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
//            : base(options, logger, encoder, clock)
//        {
//            ServiceProvider = serviceProvider;
//        }

//        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
//        {
//            return base.HandleChallengeAsync(properties);
//        }

//        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
//        {
//            return base.HandleForbiddenAsync(properties);
//        }

//        protected override Task InitializeEventsAsync()
//        {
//            return base.InitializeEventsAsync();
//        }

//        protected override Task<object> CreateEventsAsync()
//        {
//            return base.CreateEventsAsync();
//        }

//        protected override Task InitializeHandlerAsync()
//        {
//            return base.InitializeHandlerAsync();
//        }

//        protected override string? ResolveTarget(string? scheme)
//        {
//            return base.ResolveTarget(scheme);
//        }

//        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
//        {
//            var headers = Request.Headers;

//            if (headers.TryGetValue("X-API-KEY", out StringValues token))
//            {


//                if (string.IsNullOrEmpty(token))
//                {
//                    return Task.FromResult(AuthenticateResult.Fail("Token is null"));
//                }

//                bool isValidToken = false; // check token here

//                if (!isValidToken)
//                {
//                    return Task.FromResult(AuthenticateResult.Fail($"Balancer not authorize token : for token={token}"));
//                }

//                var claims = new[] { new Claim("token", token) };
//                var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
//                var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
//                return Task.FromResult(AuthenticateResult.Success(ticket));
//            }


//            return Task.FromResult(AuthenticateResult.Fail(new Exception()));

//        }
//    }

//}
