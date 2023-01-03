using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace WpfApp_Squash.HttpClient
{
    public enum httpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    class RestClient
    {
        public string endPoint { get; set; }
        public httpVerb httpMethod { get; set; }

        private string baseUrl { get; set; }

        public RestClient(string baseUrl)
        {
            endPoint = string.Empty;
            httpMethod = httpVerb.GET;
            this.baseUrl = baseUrl;

        }
        //baseurl + endpoint
        public string makeRequest(string endpoint1)
        {
            string strResponseValue = string.Empty;            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl + endpoint1);
            string username = "GENERIC_ReadOnly";
            string password = "Recettes2e2019";
            string authtype = "Basic";
            request.Method = httpMethod.ToString();
            String authHeader = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(username + ":" +password));
            request.Headers.Add("Authorization", authtype.ToString() + " " + authHeader);
            HttpWebResponse response = null;
            try

            {
                response = (HttpWebResponse)request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            strResponseValue = reader.ReadToEnd();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strResponseValue = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";
            }
            finally
            {
                if (response != null)
                {
                    ((IDisposable)response).Dispose();
                }
            }

            return strResponseValue;
        }

    }
}
