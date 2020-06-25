//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: WebRequestExtensions.cs
//
//--------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>Extension methods for working with WebRequest asynchronously.</summary>
    public static class WebRequestExtensions
    {
#if NETFRAMEWORK
        /// <summary>Creates a <see cref="Task"/> that represents an asynchronous request to <see cref="WebRequest.GetResponse"/>.</summary>
        /// <param name="webRequest">The <see cref="WebRequest"/>.</param>
        /// <returns>A <see cref="Task"/> containing the retrieved <see cref="WebResponse"/>.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException(nameof(webRequest));
            return Task<WebResponse>.Factory.FromAsync(
                webRequest.BeginGetResponse, webRequest.EndGetResponse, webRequest /* object state for debugging */);
        }

        /// <summary>Creates a <see cref="Task"/> that represents an asynchronous request to <see cref="WebRequest.GetRequestStream"/>.</summary>
        /// <param name="webRequest">The <see cref="WebRequest"/>.</param>
        /// <returns>A <see cref="Task"/> containing the retrieved <see cref="Stream"/>.</returns>
        public static Task<Stream> GetRequestStreamAsync(this WebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException(nameof(webRequest));
            return Task<Stream>.Factory.FromAsync(
                webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream, webRequest /* object state for debugging */);
        }
#endif

        /// <summary>Creates a <see cref="Task"/> that respresents downloading all of the data from a <see cref="WebRequest"/>.</summary>
        /// <param name="webRequest">The <see cref="WebRequest"/>.</param>
        /// <returns>A <see cref="Task"/> containing the downloaded content.</returns>
        public static Task<byte[]> DownloadDataAsync(this WebRequest webRequest)
        {
            // Asynchronously get the response.  When that's done, asynchronously read the contents.
            return webRequest.GetResponseAsync().ContinueWith(response =>
            {
                return response.Result.GetResponseStream().ReadAllBytesAsync();
            }).Unwrap();
        }
    }
}
