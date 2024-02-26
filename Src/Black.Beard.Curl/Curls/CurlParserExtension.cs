using Bb.Util;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Bb.Curls
{

    /// <summary>
    /// Specialized curl command parser
    /// </summary>
    public static class CurlParserExtension
    {

        /// <summary>
        /// Initializes the <see cref="CurlParserExtension"/> class.
        /// </summary>
        static CurlParserExtension()
        {

            string pattern = @"(https?|ftp|ssh|mailto):\/\/([a-z]+[a-z0-9.-]+|(\d{1,3}\.){3,3}\d{1,3})(:\d{0,5})?(\/[a-z]+[a-z0-9.-]+)*(\?([a-z]+[a-z0-9%&=+#]*)+)?";
            RegexOptions options = RegexOptions.IgnoreCase;
            _regIsUrl = new Regex(pattern, options);

        }

        /// <summary>
        /// Parses the curl line.
        /// </summary>
        /// <param name="lineArg">The line argument.</param>
        /// <returns></returns>
        public static string[] ParseCurlLine(this string lineArg)
        {

            var _lexer = new CurlLexer(lineArg);

            List<string> result = new List<string>();
            while (_lexer.Next())
            {
                var c = _lexer.Current;
                if (!string.IsNullOrEmpty(c))
                    result.Add(_lexer.Current);
            }

            return result.ToArray();

        }

        public static CurlInterpreter Precompile(this string lineArg)
        {
            var interpreter = new CurlInterpreter(lineArg.ParseCurlLine());
            interpreter.Precompile();
            return interpreter;
        }

        /// <summary>
        /// makes asynchronous curl Calls.
        /// </summary>
        /// <param name="lineArg">The line argument.</param>
        /// <param name="cancellationTokenSource">The cancellation token to cancel operation.<see cref="CancellationTokenSource"/></param>
        /// <returns><see cref="T:Task<HttpResponseMessage>"/>The task object representing the asynchronous operation.</returns>
        public static async Task<HttpResponseMessage?> CallAsync(this string lineArg, CancellationTokenSource cancellationTokenSource = null)
        {
            var interpreter = new CurlInterpreter(lineArg.ParseCurlLine());
            return await interpreter.CallAsync(cancellationTokenSource);
        }

        /// <summary>
        /// makes asynchronous curl Calls
        /// </summary>
        /// <param name="arguments">The list of argument.</param>
        /// <param name="cancellationTokenSource">The cancellation token to cancel operation.<see cref="CancellationTokenSource"/></param>
        /// <returns><see cref="T:Task<HttpResponseMessage>"/>The task object representing the asynchronous operation.</returns>
        public static async Task<HttpResponseMessage?> CallAsync(this string[] arguments, CancellationTokenSource cancellationTokenSource = null)
        {
            var interpreter = new CurlInterpreter(arguments);
            return await interpreter.CallAsync(cancellationTokenSource);
        }

        /// <summary>
        /// Determines whether this instance is an URL.
        /// </summary>
        /// <param name="self">The self text to test.</param>
        /// <returns>
        ///   <c>true</c> if the specified self is URL; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUrl(this string self)
        {
            return _regIsUrl.IsMatch(self);
        }



        /// <summary>
        /// Results to string.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static string? ResultToString(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.CallToStringAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to string asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<string?> CallToStringAsync(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var response = await self.CallAsync();
            if (response != null)
            {
                if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return responseBody;
            }

            return null;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static object? ResultToJson<T>(this CurlInterpreter self, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var e = self.CallToJsonAsync<T>(ensureSuccessStatusCode, options, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<object?> CallToJsonAsync<T>(this CurlInterpreter self, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var response = await self.CallAsync();
            if (response != null)
            {
                if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                object? responseBody = await response.Content.ReadFromJsonAsync<T>(options, cancellationToken);
                return responseBody;
            }

            return null;

        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="type"><see cref="Type"/> </param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="options"><see cref="JsonSerializerOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static object? CallToJson(this CurlInterpreter self, Type type, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var e = self.CallToJsonAsync(type, ensureSuccessStatusCode, options, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="type"><see cref="Type"/> </param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="options"><see cref="JsonSerializerOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<object?> CallToJsonAsync(this CurlInterpreter self, Type type, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var response = await self.CallAsync();
            if (response != null)
            {
                if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                object? responseBody = await response.Content.ReadFromJsonAsync(type, options, cancellationToken);
                return responseBody;

            }

            return null;

        }

        /// <summary>
        /// Results to byte array.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static byte[]? CallToByteArray(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.CallToByteArrayAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to byte array asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<byte[]?> CallToByteArrayAsync(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var response = await self.CallAsync();
            if (response != null)
            {
                if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return responseBody;
            }

            return null;
        }

        /// <summary>
        /// Results to stream.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static Stream? ResultToStream(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.CallToStreamAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to stream asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<Stream?> CallToStreamAsync(this CurlInterpreter self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var response = await self.CallAsync();
            if (response != null)
            {
                if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                Stream responseBody = await response.Content.ReadAsStreamAsync(cancellationToken);
                return responseBody;
            }
            return null;
        }




        /// <summary>
        /// Results to string.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static string ResultToString(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.ResultToStringAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to string asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<string> ResultToStringAsync(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            self.Wait();
            var response = self.Result;
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseBody;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static object? ResultToJson<T>(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var e = self.ResultToJsonAsync<T>(ensureSuccessStatusCode, options, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<object?> ResultToJsonAsync<T>(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            self.Wait();
            var response = self.Result;
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                response.EnsureSuccessStatusCode();
            object? responseBody = await response.Content.ReadFromJsonAsync<T>(options, cancellationToken);
            return responseBody;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="type"><see cref="Type"/> </param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="options"><see cref="JsonSerializerOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static object? ResultToJson(this Task<HttpResponseMessage> self, Type type, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            var e = self.ResultToJsonAsync(type, ensureSuccessStatusCode, options, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to json asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="type"><see cref="Type"/> </param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="options"><see cref="JsonSerializerOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<object?> ResultToJsonAsync(this Task<HttpResponseMessage> self, Type type, bool ensureSuccessStatusCode = false, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            self.Wait();
            var response = self.Result;
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                response.EnsureSuccessStatusCode();
            object? responseBody = await response.Content.ReadFromJsonAsync(type, options, cancellationToken);
            return responseBody;
        }

        /// <summary>
        /// Results to byte array.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static byte[] CallToByteArray(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.ResultToByteArrayAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to byte array asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<byte[]> ResultToByteArrayAsync(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            self.Wait();
            var response = self.Result;
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                response.EnsureSuccessStatusCode();
            byte[] responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return responseBody;
        }

        /// <summary>
        /// Results to stream.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static Stream CallToStream(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            var e = self.ResultToStreamAsync(ensureSuccessStatusCode, cancellationToken);
            e.Wait();
            return e.Result;
        }

        /// <summary>
        /// Results to stream asynchronous.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="ensureSuccessStatusCode">If true and the http result code is not between 200 and 299, throw <see cref="HttpRequestException"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <exception cref="HttpRequestException">if the result is not between 200 and 299</exception>
        /// <returns></returns>
        public static async Task<Stream> ResultToStreamAsync(this Task<HttpResponseMessage> self, bool ensureSuccessStatusCode = false, CancellationToken cancellationToken = default)
        {
            self.Wait();
            var response = self.Result;
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
                response.EnsureSuccessStatusCode();
            Stream responseBody = await response.Content.ReadAsStreamAsync(cancellationToken);
            return responseBody;
        }



        private static readonly Regex _regIsUrl;

    }


}
