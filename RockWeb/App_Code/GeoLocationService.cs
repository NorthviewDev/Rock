using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace RockWeb
{
    [Serializable]
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T ResponseObject { get; set; }
    }

    [Serializable]
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public interface IGeoLocationService
    {
        ServiceResult<double> Distance(double lat1, double lon1, double lat2, double lon2, DistanceUnitTypes unit);
        ServiceResult<double> Distance(GeoLocation loc1, GeoLocation loc2, DistanceUnitTypes unit);
        ServiceResult<GeoLocation> GetGeoLocationForAddress(string address);
        ServiceResult<Dictionary<string, double>> GetDrivingDistancesToCampuses(string address);
    }

    public class GeoLocationService : IGeoLocationService
    {
        public Dictionary<string, string> CampusCoordinates = new Dictionary<string, string>()
        {
            {"Anderson","40.094852, -85.658019"},
            {"Binford","39.877908, -86.075492"},
            {"Carmel","39.9773602,-86.0766334"},
            {"Fishers","39.9873849,-85.9013684"},
            {"Flora","40.5546383,-86.5131691"},
            {"Greater Lafayette","40.444812, -86.954872"},
            {"Kokomo","40.4760512,-86.1049138"},            
            {"North Put","39.7892971,-86.8032038"},
            {"Peru","40.7457737,-86.082033"},
            {"Westfield","40.0565221,-86.1815024"}
        };

        //C# Version of Haversine algorithm - see https://dotnetfiddle.net/d8pgsp
        static readonly double _eQuatorialEarthRadius = 6378.1370D;
        static readonly double _d2r = (Math.PI / 180D);

        protected IWebCaller _webCaller;

        public GeoLocationService(IWebCaller webCaller)
        {
            _webCaller = webCaller;
            this.CampusCoordinates = new Dictionary<string, string>()
        {
            {"Anderson","40.094852, -85.658019"},
            {"Binford","39.877908, -86.075492"},
            {"Carmel","39.9773602,-86.0766334"},
            {"Fishers","39.9873849,-85.9013684"},
            {"Flora","40.5546383,-86.5131691"},
            {"Greater Lafayette","40.444812, -86.954872"},
            {"Kokomo","40.4760512,-86.1049138"},
            {"North Put","39.7892971,-86.8032038"},
            {"Peru","40.7457737,-86.082033"},
            {"Westfield","40.0565221,-86.1815024"}
        };
        }

        public ServiceResult<double> Distance(GeoLocation loc1, GeoLocation loc2, DistanceUnitTypes unit)
        {
            return Distance(loc1.Latitude, loc1.Longitude, loc2.Latitude, loc2.Longitude, unit);
        }

        public ServiceResult<double> Distance(double lat1, double long1, double lat2, double long2, DistanceUnitTypes unit)
        {
            var resp = new ServiceResult<double>();

            try
            {
                double dlong = (long2 - long1) * _d2r;
                double dlat = (lat2 - lat1) * _d2r;
                double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r) * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
                double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
                double d = _eQuatorialEarthRadius * c;

                double finalDist = 0;

                switch (unit)
                {
                    case DistanceUnitTypes.Miles:
                        finalDist = d * 0.621371;
                        break;
                    case DistanceUnitTypes.Kilometers:
                    default:
                        finalDist = d;
                        break;
                }

                resp.Success = true;
                resp.ResponseObject = finalDist;
            }
            catch (Exception e)
            {
                resp.Message = String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
            }

            return resp;
        }

        public ServiceResult<GeoLocation> GetGeoLocationForAddress(string address)
        {
            var resp = new ServiceResult<GeoLocation>();

            var geoLoc = new GeoLocation()
            {
                Id = Guid.NewGuid().ToString()
            };

            try
            {  

                var key = ConfigurationManager.AppSettings["BingGeoLocationKey"];
                var url = ConfigurationManager.AppSettings["BingGeoLocationUrl"];
                var data = String.Format("?q={0}&key={1}", address,key);

                var formattedDataStr = Uri.EscapeUriString(data);

                var bingResp = _webCaller.GetAs<BingMapsLocationResponse>(url, formattedDataStr);

                if (bingResp.statusCode == HttpStatusCode.OK && bingResp.authenticationResultCode == "ValidCredentials")
                {
                    if(bingResp.resourceSets.Any())
                    {
                        var resourceSet = bingResp.resourceSets.First();

                        if (resourceSet.resources.Any())
                        {
                            var resource = resourceSet.resources.First();

                            if(resource.geocodePoints.Any())
                            {
                                BingMapsGeocodePoint geoCodePoint = null;

                                if (resource.geocodePoints.Any(x => x.usageTypes.Any(u => u == "Route"))) //These are the more accurate type of point
                                {
                                    geoCodePoint = resource.geocodePoints.First(x => x.usageTypes.Contains("Route"));
                                }
                                else
                                {
                                    geoCodePoint = resource.geocodePoints.First();
                                }

                                geoLoc.Latitude = geoCodePoint.coordinates[0];
                                geoLoc.Longitude = geoCodePoint.coordinates[1];

                                resp.Success = true;
                                resp.ResponseObject = geoLoc;
                            }
                            else
                            {
                                resp.Message = "Resource contained no geo code points! ";
                            }
                        }
                        else
                        {
                            resp.Message = "ResourceSet contained no resources! ";

                        }
                    }
                    else
                    {
                        resp.Message = "Response contained no ResourceSets! ";
                    }
                }
                else
                {
                    resp.Message = String.Format("Response was invalid! HttpStatus: {0} Authentication: {1}", bingResp.statusCode, bingResp.authenticationResultCode);
                }
                

            }
            catch(Exception e)
            {
                resp.Message = String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
            }


            return resp;
        }

        public ServiceResult<Dictionary<string, double>> GetDrivingDistancesToCampuses(string address)
        {
            var resp = new ServiceResult<Dictionary<string, double>>();

            var key = "";
            var url = "";

            try
            {

                var addrToLatLongResp = this.GetGeoLocationForAddress(address);

                if(!addrToLatLongResp.Success)
                {
                    resp.Message = addrToLatLongResp.Message;
                    return resp;
                }

                var geoCoords = String.Format("{0},{1}", addrToLatLongResp.ResponseObject.Latitude, addrToLatLongResp.ResponseObject.Longitude);

                try
                {
                    key = ConfigurationManager.AppSettings["BingGeoLocationKey"];
                    url = ConfigurationManager.AppSettings["BingDistanceMatrixUrl"];
                }
                catch (Exception e)
                {
                    resp.Message = "Error retrieving key and/or url from app settings";
                    resp.Message += String.Format("Key: {0} /r/n URL: {1}",key, url);
                    resp.Message += String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
                    return resp;
                }

                var campusCoords = String.Join(";", this.CampusCoordinates.Values);

                var data = String.Format("?origins={0}&destinations={1}&travelMode=driving&key={2}", campusCoords, geoCoords, key);

                var formattedDataStr = Uri.EscapeUriString(data);

                var bingResp = _webCaller.GetAs<BingMapsDistanceMatrixResponse>(url, formattedDataStr);

                if (bingResp.statusCode == HttpStatusCode.OK && bingResp.authenticationResultCode == "ValidCredentials")
                {
                    if (bingResp.resourceSets.Any())
                    {
                        var resourceSet = bingResp.resourceSets.First();

                        if (resourceSet.resources.Any())
                        {
                            var resource = resourceSet.resources.First();

                            if (resource.results.Any())
                            {
                                Dictionary<string, double> distanceResults = new Dictionary<string, double>();

                                var names = this.CampusCoordinates.Keys.ToList();

                                foreach (var result in resource.results)
                                {
                                    var campusName = names[result.originIndex];

                                    distanceResults.Add(campusName, result.travelDistance);
                                }

                                resp.Success = true;
                                resp.ResponseObject = distanceResults;
                            }
                            else
                            {
                                resp.Message = "Resource contained no distances! ";
                            }
                        }
                        else
                        {
                            resp.Message = "ResourceSet contained no resources! ";

                        }
                    }
                    else
                    {
                        resp.Message = "Response contained no ResourceSets! ";
                    }
                }
                else
                {
                    resp.Message = String.Format("Response was invalid! HttpStatus: {0} Authentication: {1}", bingResp.statusCode, bingResp.authenticationResultCode);
                }
            }
            catch(Exception e)
            {
                resp.Message = String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);

                while(e.InnerException != null)
                {
                    e = e.InnerException;

                    resp.Message += String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
                }
            }

            return resp;
        }


    }

    [Serializable]
    public enum LocationTypes
    {
        None = -1,
        Campus,
        Project,
        Personal,
        Other
    }

    [Serializable]
    public enum DistanceUnitTypes
    {
        Miles,
        Kilometers
    }


    [Serializable]
    public class GeoLocation : IEquatable<GeoLocation>
    {
        public string Id        { get; set; }
        public string Address   { get; set; }
        public string Zip       { get; set; }
        public string State     { get; set; }
        public double Latitude  { get; set; }
        public double Longitude { get; set; }

        public LocationTypes LocationType { get; set; }

        public bool Equals(GeoLocation other)
        {
            return (Latitude == other.Latitude && Longitude == other.Longitude);
        }
    }

    [Serializable]
    public class BingMapsLocationResponse
    {
        public string authenticationResultCode { get; set; }
        public HttpStatusCode statusCode { get; set; }
        public List<BingMapsLocationResourceSet> resourceSets { get; set; }

        public BingMapsLocationResponse()
        {
            resourceSets = new List<BingMapsLocationResourceSet>();
        }
    }

    [Serializable]
    public class BingMapsGeocodePoint
    {
        public string type { get; set; }
        public string calculationMethod { get; set; }
        public List<string> usageTypes { get; set; }
        public List<double> coordinates { get; set; }

        public BingMapsGeocodePoint()
        {
            usageTypes = new List<string>();
            coordinates = new List<double>();
        }
    }

    [Serializable]
    public class BingMapsLocationResource
    {
        public string name { get; set; }
        public List<BingMapsGeocodePoint> geocodePoints { get; set; }

        public BingMapsLocationResource()
        {
            geocodePoints = new List<BingMapsGeocodePoint>();
        }
    }

    [Serializable]
    public class BingMapsLocationResourceSet
    {
        public int estimatedTotal { get; set; }
        public List<BingMapsLocationResource> resources { get; set; }

        public BingMapsLocationResourceSet()
        {
            resources = new List<BingMapsLocationResource>();
        }
    }

    [Serializable]
    public class BingMapsDistanceMatrixResponse
    {
        public string authenticationResultCode { get; set; }
        public HttpStatusCode statusCode { get; set; }
        public List<BingMapsDistanceMatrixResourceSet> resourceSets { get; set; }

        public BingMapsDistanceMatrixResponse()
        {
            resourceSets = new List<BingMapsDistanceMatrixResourceSet>();
        }
    }

    [Serializable]
    public class BingMapsDistanceMatrixResult
    {
        public int destinationIndex { get; set; }
        public int originIndex { get; set; }
        public double travelDistance { get; set; }
        public double travelDuration { get; set; }
    }

    [Serializable]
    public class BingMapsDistanceMatrixResource
    {
        public List<BingMapsDistanceMatrixResult> results { get; set; }

        public BingMapsDistanceMatrixResource()
        {
            results = new List<BingMapsDistanceMatrixResult>();
        }
    }

    [Serializable]
    public class BingMapsDistanceMatrixResourceSet
    {
        public int estimatedTotal { get; set; }
        public List<BingMapsDistanceMatrixResource> resources { get; set; }

        public BingMapsDistanceMatrixResourceSet()
        {
            resources = new List<BingMapsDistanceMatrixResource>();
        }
    }

}