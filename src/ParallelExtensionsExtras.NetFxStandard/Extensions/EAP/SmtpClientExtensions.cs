//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: SmtpClientExtensions.cs
//
//--------------------------------------------------------------------------

using System.Net.Mail;
using System.Threading.Tasks;

#if NET40
namespace System.Net.NetworkInformation
{
    /// <summary>Extension methods for working with <see cref="SmtpClient"/> asynchronously.</summary>
    public static class SmtpClientExtensions
    {
        /// <summary>Sends an e-mail message asynchronously.</summary>
        /// <param name="smtpClient">The <see cref="SmtpClient"/>.</param>
        /// <param name="message">A <see cref="MailMessage"/> that contains the message to send.</param>
        /// <param name="userToken">A user-defined object stored in the resulting <see cref="Task"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous send.</returns>
        public static Task SendTask(this SmtpClient smtpClient, MailMessage message, object userToken)
        {
            return SendTaskCore(smtpClient, userToken, tcs => smtpClient.SendAsync(message, tcs));
        }

        /// <summary>Sends an e-mail message asynchronously.</summary>
        /// <param name="smtpClient">The <see cref="SmtpClient"/>.</param>
        /// <param name="from">A <see cref="string"/> that contains the address information of the message sender.</param>
        /// <param name="recipients">A <see cref="string"/> that contains the address that the message is sent to.</param>
        /// <param name="subject">A <see cref="string"/> that contains the subject line for the message.</param>
        /// <param name="body">A <see cref="string"/> that contains the message body.</param>
        /// <param name="userToken">A user-defined object stored in the resulting <see cref="Task"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous send.</returns>
        public static Task SendTask(this SmtpClient smtpClient, string from, string recipients, string subject, string body, object userToken)
        {
            return SendTaskCore(smtpClient, userToken, tcs => smtpClient.SendAsync(from, recipients, subject, body, tcs));
        }

        /// <summary>The core implementation of SendTask.</summary>
        /// <param name="smtpClient">The <see cref="SmtpClient"/>.</param>
        /// <param name="userToken">The user-supplied state.</param>
        /// <param name="sendAsync">
        /// A delegate that initiates the asynchronous send.
        /// The provided TaskCompletionSource must be passed as the user-supplied state to the actual SmtpClient.SendAsync method.
        /// </param>
        /// <returns></returns>
        private static Task SendTaskCore(SmtpClient smtpClient, object userToken, Action<TaskCompletionSource<object>> sendAsync)
        {
            // Validate we're being used with a real smtpClient.  The rest of the arg validation
            // will happen in the call to sendAsync.
            if (smtpClient == null) throw new ArgumentNullException(nameof(smtpClient));

            // Create a TaskCompletionSource to represent the operation
             var tcs = new TaskCompletionSource<object>(userToken);

            // Register a handler that will transfer completion results to the TCS Task
            SendCompletedEventHandler handler = null!;
            handler = (sender, e) => EAPCommon.HandleCompletion(tcs, e, () => null!, () => smtpClient.SendCompleted -= handler);
            smtpClient.SendCompleted += handler;

            // Try to start the async operation.  If starting it fails (due to parameter validation)
            // unregister the handler before allowing the exception to propagate.
            try
            {
                sendAsync(tcs);
            }
            catch(Exception exc)
            {
                smtpClient.SendCompleted -= handler;
                tcs.TrySetException(exc);
            }

            // Return the task to represent the asynchronous operation
            return tcs.Task;
        }
    }
}
#endif
