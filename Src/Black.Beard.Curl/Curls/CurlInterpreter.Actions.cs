using System;
using static System.Collections.Specialized.BitVector32;
using System.Linq;
using Bb;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bb.Http.Configuration;
using Bb.Http;

namespace Bb.Curls
{

    // https://curlconverter.com/csharp/
    public partial class CurlInterpreter
    {

        /// <summary>
        /// Sets the URL.
        /// </summary>
        /// <returns></returns>
        internal CurlInterpreterAction SetUrl()
        {

            var arg = new Argument(_arguments[_index]);

            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {

                 var client = context.HttpClient;
                 var message = context.RequestMessage;

                 var productValue = new ProductInfoHeaderValue("black.beard.curl", "1.0");
                 var url = new Url(sender.FirstValue);
                 
                 client.DefaultRequestHeaders.UserAgent.Add(productValue);
                 message.RequestUri = new Uri(url);
                 message.Headers.Host = $"{url.Host}:{url.Port}";

                 context.Request = url.Request();

                 return await sender.CallNext(context);

             };

            return new CurlInterpreterAction(action, arg) { Priority = 0 };

        }

        internal CurlInterpreterAction Header()
        {

            bool isMultipart = false;
            var arg = _arguments[++_index].Split(':');

            var a = new Argument(arg[1].Trim(), arg[0].Trim());
            if (a.Name == "Content-Type" && a.Value.ToLower().Trim().StartsWith("multipart"))
                isMultipart = true;

            static async Task<HttpResponseMessage> action(CurlInterpreterAction sender, Context context)
            {

                var message = context.RequestMessage;

                var arg = sender.First;
                if (message.Content != null)
                {
                    switch (arg.Name.ToLower())
                    {

                        case "accept":
                            message.Headers.Add("Accept", arg.Value);
                            break;

                        default:
                            message.Content.Headers.TryAddWithoutValidation(arg.Name, arg.Value);
                            break;
                    }

                    message.Content.Headers.TryAddWithoutValidation(arg.Name, arg.Value);

                }
                else
                {
                    context.Request.Headers.Add(arg.Name, arg.Name);
                    message.Headers.TryAddWithoutValidation(arg.Name, arg.Value);
                }

                return await sender.CallNext(context);

            }

            return new CurlInterpreterAction(action, a)
            {
                Priority = 100
            }
            .Add("isMultipart", isMultipart ? "1" : "0");

        }

        /// <summary>
        /// <command> Specify request command to use
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal CurlInterpreterAction Request()
        {

            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = null;

            var arg = _arguments[++_index];
            switch (arg.ToUpper())
            {
                case "POST":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Post;
                        return await sender.CallNext(context);
                    };
                    break;

                case "GET":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Get;

                        context.Request.Verb = HttpMethod.Get;

                        return await sender.CallNext(context);
                    };
                    break;

