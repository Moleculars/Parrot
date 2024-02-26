using Bb.Http;
using Bb.Http.Configuration;

namespace Bb.Curls
{

    public partial class CurlInterpreter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CurlInterpreter"/> class.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        public CurlInterpreter(string[] arguments)
        {
            this.Factory = FlurlHttp.GlobalSettings.FlurlClientFactory;
            this._arguments = arguments;
            this._index = 0;
            this._max = this._arguments.Length;
            _handlers = new List<CurlInterpreterAction>();
        }


        public HttpResponseMessage? Call(CancellationTokenSource source = null)
        {
            var task = CallAsync(source);
            var awaiter = task.GetAwaiter();
            return awaiter.GetResult();
        }


        public async Task<HttpResponseMessage?> CallAsync(CancellationTokenSource source = null)
        {

            var act = GetActions();
            if (act == null) return null;



            using (var message = new HttpRequestMessage())
            {

                var context = act.CollectParameters(new Context(source)
                {

                    HttpClient = new HttpClient(),
                    RequestMessage = message,
                    Factory = this.Factory,
                });

                return await act.CallAsync(context);

            }

        }

        public IFlurlClientFactory Factory { get; set; }

        internal void Precompile()
        {
            GetActions();
        }

        private CurlInterpreterAction? GetActions()
        {

            if (_first == null && _index < this._max)
                lock (_lock)
                    if (_first == null)
                    {

                        _current = this._arguments[this._index];

                        if (_current.ToLower() == "curl" || _current.ToLower() == "curl.exe")
                            this._index++;

                        ParseArguments();

                        var list = _handlers.OrderBy(c => c.Priority).ToList();

                        if (list.Count > 0)
                        {
                            var first = list[0];

                            if (list.Count - 1 > 1)
                                first.Append(list, 1);

                            _first = first;
                        }

                    }

            return _first;

        }

