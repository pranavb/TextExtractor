using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace PicToTextRT
{
    /// <summary>
    /// Base class representing a simplified response from a service call 
    /// </summary>
    public abstract class StandardResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Per call identifier that can be logged to diagnose issues with Microsoft support
        /// </summary>
        public string CorrelationId { get; set; }
    }

    /// <summary>
    /// Class representing standard error from the service
    /// </summary>
    public class StandardErrorResponse : StandardResponse
    {
        /// <summary>
        /// Error message - intended for developer, not end user
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StandardErrorResponse()
        {
            this.StatusCode = HttpStatusCode.InternalServerError;
        }
    }

    /// <summary>
    /// Class representing a successful create call from the service
    /// </summary>
    public class CreateSuccessResponse : StandardResponse
    {
        /// <summary>
        /// URL to launch OneNote rich client
        /// </summary>
        public string OneNoteClientUrl { get; set; }

        /// <summary>
        /// URL to launch OneNote web experience
        /// </summary>
        public string OneNoteWebUrl { get; set; }
    }

}