                case "HEAD":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Head;
                        return await sender.CallNext(context);
                    };
                    break;
                case "PUT":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Put;
                        return await sender.CallNext(context);
                    };
                    break;

                case "DELETE":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Delete;
                        return await sender.CallNext(context);
                    };
                    break;

                case "CONNECT":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = new HttpMethod("CONNECT");
                        Stop();
                        return await sender.CallNext(context);
                    };
                    break;
                case "OPTIONS":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Options;
                        return await sender.CallNext(context);
                    };
                    break;

                case "TRACE":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = HttpMethod.Trace;
                        return await sender.CallNext(context);
                    };
                    break;

                case "PATCH":
                    action = async (sender, context) =>
                    {
                        var message = context.RequestMessage;
                        message.Method = new HttpMethod("PATCH");
                        Stop();
                        return await sender.CallNext(context);
                    };
                    break;
            }

            return new CurlInterpreterAction(action) { Priority = 10 };

        }

        internal CurlInterpreterAction Form()
        {

            var arg = _arguments[++_index].Split(';');
            List<Argument> _args = new List<Argument>(arg.Length);

            foreach (var item in arg)
            {
                var i = item.Split('=');
                _args.Add(new Argument(i[1], i[0].Trim()));
            }

            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
            {

                var args = sender.Arguments;
                var client = context.HttpClient;
                var message = context.RequestMessage;

                if (context.Has("IsMultipart", "1"))
                {

                    MultipartFormDataContent content = new MultipartFormDataContent();

                    var f = sender.Get(c => c.Value.StartsWith("@"));
                    if (f != null)
                        UploadFile(content, f, sender);
                    else
                    {
                        Stop();
                    }

                    message.Content = content;
                    return await sender.CallNext(context);

                }
                else
                {

                    var formVariables = new List<KeyValuePair<string, string>>();

                    foreach (var item in args)
                    {
                        if (item.Value.StartsWith("@"))
                        {
                            Stop();
                        }
                        else
                            formVariables.Add(new KeyValuePair<string, string>(item.Name.Trim(), item.Value.Trim()));
                    }

                    using (var formContent = new FormUrlEncodedContent(formVariables))
                    {
                        message.Content = formContent;

                        return await sender.CallNext(context);

                    }

                }

            };

            return new CurlInterpreterAction(action, _args.ToArray());

        }

        private static void UploadFile(MultipartFormDataContent content, Argument f, CurlInterpreterAction sender)
        {

            var file = new FileInfo(f.Value.Substring(1));

            var c = new ByteArrayContent(File.ReadAllBytes(file.FullName));
            c.Headers.ContentDisposition = new ContentDispositionHeaderValue($"form-data")
            {
                Name = "\"" + f.Name + "\"",
                FileName = "\"" + file.Name + "\"",
                FileNameStar = string.Empty
            };

            foreach (var item in sender.Arguments)
            {
                switch (item.Name)
                {

                    case "type":
                        c.Headers.ContentType = new MediaTypeHeaderValue(item.Value);
                        break;

                    case "upfile":
                    default:
                        break;
                }
            }

            content.Add(c, "\"" + f.Name + "\"", "\"" + file.Name + "\"");

        }

        internal CurlInterpreterAction Head()
        {
            Stop();

            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };

        }

        internal CurlInterpreterAction HaproxyProtocol()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction HappyEyeballsTimeoutMs()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);

             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Globoff()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Get()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpSslControl()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpSslCccMode()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpSslCcc()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpSkipPasvIp()
        {
            Stop();

            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpPret()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpPort()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpPasv()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpMethod()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpCreateDirs()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpAlternativeToUser()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FtpAccount()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FormString()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FalseStart()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction FailEarly()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Fail()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Expect100Timeout()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Engine()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction EgdFile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DumpHeader()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DohUrl()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DnsServers()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DnsIpv6Addr()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DnsIpv4Addr()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DnsInterface()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DisallowUsernameInUrl()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DisableEpsv()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DisableEprt()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Disable()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Digest()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Delegation()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DataUrlencode()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DataRaw()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DataBinary()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction DataAscii()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Data()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Crlfile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Crlf()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CreateDirs()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CookieJar()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Cookie()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ContinueAt()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ConnectTo()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ConnectTimeout()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Config()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CompressedSsh()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Compressed()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Ciphers()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CertType()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CertStatus()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Cert()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Capath()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction CaCert()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Basic()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Append()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyNegotiate()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyKeyType()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyKey()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyInsecure()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyHeader()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyDigest()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCrlfile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCiphers()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCertType()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCert()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCapath()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyCacert()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyBasic()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyAnyauth()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Proxy()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProtoRedir()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProtoDefault()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Proto()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProgressBar()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Preproxy()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Post303()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Post302()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Post301()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Pinnedpubkey()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction PathAsIs()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Pass()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Output()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Oauth2Bearer()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NtlmWb()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Ntlm()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Noproxy()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NoSessionid()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NoNpn()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NoKeepalive()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NoBuffer()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NoAlpn()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Next()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NetrcOptional()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction NetrcFile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Netrc()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Negotiate()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Metalink()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MaxTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MaxRedirs()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MaxFilesize()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Manual()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MailRcpt()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MailFrom()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction MailAuth()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction LoginOptions()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction LocationTrusted()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Location()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction LocalPort()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ListOnly()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction LimitRate()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Libcurl()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Krb()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction KeyType()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Key()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction KeepaliveTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction JunkSessionCookies()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Ipv6()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Ipv4()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Interface
        {
            get
            {
                Stop();
                Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
                 {
                     Stop();
                     return await sender.CallNext(context);
                 };

                return new CurlInterpreterAction(action) { Priority = 20 };
            }
        }

        internal CurlInterpreterAction Insecure()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Include()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction IgnoreContentLength()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Http2PriorKnowledge()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Http2()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Http11()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Http10()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Http09()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Hostpubmd5()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Help()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RemoteHeaderName()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Referer()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Raw()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Range()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RandomFile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Quote()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Pubkey()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Proxytunnel()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Proxy10()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyUser()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyTlsv1()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyTlsuser()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyTlspassword()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyTlsauthtype()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyTls13Ciphers()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxySslAllowBeast()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyServiceName()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyPinnedpubkey()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyPass()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ProxyNtlm()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }


        internal CurlInterpreterAction Anyauth()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction AltSvc()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction AbstractUnixSocket()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Xattr()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction WriteOut()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Version()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Verbose()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction UserAgent()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction User()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction UseAscii()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Url()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction UploadFile()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction UnixSocket()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TraceTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TraceAscii()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Trace()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TrEncoding()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsv13()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsv12()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsv11()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsv10()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsv1()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsuser()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlspassword()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tlsauthtype()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Tls13Ciphers()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TlsMax()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TimeCond()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TftpNoOptions()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TftpBlksize()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TelnetOption()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TcpNodelay()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction TcpFastopen()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SuppressConnectHeaders()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction StyledOutput()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Stderr()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Sslv3()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Sslv2()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SslReqd()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SslNoRevoke()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SslAllowBeast()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Ssl()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SpeedTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SpeedLimit()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5Hostname()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5GssapiService()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5GssapiNec()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5Gssapi()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5Basic()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks5()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks4a()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Socks4()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Silent()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ShowError()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction ServiceName()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction SaslIr()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RetryMaxTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RetryRelay()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RetryConnrefused()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Retry()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction Resolve()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RequestTarget()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RemoteTime()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RemoteNameAll()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

        internal CurlInterpreterAction RemoteName()
        {
            Stop();
            Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> action = async (sender, context) =>
             {
                 Stop();
                 return await sender.CallNext(context);
             };

            return new CurlInterpreterAction(action) { Priority = 20 };
        }

    }


}