        private void ParseArguments()
        {
            while ((_index < this._max))
            {

                _current = this._arguments[this._index];
                _isUrl = _current.IsUrl();

                if (_isUrl)
                    Append(SetUrl());

                else
                {

                    switch (_current)
                    {

                        case "--request":
                        case "-X":                          // <command> Specify request command to use
                            Append(Request());
                            break;

                        case "--header":
                        case "-H":                          // <header/@file> Pass custom header(s) to server
                            Append(Header());
                            break;

                        case "--append":
                        case "-a":                          // Append to target file when uploading
                            Append(Append());
                            break;

                        case "--basic":                     // Use HTTP Basic Authentication
                            Append(Basic());
                            break;

                        case "--cacert":                    // <file> CA certificate to verify peer against
                            Append(CaCert());
                            break;

                        case "--capath":                    // <dir>  CA directory to verify peer against
                            Append(Capath());
                            break;

                        case "--cert":
                        case "-E":                          // <certificate[:password]> Client certificate file and password
                            Append(Cert());
                            break;

                        case "--cert-status":               // Verify the status of the server certificate
                            Append(CertStatus());
                            break;

                        case "--cert-type":                 // <type> Certificate file type (DER/PEM/ENG)
                            Append(CertType());
                            break;

                        case "--ciphers":                   // <list of ciphers> SSL ciphers to use
                            Append(Ciphers());
                            break;

                        case "--compressed":                // Request compressed response
                            Append(Compressed());
                            break;

                        case "--compressed-ssh":            // Enable SSH compression
                            Append(CompressedSsh());
                            break;

                        case "--config":
                        case "-K":                          // <file> Read config from a file
                            Append(Config());
                            break;

                        case "--connect-timeout":           // <seconds> Maximum time allowed for connection
                            Append(ConnectTimeout());
                            break;

                        case "--connect-to":                // <HOST1:PORT1:HOST2:PORT2> Connect to host
                            Append(ConnectTo());
                            break;

                        case "--continue-at":
                        case "-C":                          // <offset> Resumed transfer offset
                            Append(ContinueAt());
                            break;

                        case "--cookie":
                        case "-b":                          // <data|filename> Send cookies from string/file
                            Append(Cookie());
                            break;

                        case "--cookie-jar":
                        case "-c":                          // <filename> Write cookies to <filename> after operation
                            Append(CookieJar());
                            break;

                        case "--create-dirs":               // Create necessary local directory hierarchy
                            Append(CreateDirs());
                            break;

                        case "--crlf":                      // Convert LF to CRLF in upload
                            Append(Crlf());
                            break;

                        case "--crlfile":                   // <file> Get a CRL list in PEM format from the given file
                            Append(Crlfile());
                            break;

                        case "--data":
                        case "-d":                          // <data>   HTTP POST data
                            Append(Data());
                            break;

                        case "--data-ascii":                // <data> HTTP POST ASCII data
                            Append(DataAscii());
                            break;

                        case "--data-binary":               // <data> HTTP POST binary data
                            Append(DataBinary());
                            break;

                        case "--data-raw":                  // <data> HTTP POST data, '@' allowed
                            Append(DataRaw());
                            break;

                        case "--data-urlencode":            // <data> HTTP POST data url encoded
                            Append(DataUrlencode());
                            break;

                        case "--delegation":                // <LEVEL> GSS-API delegation permission
                            Append(Delegation());
                            break;

                        case "--digest":                    // Use HTTP Digest Authentication
                            Append(Digest());
                            break;

                        case "--disable":
                        case "-q":                          // Disable .curlrc
                            Append(Disable());
                            break;

                        case "--disable-eprt":              // Inhibit using EPRT or LPRT
                            Append(DisableEprt());
                            break;

                        case "--disable-epsv":              // Inhibit using EPSV
                            Append(DisableEpsv());
                            break;

                        case "--disallow-username-in-url":  // Disallow username in url
                            Append(DisallowUsernameInUrl());
                            break;

                        case "--dns-interface":             // <interface> Interface to use for DNS requests
                            Append(DnsInterface());
                            break;

                        case "--dns-ipv4-addr":             // <address> IPv4 address to use for DNS requests
                            Append(DnsIpv4Addr());
                            break;

                        case "--dns-ipv6-addr":             // <address> IPv6 address to use for DNS requests
                            Append(DnsIpv6Addr());
                            break;

                        case "--dns-servers":               // <addresses> DNS server addrs to use
                            Append(DnsServers());
                            break;

                        case "--doh-url":                   // <URL> Resolve host names over DOH
                            Append(DohUrl());
                            break;

                        case "--dump-header":
                        case "-D":                          // <filename> Write the received headers to <filename>
                            Append(DumpHeader());
                            break;

                        case "--egd-file":                  // <file> EGD socket path for random data
                            Append(EgdFile());
                            break;

                        case "--engine":                    // <name> Crypto engine to use
                            Append(Engine());
                            break;

                        case "--expect100-timeout":         // <seconds> How long to wait for 100-continue
                            Append(Expect100Timeout());
                            break;

                        case "--fail":
                        case "-f":                          // Fail silently (no output at all) on HTTP errors
                            Append(Fail());
                            break;

                        case "--fail-early":                // Fail on first transfer error, do not continue
                            Append(FailEarly());
                            break;

                        case "--false-start":               // Enable TLS False Start
                            Append(FalseStart());
                            break;

                        case "--form":
                        case "-F":                          // <name=content> Specify multipart MIME data
                            Append(Form());
                            break;

                        case "--form-string":               // <name=string> Specify multipart MIME data
                            Append(FormString());
                            break;

                        case "--ftp-account":               // <data> Account data string
                            Append(FtpAccount());
                            break;

                        case "--ftp-alternative-to-user":   // <command> String to replace USER [name]
                            Append(FtpAlternativeToUser());
                            break;

                        case "--ftp-create-dirs":           // Create the remote dirs if not present
                            Append(FtpCreateDirs());
                            break;

                        case "--ftp-method":                // <method> Control CWD usage
                            Append(FtpMethod());
                            break;

                        case "--ftp-pasv":                  // Use PASV/EPSV instead of PORT
                            Append(FtpPasv());
                            break;

                        case "--ftp-port":
                        case "-P":                          // <address> Use PORT instead of PASV
                            Append(FtpPort());
                            break;

                        case "--ftp-pret":                  // Send PRET before PASV
                            Append(FtpPret());
                            break;

                        case "--ftp-skip-pasv-ip":          // Skip the IP address for PASV
                            Append(FtpSkipPasvIp());
                            break;

                        case "--ftp-ssl-ccc":               // Send CCC after authenticating
                            Append(FtpSslCcc());
                            break;

                        case "--ftp-ssl-ccc-mode":          // <active/passive> Set CCC mode
                            Append(FtpSslCccMode());
                            break;

                        case "--ftp-ssl-control":           // Require SSL/TLS for FTP login, clear for transfer
                            Append(FtpSslControl());
                            break;

                        case "--get":
                        case "-G":
                            Append(Get());
                            break;                          // Put the post data in the URL and use GET

                        case "--globoff":
                        case "-g":                          // Disable URL sequences and ranges using {} and []
                            Append(Globoff());
                            break;

                        case "--happy-eyeballs-timeout-ms": // <milliseconds> How long to wait in milliseconds for IPv6 before trying IPv4
                            Append(HappyEyeballsTimeoutMs());
                            break;

                        case "--haproxy-protocol":          // Send HAProxy PROXY protocol v1 header
                            Append(HaproxyProtocol());
                            break;

                        case "--head":
                        case "-I":                          // Show document info only
                            Append(Head());
                            break;

                        case "--help":
                        case "-h":                          // This help text
                            Append(Help());
                            break;

                        case "--hostpubmd5":                // <md5> Acceptable MD5 hash of the host public key
                            Append(Hostpubmd5());
                            break;

                        case "--http0.9":                   // Allow HTTP 0.9 responses
                            Append(Http09());
                            break;

                        case "--http1.0":
                        case "-0":                          // Use HTTP 1.0
                            Append(Http10());
                            break;

                        case "--http1.1":                   // Use HTTP 1.1
                            Append(Http11());
                            break;

                        case "--http2":                     // Use HTTP 2
                            Append(Http2());
                            break;

                        case "--http2-prior-knowledge":     // Use HTTP 2 without HTTP/1.1 Upgrade
                            Append(Http2PriorKnowledge());
                            break;

                        case "--ignore-content-length":     // Ignore the size of the remote resource
                            Append(IgnoreContentLength());
                            break;

                        case "--include":
                        case "-i":                          // Include protocol response headers in the output
                            Append(Include());
                            break;

                        case "--insecure":
                        case "-k":                          // Allow insecure server connections when using SSL
                            Append(Insecure());
                            break;

                        case "--interface":                 // <name> Use network INTERFACE (or address)
                            Append(Interface);
                            break;

                        case "--ipv4":
                        case "-4":                          // Resolve names to IPv4 addresses
                            Append(Ipv4());
                            break;

                        case "--ipv6":
                        case "-6":                          // Resolve names to IPv6 addresses
                            Append(Ipv6());
                            break;

                        case "--junk-session-cookies":
                        case "-j":                          // Ignore session cookies read from file
                            Append(JunkSessionCookies());
                            break;

                        case "--keepalive-time":            // <seconds> Interval time for keepalive probes
                            Append(KeepaliveTime());
                            break;

                        case "--key":                       // <key>     Private key file name
                            Append(Key());
                            break;

                        case "--key-type":                  // <type> Private key file type (DER/PEM/ENG)
                            Append(KeyType());
                            break;

                        case "--krb":                       // <level>   Enable Kerberos with security <level>
                            Append(Krb());
                            break;

                        case "--libcurl":                   // <file> Dump libcurl equivalent code of this command line
                            Append(Libcurl());
                            break;

                        case "--limit-rate":                // <speed> Limit transfer speed to RATE
                            Append(LimitRate());
                            break;

                        case "--list-only":
                        case "-l":                          // List only mode
                            Append(ListOnly());
                            break;

                        case "--local-port":                // <num/range> Force use of RANGE for local port numbers
                            Append(LocalPort());
                            break;

                        case "--location":
                        case "-L":                          // Follow redirects
                            Append(Location());
                            break;

                        case "--location-trusted":          // Like --location, and send auth to other hosts
                            Append(LocationTrusted());
                            break;

                        case "--login-options":             // <options> Server login options
                            Append(LoginOptions());
                            break;

                        case "--mail-auth":                 // <address> Originator address of the original email
                            Append(MailAuth());
                            break;

                        case "--mail-from":                 // <address> Mail from this address
                            Append(MailFrom());
                            break;

                        case "--mail-rcpt":                 // <address> Mail to this address
                            Append(MailRcpt());
                            break;

                        case "--manual":
                        case "-M":                          // Display the full manual
                            Append(Manual());
                            break;

                        case "--max-filesize":              // <bytes> Maximum file size to download
                            Append(MaxFilesize());
                            break;

                        case "--max-redirs":                // <num> Maximum number of redirects allowed
                            Append(MaxRedirs());
                            break;

                        case "--max-time":
                        case "-m":                          // <seconds> Maximum time allowed for the transfer
                            Append(MaxTime());
                            break;

                        case "--metalink":                  // Process given URLs as metalink XML file
                            Append(Metalink());
                            break;

                        case "--negotiate":                 // Use HTTP Negotiate (SPNEGO) authentication
                            Append(Negotiate());
                            break;

                        case "--netrc":
                        case "-n":                          // Must read .netrc for user name and password
                            Append(Netrc());
                            break;

                        case "--netrc-file":                // <filename> Specify FILE for netrc
                            Append(NetrcFile());
                            break;

                        case "--netrc-optional":            // Use either .netrc or URL
                            Append(NetrcOptional());
                            break;

                        case "--next":
                        case "-:":                          // Make next URL use its separate set of options
                            Append(Next());
                            break;

                        case "--no-alpn":                   // Disable the ALPN TLS extension
                            Append(NoAlpn());
                            break;

                        case "--no-buffer":
                        case "-N":                          // Disable buffering of the output stream
                            Append(NoBuffer());
                            break;

                        case "--no-keepalive":              // Disable TCP keepalive on the connection
                            Append(NoKeepalive());
                            break;

                        case "--no-npn":                    // Disable the NPN TLS extension
                            Append(NoNpn());
                            break;

                        case "--no-sessionid":              // Disable SSL session-ID reusing
                            Append(NoSessionid());
                            break;

                        case "--noproxy":                   // <no-proxy-list> List of hosts which do not use proxy
                            Append(Noproxy());
                            break;

                        case "--ntlm":                      // Use HTTP NTLM authentication
                            Append(Ntlm());
                            break;

                        case "--ntlm-wb":                   // Use HTTP NTLM authentication with winbind
                            Append(NtlmWb());
                            break;

                        case "--oauth2-bearer":             // <token> OAuth 2 Bearer Token
                            Append(Oauth2Bearer());
                            break;

                        case "--output":
                        case "-o":                          // <file> Write to file instead of stdout
                            Append(Output());
                            break;

                        case "--pass":                      // <phrase> Pass phrase for the private key
                            Append(Pass());
                            break;
                        case "--path-as-is":                // Do not squash .. sequences in URL path
                            Append(PathAsIs());
                            break;

                        case "--pinnedpubkey":              // <hashes> FILE/HASHES Public key to verify peer against
                            Append(Pinnedpubkey());
                            break;

                        case "--post301":                   // Do not switch to GET after following a 301
                            Append(Post301());
                            break;

                        case "--post302":                   // Do not switch to GET after following a 302
                            Append(Post302());
                            break;

                        case "--post303":                   // Do not switch to GET after following a 303
                            Append(Post303());
                            break;

                        case "--preproxy":                  // [protocol://]host[:port] Use this proxy first
                            Append(Preproxy());
                            break;

                        case "--progress-bar":
                        case "-#":                          // Display transfer progress as a bar
                            Append(ProgressBar());
                            break;

                        case "--proto":                     // <protocols> Enable/disable PROTOCOLS
                            Append(Proto());
                            break;

                        case "--proto-default":             // <protocol> Use PROTOCOL for any URL missing a scheme
                            Append(ProtoDefault());
                            break;

                        case "--proto-redir":               // <protocols> Enable/disable PROTOCOLS on redirect
                            Append(ProtoRedir());
                            break;

                        case "--proxy":
                        case "-x":                          // [protocol://]host[:port] Use this proxy
                            Append(Proxy());
                            break;

                        case "--proxy-anyauth":             // Pick any proxy authentication method
                            Append(ProxyAnyauth());
                            break;

                        case "--proxy-basic":               // Use Basic authentication on the proxy
                            Append(ProxyBasic());
                            break;

                        case "--proxy-cacert":              // <file> CA certificate to verify peer against for proxy
                            Append(ProxyCacert());
                            break;

                        case "--proxy-capath":              // <dir> CA directory to verify peer against for proxy
                            Append(ProxyCapath());
                            break;

                        case "--proxy-cert":                // <cert[:passwd]> Set client certificate for proxy
                            Append(ProxyCert());
                            break;

                        case "--proxy-cert-type":           // <type> Client certificate type for HTTPS proxy
                            Append(ProxyCertType());
                            break;

                        case "--proxy-ciphers":             // <list> SSL ciphers to use for proxy
                            Append(ProxyCiphers());
                            break;

                        case "--proxy-crlfile":             // <file> Set a CRL list for proxy
                            Append(ProxyCrlfile());
                            break;

                        case "--proxy-digest":              // Use Digest authentication on the proxy
                            Append(ProxyDigest());
                            break;

                        case "--proxy-header":              // <header/@file> Pass custom header(s) to proxy
                            Append(ProxyHeader());
                            break;

                        case "--proxy-insecure":            // Do HTTPS proxy connections without verifying the proxy
                            Append(ProxyInsecure());
                            break;

                        case "--proxy-key":                 // <key> Private key for HTTPS proxy
                            Append(ProxyKey());
                            break;

                        case "--proxy-key-type":            // <type> Private key file type for proxy
                            Append(ProxyKeyType());
                            break;

                        case "--proxy-negotiate":           // Use HTTP Negotiate (SPNEGO) authentication on the proxy
                            Append(ProxyNegotiate());
                            break;

                        case "--proxy-ntlm":                // Use NTLM authentication on the proxy
                            Append(ProxyNtlm());
                            break;

                        case "--proxy-pass":                // <phrase> Pass phrase for the private key for HTTPS proxy
                            Append(ProxyPass());
                            break;

                        case "--proxy-pinnedpubkey":        // <hashes> FILE/HASHES public key to verify proxy with
                            Append(ProxyPinnedpubkey());
                            break;

                        case "--proxy-service-name":        // <name> SPNEGO proxy service name
                            Append(ProxyServiceName());
                            break;

                        case "--proxy-ssl-allow-beast":     // Allow security flaw for interop for HTTPS proxy
                            Append(ProxySslAllowBeast());
                            break;

                        case "--proxy-tls13-ciphers":       // <ciphersuite list> TLS 1.3 proxy cipher suites
                            Append(ProxyTls13Ciphers());
                            break;

                        case "--proxy-tlsauthtype":         // <type> TLS authentication type for HTTPS proxy
                            Append(ProxyTlsauthtype());
                            break;

                        case "--proxy-tlspassword":         // <string> TLS password for HTTPS proxy
                            Append(ProxyTlspassword());
                            break;

                        case "--proxy-tlsuser":             // <name> TLS username for HTTPS proxy
                            Append(ProxyTlsuser());
                            break;

                        case "--proxy-tlsv1":               // Use TLSv1 for HTTPS proxy
                            Append(ProxyTlsv1());
                            break;

                        case "--proxy-user":
                        case "-U":                          // <user:password> Proxy user and password
                            Append(ProxyUser());
                            break;

                        case "--proxy1.0":                  // <host[:port]> Use HTTP/1.0 proxy on given port
                            Append(Proxy10());
                            break;

                        case "--proxytunnel":
                        case "-p":                          // Operate through an HTTP proxy tunnel (using CONNECT)
                            Append(Proxytunnel());
                            break;

                        case "--pubkey":                    // <key>  SSH Public key file name
                            Append(Pubkey());
                            break;

                        case "--quote":
                        case "-Q":                          // Send command(s) to server before transfer
                            Append(Quote());
                            break;

                        case "--random-file":               // <file> File for reading random data from
                            Append(RandomFile());
                            break;

                        case "--range":
                        case "-r":                          // <range> Retrieve only the bytes within RANGE
                            Append(Range());
                            break;

                        case "--raw":                       // Do HTTP "raw"; no transfer decoding
                            Append(Raw());
                            break;

                        case "--referer":
                        case "-e":
                            Append(Referer());
                            break;

                        case "--remote-header-name":
                        case "-J":                          // Use the header-provided filename
                            Append(RemoteHeaderName());
                            break;

                        case "--remote-name":
                        case "-O":                          // Write output to a file named as the remote file
                            Append(RemoteName());
                            break;

                        case "--remote-name-all":           // Use the remote file name for all URLs
                            Append(RemoteNameAll());
                            break;

                        case "--remote-time":
                        case "-R":                          // Set the remote file's time on the local output
                            Append(RemoteTime());
                            break;

                        case "--request-target":            // Specify the target for this request
                            Append(RequestTarget());
                            break;

                        case "--resolve":                   // <host:port:address[,address]...> Resolve the host+port to this address
                            Append(Resolve());
                            break;

                        case "--retry":                     // <num>   Retry request if transient problems occur
                            Append(Retry());
                            break;

                        case "--retry-connrefused":         // Retry on connection refused (use with --retry)
                            Append(RetryConnrefused());
                            break;

                        case "--retry-delay":               // <seconds> Wait time between retries
                            Append(RetryRelay());
                            break;

                        case "--retry-max-time":            // <seconds> Retry only within this period
                            Append(RetryMaxTime());
                            break;

                        case "--sasl-ir":                   // Enable initial response in SASL authentication
                            Append(SaslIr());
                            break;

                        case "--service-name":              // <name> SPNEGO service name
                            Append(ServiceName());
                            break;

                        case "--show-error":
                        case "-S":                          // Show error even when -s is used
                            Append(ShowError());
                            break;


                        case "--silent":
                        case "-s":                          // Silent mode
                            Append(Silent());
                            break;

                        case "--socks4":                    // <host[:port]> SOCKS4 proxy on given host + port
                            Append(Socks4());
                            break;

                        case "--socks4a":                   // <host[:port]> SOCKS4a proxy on given host + port
                            Append(Socks4a());
                            break;

                        case "--socks5":                    // <host[:port]> SOCKS5 proxy on given host + port
                            Append(Socks5());
                            break;

                        case "--socks5-basic":              // Enable username/password auth for SOCKS5 proxies
                            Append(Socks5Basic());
                            break;

                        case "--socks5-gssapi":             // Enable GSS-API auth for SOCKS5 proxies
                            Append(Socks5Gssapi());
                            break;

                        case "--socks5-gssapi-nec":         // Compatibility with NEC SOCKS5 server
                            Append(Socks5GssapiNec());
                            break;

                        case "--socks5-gssapi-service":     // <name> SOCKS5 proxy service name for GSS-API
                            Append(Socks5GssapiService());
                            break;

                        case "--socks5-hostname":           // <host[:port]> SOCKS5 proxy, pass host name to proxy
                            Append(Socks5Hostname());
                            break;

                        case "--speed-limit":
                        case "-Y":                          // <speed> Stop transfers slower than this
                            Append(SpeedLimit());

                            break;

                        case "--speed-time":
                        case "-y":                          // <seconds> Trigger 'speed-limit' abort after this time
                            Append(SpeedTime());
                            break;

                        case "--ssl":                       // Try SSL/TLS
                            Append(Ssl());
                            break;

                        case "--ssl-allow-beast":           // Allow security flaw to improve interop
                            Append(SslAllowBeast());
                            break;

                        case "--ssl-no-revoke":             // Disable cert revocation checks (Schannel)
                            Append(SslNoRevoke());
                            break;

                        case "--ssl-reqd":                  // Require SSL/TLS
                            Append(SslReqd());
                            break;

                        case "--sslv2":
                        case "-2":                          // Use SSLv2
                            Append(Sslv2());
                            break;

                        case "--sslv3":
                        case "-3":                          // Use SSLv3
                            Append(Sslv3());
                            break;

                        case "--stderr":                    // Where to redirect stderr
                            Append(Stderr());
                            break;

                        case "--styled-output":             // Enable styled output for HTTP headers
                            Append(StyledOutput());
                            break;

                        case "--suppress-connect-headers":         // Suppress proxy CONNECT response headers
                            Append(SuppressConnectHeaders());
                            break;

                        case "--tcp-fastopen":              // Use TCP Fast Open
                            Append(TcpFastopen());
                            break;

                        case "--tcp-nodelay":               // Use the TCP_NODELAY option
                            Append(TcpNodelay());
                            break;

                        case "--telnet-option":
                        case "-t":                          // <opt=val> Set telnet option
                            Append(TelnetOption());
                            break;

                        case "--tftp-blksize":              // <value> Set TFTP BLKSIZE option
                            Append(TftpBlksize());
                            break;

                        case "--tftp-no-options":           // Do not send any TFTP options
                            Append(TftpNoOptions());
                            break;

                        case "--time-cond":
                        case "-z":                          // <time> Transfer based on a time condition
                            Append(TimeCond());
                            break;

                        case "--tls-max":                   // <VERSION> Set maximum allowed TLS version
                            Append(TlsMax());
                            break;

                        case "--tls13-ciphers":             // <list of TLS 1.3 ciphersuites> TLS 1.3 cipher suites to use
                            Append(Tls13Ciphers());
                            break;

                        case "--tlsauthtype":               // <type> TLS authentication type
                            Append(Tlsauthtype());
                            break;

                        case "--tlspassword":               // TLS password
                            Append(Tlspassword());
                            break;

                        case "--tlsuser":                   // <name> TLS user name
                            Append(Tlsuser());
                            break;

                        case "--tlsv1":
                        case "-1":                          // Use TLSv1.0 or greater
                            Append(Tlsv1());
                            break;

                        case "--tlsv1.0":                   // Use TLSv1.0 or greater
                            Append(Tlsv10());
                            break;

                        case "--tlsv1.1":                   // Use TLSv1.1 or greater
                            Append(Tlsv11());
                            break;

                        case "--tlsv1.2":                   // Use TLSv1.2 or greater
                            Append(Tlsv12());
                            break;

                        case "--tlsv1.3":                   // Use TLSv1.3 or greater
                            Append(Tlsv13());
                            break;

                        case "--tr-encoding":               // Request compressed transfer encoding
                            Append(TrEncoding());
                            break;

                        case "--trace":                     // <file>  Write a debug trace to FILE
                            Append(Trace());
                            break;

                        case "--trace-ascii":               // <file> Like --trace, but without hex output
                            Append(TraceAscii());
                            break;

                        case "--trace-time":                // Add time stamps to trace/verbose output
                            Append(TraceTime());
                            break;

                        case "--unix-socket":               // <path> Connect through this Unix domain socket
                            Append(UnixSocket());
                            break;

                        case "--upload-file":
                        case "-T":                          // <file> Transfer local FILE to destination
                            Append(UploadFile());
                            break;

                        case "--url":                       // <url> URL to work with
                            Append(Url());
                            break;

                        case "--use-ascii":
                        case "-B":                          // Use ASCII/text transfer
                            Append(UseAscii());
                            break;

                        case "--user":
                        case "-u":                          // <user:password> Server user and password
                            Append(User());
                            break;

                        case "--user-agent":
                        case "-A":                          // <name> Send User-Agent <name> to server
                            Append(UserAgent());
                            break;

                        case "--verbose":
                        case "-v":                          // Make the operation more talkative
                            Append(Verbose());
                            break;

                        case "--version":
                        case "-V":                          // Show version number and quit
                            Append(Version());
                            break;

                        case "--write-out":
                        case "-w":                          // <format> Use output FORMAT after completion
                            Append(WriteOut());
                            break;

                        case "--xattr":                     // Store metadata in extended file attributes
                            Append(Xattr());
                            break;

                        case "--abstract-unix-socket":      // <path> Connect via abstract Unix domain socket
                            Append(AbstractUnixSocket());
                            break;

                        case "--alt-svc":                   // <file name> Enable alt-svc with this cache file
                            Append(AltSvc());
                            break;

                        case "--anyauth":                   // Pick any authentication method
                            Append(Anyauth());
                            break;

                        default:

                            break;

                    }

                }

                this._index++;

            }
        }

        private void Append(CurlInterpreterAction curlInterpreterAction)
        {
            _handlers.Add(curlInterpreterAction);
        }

        public static implicit operator CurlInterpreter(string self)
        {
            return self.Precompile();
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {
            System.Diagnostics.Debugger.Break();
        }


        private readonly List<CurlInterpreterAction> _handlers;
        private readonly string[] _arguments;

        private volatile object _lock = new object();
        private int _index;
        private string? _current;
        private bool _isUrl;
        private CurlInterpreterAction? _first;
        private readonly int _max;

    }





}
