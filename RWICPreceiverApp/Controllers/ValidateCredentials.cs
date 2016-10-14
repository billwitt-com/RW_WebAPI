using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using RWICPreceiverApp.Models; 

namespace RWICPreceiverApp.Controllers
{
    public class ValidateCredentials
    {
        public bool checkCreds(HttpRequestMessage request)
        {
            string decodedCredentials = "";
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            if (authorization == null)
            {
                return false; 
            }

            if (authorization.Scheme != "Basic")
            {
                return false;
            }

            if (String.IsNullOrEmpty(authorization.Parameter))
            {
                return false;
            }

            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorization.Parameter);
            }
            catch (FormatException)
            {
                return false;
            }

            Encoding encoding = Encoding.ASCII;
            // Make a writable copy of the encoding to enable setting a decoder fallback.
            encoding = (Encoding)encoding.Clone();
            // Fail on invalid bytes rather than silently replacing and continuing.
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return false;
            }

            if (String.IsNullOrEmpty(decodedCredentials))
            {
                return false; ;
            }

            int colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
            {
                return false;
            }

            string userName = decodedCredentials.Substring(0, colonIndex);
            string password = decodedCredentials.Substring(colonIndex + 1);

            // I think this is all we need to do here
            // XXXX move these values to webapiconfig
            if (userName != "Bill")
                return false;

            if (password != "Password1")
                return false;

            return true; // success
        }
    }
}