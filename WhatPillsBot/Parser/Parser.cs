using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace SiteParser
{
    public class Parser
    {
        public static string ParseSiteAsString(string url)
        {
            var pageContent = new WebClient().DownloadData(url);
            return Encoding.UTF8.GetString(pageContent);
        }

        public static string SendPostRequest(string dataToSend, string url, string referer)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(dataToSend);

            if (request != null)
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Referer = referer;
                request.ContentLength = data.Length;

                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            return null;
        }

        public static HtmlNode LoadHtmlRootElement(string siteContent)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(siteContent);
            return html.DocumentNode;
        }

        public static HtmlNode GetElementById(HtmlNode node, string id)
        {
            return node.Descendants().Where(x => (x.Attributes["id"] != null && x.Attributes["id"].Value == id)).FirstOrDefault();
        }

        public static IEnumerable<string> GetElementValueByTag(HtmlNode node, string className)
        {
            return node.Descendants().Where(x => x.Name == className).Select(x => x.InnerText);
        }

        public static IEnumerable<HtmlNode> GetElementByClass(HtmlNode node, string className)
        {
            return node.Descendants().Where(x => (x.Attributes["class"] != null && x.Attributes["class"].Value == className));
        }

        public static string GetFirstElementValueByClass(HtmlNode node, string className)
        {
            var value = node.Descendants().Where(x => (x.Attributes["class"] != null && x.Attributes["class"].Value == className)).FirstOrDefault();
            return value == null ? string.Empty : value.InnerText;
        }

        public static string GetLastElementValueByClass(HtmlNode node, string className)
        {
            var value = node.Descendants().Where(x => (x.Attributes["class"] != null && x.Attributes["class"].Value == className)).LastOrDefault();
            return value == null ? string.Empty : value.InnerText;
        }

        public static string GetElementValueByClass(HtmlNode node, string className)
        {
            return node.Descendants().Where(x => (x.Attributes["class"] != null && x.Attributes["class"].Value == className))
                .Select(x => x.InnerText).Aggregate("", (x, y) => x += y);
        }

    }
}