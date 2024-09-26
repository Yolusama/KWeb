using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace KWeb.HttpOption.RequestHandle
{
    public class ResourceLocator
    {
        public string Pattern { get; set; }
        public string[] Locations { get; set; }

        private string? ResourceLocate(string path)
        {
            string? res = null;
            for (int i = 0; i < Locations.Length; i++)
            {
                DirectoryInfo directory = new DirectoryInfo(Locations[i]);
                string filePath = $"{directory.FullName}{path}";
                if (File.Exists(filePath))
                      return filePath;
            }
            return res;
        }
        public byte[] Locate(string path)
        {
            string? actualPath = ResourceLocate(path);
            if (actualPath == null)
                return null;
            return File.ReadAllBytes(actualPath);
        }
    }
    public class ResourceHandler
    {
        public List<ResourceLocator> Locators { get; } = new List<ResourceLocator>();
        private ResourceLocator?  tempLocator = null;
        
        public bool RouteMatched(string route)
        {
            foreach (ResourceLocator locator in Locators)
            {
                Regex regex = new Regex($"^{locator.Pattern}/*");
                if (regex.IsMatch(route))
                {
                    tempLocator = locator;
                    return true;
                }
            }
            return false;
        }
        public byte[] Locate(string route,HttpResponse response)
        {
            route = HttpUtility.UrlDecode(route);
            string filePath = route.Substring(
                route.IndexOf(tempLocator.Pattern)+tempLocator.Pattern.Length).Replace('/','\\');
            byte[] res = tempLocator.Locate(filePath);
            if(res == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                response.Headers["Content-Type"] = GetContentType(filePath);
                response.StatusCode = HttpStatusCode.OK;
            }
            tempLocator = null;
            return res;
        }

        private string GetContentType(string fileName)
        {
            if (!fileName.Contains('.'))
                return "application/octet-stream";
            string suffix = fileName.Substring(fileName.LastIndexOf('.')+1).ToLowerInvariant();
            if (suffix == "jpg" || suffix == "jpeg")
                return "image/jpeg";
            else if (suffix == "png")
                return "image/png";
            else if (suffix == "tiff")
                return "image/tiff";
            else if (suffix == "ico")
                return "image/x-icon";
            else if (suffix == "bmp")
                return "image/bmp";
            else if (suffix == "txt" || suffix == "log")
                return "text/plain";
            else if (suffix == "html" || suffix == "htm")
                return "text/html";
            else if (suffix == "css")
                return "text/css";
            else if (suffix == "js")
                return "application/javascript";
            else if (suffix == "gif")
                return "image/gif";
            else if (suffix == "xml")
                return "application/xml";
            else if (suffix == "mpeg")
                return "audio/mpeg";
            else if (suffix == "mp4")
                return "video/mp4";
            else if (suffix == "mp3")
                return "audio/mp3";
            else if (suffix == "ogg")
                return "audio/ogg";
            else if (suffix == "webm")
                return "video/webm";
            else if (suffix == "webp")
                return "image/webp";
            else if (suffix == "pdf")
                return "application/pdf";
            else
                return "application/octet-stream";
        }
    }

    public class ResourceHandlerBuilder
    {
        private ResourceHandler handler = new ResourceHandler();
        public ResourceHandlerBuilder AddPatterns(params string[] patterns)
        {
            IEnumerable<ResourceLocator> locators = handler.Locators.Where(l=>l.Locations!=null);
            if (locators.Count() == 0)
            {
                foreach (string pattern in patterns)
                {
                    handler.Locators.Add(new ResourceLocator { Pattern = pattern });
                }
            }
            else
            {
                ResourceLocator locator = handler.Locators.Last();
                for(int i = 0;i< patterns.Length;i++)
                {
                    if(i==0)
                        locator.Pattern = patterns[i];
                    else
                        handler.Locators.Add(new ResourceLocator
                        { 
                            Pattern = patterns[i],
                            Locations = locator.Locations
                        });
                }

            }
            return this;
        }

        public ResourceHandlerBuilder AddLocations(params string[] locations)
        {
            IEnumerable<ResourceLocator> locators = handler.Locators.Where(l=>l.Pattern!=null);
            if(locators.Count() == 0)
            {
                handler.Locators.Add(new ResourceLocator { Locations = locations });
            }
            else
            {
                foreach(ResourceLocator locator in locators)
                {
                    locator.Locations = locations;
                }
            }
            return this;
        }

        public ResourceHandler Build()
        {
            return handler;
        }
    }
}
