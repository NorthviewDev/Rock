using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace RockWeb
{
    public interface IWebCaller
    {
        string GetAsString(string urlHost, string urlData, bool allowRedirect = false);
        T GetAs<T>(string urlHost, string urlData, bool allowRedirect = false);
        string GetXmlResponse(System.Net.HttpWebRequest webRequest);
    }

    public class WebCaller : IWebCaller
    {
        public string GetAsString(string urlHost, string urlData, bool allowRedirect = false)
        {
            if (!string.IsNullOrEmpty(urlData) && urlData.IndexOf("?") < 0)
            {
                urlData = "?" + urlData;
            }

            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(urlHost + urlData);
            webRequest.UseDefaultCredentials = true;
            webRequest.Proxy = new WebProxy();
            webRequest.AllowAutoRedirect = allowRedirect;

            string xml = GetXmlResponse(webRequest);


            return xml;

        }

        public T GetAs<T>(string urlHost, string urlData, bool allowRedirect = false)
        {
            if (!string.IsNullOrEmpty(urlData) && urlData.IndexOf("?") < 0)
            {
                urlData = "?" + urlData;
            }

            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(urlHost + urlData);
            webRequest.UseDefaultCredentials = true;
            webRequest.Proxy = new WebProxy();
            webRequest.AllowAutoRedirect = allowRedirect;

            string xml = GetXmlResponse(webRequest);

            var returnObject = default(T);

            returnObject = JsonConvert.DeserializeObject<T>(xml);

            return returnObject;

        }

        public string GetXmlResponse(System.Net.HttpWebRequest webRequest)
        {
            string xml = string.Empty;

            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    using (var stream = new System.IO.StreamReader(webResponse.GetResponseStream()))
                    {
                        xml = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(WebException)))
                {
                    using (var stream = new System.IO.StreamReader(((WebException)ex).Response.GetResponseStream()))
                    {
                    }
                }

                xml = string.Empty;
            }

            return xml;
        }
    }

    public class TestWebCaller : IWebCaller
    {
        public string GetAsString(string urlHost, string urlData, bool allowRedirect = false)
        {
            if (!string.IsNullOrEmpty(urlData) && urlData.IndexOf("?") < 0)
            {
                urlData = "?" + urlData;
            }

            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(urlHost + urlData);
            webRequest.UseDefaultCredentials = true;
            webRequest.Proxy = new WebProxy();
            webRequest.AllowAutoRedirect = allowRedirect;

            string xml = GetXmlResponse(webRequest);


            return xml;

        }

        public T GetAs<T>(string urlHost, string urlData, bool allowRedirect = false)
        {
            if (!string.IsNullOrEmpty(urlData) && urlData.IndexOf("?") < 0)
            {
                urlData = "?" + urlData;
            }

            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(urlHost + urlData);
            webRequest.UseDefaultCredentials = true;
            webRequest.Proxy = new WebProxy();
            webRequest.AllowAutoRedirect = allowRedirect;

            string xml = GetXmlResponse(webRequest);

            var returnObject = default(T);

            returnObject = JsonConvert.DeserializeObject<T>(xml);

            return returnObject;

        }

        public string GetXmlResponse(System.Net.HttpWebRequest webRequest)
        {
            var xml = @"{""authenticationResultCode"":""ValidCredentials"",""brandLogoUri"":""http:\/\/dev.virtualearth.net\/Branding\/logo_powered_by.png"",""copyright"":""Copyright © 2019 Microsoft and its suppliers. All rights reserved. This API cannot be accessed and the content and any results may not be used, reproduced or transmitted in any manner without express written permission from Microsoft Corporation."",""resourceSets"":[{""estimatedTotal"":1,""resources"":[{""__type"":""DistanceMatrix:http:\/\/schemas.microsoft.com\/search\/local\/ws\/rest\/v1"",""destinations"":[{""latitude"":45.5347,""longitude"":-122.6231}],""errorMessage"":""Request accepted."",""origins"":[{""latitude"":47.6044,""longitude"":-122.3345},{""latitude"":47.6731,""longitude"":-122.1185},{""latitude"":47.6149,""longitude"":-122.1936}],""results"":[{""destinationIndex"":0,""originIndex"":0,""totalWalkDuration"":0,""travelDistance"":5.546,""travelDuration"":165.723},{""destinationIndex"":0,""originIndex"":1,""totalWalkDuration"":0,""travelDistance"":12.488,""travelDuration"":175.545},{""destinationIndex"":0,""originIndex"":2,""totalWalkDuration"":0,""travelDistance"":30.19,""travelDuration"":167.583}]}]}],""statusCode"":200,""statusDescription"":""OK"",""traceId"":""6a6bf480bd28441fabc8370dae3a4fa7|BN00002091|7.7.0.0""}";

            return xml;
        }
    }
}