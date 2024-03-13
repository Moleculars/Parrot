using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

#pragma warning disable CS0162
#pragma warning disable CS1591
#pragma warning disable CS8618
namespace Bb.ParrotServices
{

    /// <summary>
    /// model for return http error
    /// </summary>
    public class HttpExceptionModel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpExceptionModel"/> class.
        /// </summary>
        public HttpExceptionModel()
        {
        }

        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        /// <value>
        /// The trace identifier.
        /// </value>
        public string TraceIdentifier { get; set; }

        /// <summary>
        /// Gets the origin application of the exception
        /// </summary>
        /// <value>
        /// The origin.
        /// </value>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; } = "Sorry, an error has occurred. Please contact our customer service with TraceIdentifier for assistance.";

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public string Session { get; set; }

    }

}
