using Bb.Codings;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApiServices
{

    public static class GeneratorHelper
    {

        public static string CodeHttp(this string codeKey)
        {

            switch (codeKey)
            {
                case "100": return "StatusCodes.Status100Continue";
                case "101": return "StatusCodes.Status101SwitchingProtocols";
                case "102": return "StatusCodes.Status102Processing";

                //  Ok
                case "200": return "StatusCodes.Status200OK";
                case "201": return "StatusCodes.Status201Created";
                case "202": return "StatusCodes.Status202Accepted";
                case "203": return "StatusCodes.Status203NonAuthoritative";
                case "204": return "StatusCodes.Status204NoContent";
                case "205": return "StatusCodes.Status205ResetContent";
                case "206": return "StatusCodes.Status206PartialContent";
                case "207": return "StatusCodes.Status207MultiStatus";
                case "208": return "StatusCodes.Status208AlreadyReported";
                case "226": return "StatusCodes.Status226IMUsed";

                // 
                case "300": return "StatusCodes.Status300MultipleChoices";
                case "301": return "StatusCodes.Status301MovedPermanently";
                case "302": return "StatusCodes.Status302Found";
                case "303": return "StatusCodes.Status303SeeOther";
                case "304": return "StatusCodes.Status304NotModified";
                case "305": return "StatusCodes.Status305UseProxy";
                case "306": return "StatusCodes.Status306SwitchProxy";                      // RFC 2616, removed
                case "307": return "StatusCodes.Status307TemporaryRedirect";
                case "308": return "StatusCodes.Status308PermanentRedirect";

                // request Error
                case "400": return "StatusCodes.Status400BadRequest";
                case "401": return "StatusCodes.Status401Unauthorized";
                case "402": return "StatusCodes.Status402PaymentRequired";
                case "403": return "StatusCodes.Status403Forbidden";
                case "404": return "StatusCodes.Status404NotFound";
                case "405": return "StatusCodes.Status405MethodNotAllowed";
                case "406": return "StatusCodes.Status406NotAcceptable";
                case "407": return "StatusCodes.Status407ProxyAuthenticationRequired";
                case "408": return "StatusCodes.Status408RequestTimeout";
                case "409": return "StatusCodes.Status409Conflict";
                case "410": return "StatusCodes.Status410Gone";
                case "411": return "StatusCodes.Status411LengthRequired";
                case "412": return "StatusCodes.Status412PreconditionFailed";
                case "413": return "StatusCodes.Status413RequestEntityTooLarge";            // RFC 2616, renamed
                //case "413": return "StatusCodes.Status413PayloadTooLarge";                  // RFC 7231
                case "414": return "StatusCodes.Status414RequestUriTooLong";                // RFC 2616, renamed
                //case "414": return "StatusCodes.Status414UriTooLong";                       // RFC 7231
                case "415": return "StatusCodes.Status415UnsupportedMediaType";
                case "416": return "StatusCodes.Status416RequestedRangeNotSatisfiable";     // RFC 2616, renamed
                //case "416": return "StatusCodes.Status416RangeNotSatisfiable";              // RFC 7233
                case "417": return "StatusCodes.Status417ExpectationFailed";
                case "418": return "StatusCodes.Status418ImATeapot";
                case "419": return "StatusCodes.Status419AuthenticationTimeout";            // Not defined in any RFC
                case "421": return "StatusCodes.Status421MisdirectedRequest";
                case "422": return "StatusCodes.Status422UnprocessableEntity";
                case "423": return "StatusCodes.Status423Locked";
                case "424": return "StatusCodes.Status424FailedDependency";
                case "426": return "StatusCodes.Status426UpgradeRequired";
                case "428": return "StatusCodes.Status428PreconditionRequired";
                case "429": return "StatusCodes.Status429TooManyRequests";
                case "431": return "StatusCodes.Status431RequestHeaderFieldsTooLarge";
                case "451": return "StatusCodes.Status451UnavailableForLegalReasons";

                // Internals Error
                case "500": return "StatusCodes.Status500InternalServerError";
                case "501": return "StatusCodes.Status501NotImplemented";
                case "502": return "StatusCodes.Status502BadGateway";
                case "503": return "StatusCodes.Status503ServiceUnavailable";
                case "504": return "StatusCodes.Status504GatewayTimeout";
                case "505": return "StatusCodes.Status505HttpVersionNotsupported";
                case "506": return "StatusCodes.Status506VariantAlsoNegotiates";
                case "507": return "StatusCodes.Status507InsufficientStorage";
                case "508": return "StatusCodes.Status508LoopDetected";
                case "510": return "StatusCodes.Status510NotExtended";
                case "511": return "StatusCodes.Status511NetworkAuthenticationRequired";

            }

            return codeKey;

        }


        public static void ApplyHttpMethod(this string keyText, CsMethodDeclaration method)
        {

            var key = keyText.ToLower();
            switch (key)
            {

                case "get":
                    method.Attribute("HttpGet");
                    break;

                case "post":
                    method.Attribute("HttpPost");
                    break;

                case "delete":
                    method.Attribute("HttpDelete");
                    break;

                case "options":
                    method.Attribute("HttpOptions");
                    break;

                case "patch":
                    method.Attribute("Patch");
                    break;

                case "put":
                    method.Attribute("HttpPut");
                    break;

                default:
                    CodeHelper.Stop();
                    break;

            }


        }


        public static void ApplyAttributes(this CsParameterDeclaration self, OpenApiParameter source)
        {
            switch (source.In.Value)
            {

                case ParameterLocation.Query:
                    self.Attribute("FromQuery");
                    break;

                case ParameterLocation.Header:
                    self.Attribute("FromHeader");
                    break;

                case ParameterLocation.Path:
                    self.Attribute("FromRoute");
                    break;

                case ParameterLocation.Cookie:
                    CodeHelper.Stop();
                    break;

                default:
                    CodeHelper.Stop();
                    break;

            }

        }

        /// <summary>
        ///generate random code
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomCode(int length = 4, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];
            var code = new string(stringChars);
            return code;
        }

    }


}