using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RestSharp;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using System.Net.Mail;
using Google.Apis.Auth.OAuth2.Requests;


namespace RSSInfraTI
{
    public partial class Default : System.Web.UI.Page
    {

	//Convertendo formato Atom para Rss para padrão utilizado na aplicação.
        public string AtomToRssConverter(XmlDocument atomDoc)
        {
            XmlDocument xmlDoc = atomDoc;
            XmlNamespaceManager mgr = new XmlNamespaceManager(xmlDoc.NameTable);
            mgr.AddNamespace("atom", "http://purl.org/atom/ns#");
            const string rssVersion = "2.0";
            const string rssLanguage = "en-US";
            string rssGenerator = "RDFFeedConverter";
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(memoryStream, null);
            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            string feedTitle = xmlDoc.SelectSingleNode("//atom:title", mgr).InnerText;
            string feedLink = xmlDoc.SelectSingleNode("//atom:link/@href", mgr).InnerText;
            string rssDescription = xmlDoc.SelectSingleNode
                ("//atom:tagline", mgr).InnerText;
            xmlWriter.WriteStartElement("rss");
            xmlWriter.WriteAttributeString("version", rssVersion);
            xmlWriter.WriteStartElement("channel");
            xmlWriter.WriteElementString("title", feedTitle);
            xmlWriter.WriteElementString("link", feedLink);
            xmlWriter.WriteElementString("description", rssDescription);
            xmlWriter.WriteElementString("language", rssLanguage);
            xmlWriter.WriteElementString("generator", rssGenerator);
            XmlNodeList items = xmlDoc.SelectNodes("//atom:entry", mgr);
            if (items == null)
                throw new FormatException("Atom feed is not in expected format. ");
            else
            {
                string title = String.Empty;
                string link = String.Empty;
                string description = String.Empty;
                string author = String.Empty;
                string pubDate = String.Empty;
                for (int i = 0; i < items.Count; i++)
                {
                    XmlNode nodTitle = items[i];
                    title = nodTitle.SelectSingleNode("atom:title", mgr).InnerText;
                    link = items[i].SelectSingleNode("atom:link[@rel='alternate']",
                    mgr).Attributes["href"].InnerText;
                    description = items[i].SelectSingleNode("atom:summary",
                            mgr).InnerText;
                    author = items[i].SelectSingleNode("//atom:name", mgr).InnerText;
                    pubDate = items[i].SelectSingleNode("atom:issued", mgr).InnerText;
                    xmlWriter.WriteStartElement("item");
                    xmlWriter.WriteElementString("title", title);
                    xmlWriter.WriteElementString("link", link);
                    xmlWriter.WriteElementString("pubDate",
            Convert.ToDateTime(pubDate).ToUniversalTime().ToString
            (@"ddd, dd MMM yyyy HH:mm:ss G\MT"));
                    xmlWriter.WriteElementString("author", author);
                    xmlWriter.WriteElementString("description", description);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
                xmlWriter.Close();
            }
            XmlDocument retDoc = new XmlDocument();
            string outStr = Encoding.UTF8.GetString(memoryStream.ToArray());
            memoryStream.Close();
            xmlWriter.Close();
            return outStr;
        }


        public static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var reader = xDocument.CreateReader())
            {
                xmlDocument.Load(reader);
            }

            var xDeclaration = xDocument.Declaration;
            if (xDeclaration != null)
            {
                var xmlDeclaration = xmlDocument.CreateXmlDeclaration(
                    xDeclaration.Version,
                    xDeclaration.Encoding,
                    xDeclaration.Standalone);

                xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.FirstChild);
            }

            return xmlDocument;
        }

	//Validando Xml
        private bool ValidXML(string xml)
        {

            string pattern = "<?xml";
            Match match = Regex.Match(xml, pattern);
            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

	//Verificar disponibilidade do acesso ao rss do gmail
        private bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PopulateRssFeed();
        }
        private int CompareDates(RSS x, RSS y)
        {
            return y.PublishDate.CompareTo(x.PublishDate);
        }

        private DateTime ParseDate(string date)
        {
            DateTime result;
            if (DateTime.TryParse(date, out result))
                return result;
            else
                return DateTime.MinValue;
        }
        private const string UserId = "user-id";
    


	//fazendo override GoogleAuthorizationCodeRequestUrl
        internal class ForceOfflineGoogleAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
        {
            public ForceOfflineGoogleAuthorizationCodeFlow(GoogleAuthorizationCodeFlow.Initializer initializer) : base(initializer) { }

            public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
            {
                var ss = new Google.Apis.Auth.OAuth2.Requests.GoogleAuthorizationCodeRequestUrl(new Uri(AuthorizationServerUrl));
                ss.AccessType = "offline";
                ss.ApprovalPrompt = "force";
                ss.ClientId = ClientSecrets.ClientId;
                ss.Scope = string.Join(" ", Scopes);
                ss.RedirectUri = redirectUri;
                return ss;
            }
        };


        private void PopulateRssFeed()
        {
            string client_id = "client id - google";
            string client_secret = "client secret - google";
            string scope = "https://mail.google.com/mail/feed/atom/";
                 

            GoogleAuthorizationCodeFlow flow;
            flow = new ForceOfflineGoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = client_id,
                        ClientSecret = client_secret
                    },

                    Scopes = new[] { scope },
                    DataStore = new FileDataStore(@"Auth.Store", true)
                });

            //var flow = new CustomAuthorizationCodeFlow(initializer);
            var uri = Request.Url.ToString();
            var code = Request["code"];
            if (code != null)
            {

                var token = flow.ExchangeCodeForTokenAsync("user", code,
                    uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

                // Extract the right state.
                var oauthState = AuthWebUtility.ExtracRedirectFromState(
                    flow.DataStore, UserId, Request["state"]).Result;
                Response.Redirect(oauthState);
            }
            else
            {
                var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync("user",
                            CancellationToken.None).Result;

                if (result.RedirectUri != null)
                {                  
                    // Redirect the user to the authorization server.
                    Response.Redirect(result.RedirectUri);
                }
                else
                {
                        //Verificando disponibilidade com a internet
                        if (RemoteFileExists("remote server"))
                        {
                            string token = result.Credential.GetAccessTokenForRequestAsync().Result;
                            var client = new RestClient("https://mail.google.com/mail/feed/atom/{label}");
                           var request = new RestRequest(Method.GET);
                            request.RequestFormat = DataFormat.Json;
                            request.AddHeader("Content-Type", "application/json");
                            request.AddHeader("cache-control", "no-cache");
                            request.AddHeader("authorization", "Bearer " + token);
                            IRestResponse response = client.Execute(request);
                            response.Content = response.Content;

                        string[] RssFeedUrls = new string[] { 
			    //RSS local gerado caso houver indisponibilidade na internet ou entrar com informações manuais.
                            "https://localrss.manual/rssinfrati/local.xml"
                    };

                        List<RSS> feeds = new List<RSS>();

                      
                            //RSS automatico
                            foreach (string RssFeedUrl in RssFeedUrls)
                            {

                                if (RemoteFileExists(RssFeedUrl))
                                {
                                    XDocument xDoc = new XDocument();

                                    if (RssFeedUrl ==  "https://localrss.manual/rssinfrati/local.xml")
)
                                    {

                                        response.Content = Regex.Replace(response.Content, @"""", @"'");
                                        response.Content = System.Net.WebUtility.HtmlDecode(response.Content);
                                        response.Content = Regex.Replace(response.Content, @"&", @"");

                                        if (ValidXML(response.Content))
                                        {
                                            var xmlDoc = new XmlDocument();
                                            xmlDoc.LoadXml(response.Content);
                                            string xDoc2 = AtomToRssConverter(xmlDoc);
                                            xDoc = XDocument.Parse(xDoc2);
                                        }
                                    }
                                    else
                                    {
                                        xDoc = XDocument.Load(RssFeedUrl);
                                    }

                                    var items = (from x in xDoc.Descendants("item")
                                                 select new
                                                 {
                                                     title = x.Element("title").Value,
                                                     link = x.Element("link").Value,
                                                     pubDate = ParseDate(x.Element("pubDate").Value),
                                                     description = x.Element("description").Value
                                                 });
                                    if (items != null)
                                    {
                                        foreach (var i in items)
                                        {
                                            RSS f = new RSS
                                            {
                                                Title = i.title,
                                                Link = i.link,
                                                PublishDate = i.pubDate,
                                                Description = i.description
                                            };
                                            feeds.Add(f);
                                        }
                                    }


                                }
                            }
                            feeds.Sort(CompareDates);
                            gvRss.DataSource = feeds;
                            gvRss.DataBind();

                       
                    }
                        else
                        {

                            string token = null;
                            var client = new RestClient("");
                            var request = new RestRequest(Method.GET);
                            request.RequestFormat = DataFormat.Json;
                            request.AddHeader("Content-Type", "application/json");
                            request.AddHeader("cache-control", "no-cache");
                            request.AddHeader("authorization", "Bearer " + token);
                            IRestResponse response = client.Execute(request);
                            response.Content = response.Content;

                        string[] RssFeedUrls = new string[] {                           
                             "https://localrss.manual/rssinfrati/local.xml"

                    };

                        List<RSS> feeds = new List<RSS>();

                     
                            //RSS automatico
                            foreach (string RssFeedUrl in RssFeedUrls)
                            {

                                if (RemoteFileExists(RssFeedUrl))
                                {
                                    XDocument xDoc = new XDocument();

                                    if (RssFeedUrl == "https://localrss.manual/rssinfrati/local.xml")
                                    {

                                        response.Content = Regex.Replace(response.Content, @"""", @"'");
                                        response.Content = System.Net.WebUtility.HtmlDecode(response.Content);
                                        response.Content = Regex.Replace(response.Content, @"&", @"");

                                        if (ValidXML(response.Content))
                                        {
                                            var xmlDoc = new XmlDocument();
                                            xmlDoc.LoadXml(response.Content);
                                            string xDoc2 = AtomToRssConverter(xmlDoc);
                                            xDoc = XDocument.Parse(xDoc2);
                                        }
                                    }
                                    else
                                    {
                                        xDoc = XDocument.Load(RssFeedUrl);
                                    }

                                    var items = (from x in xDoc.Descendants("item")
                                                 select new
                                                 {
                                                     title = x.Element("title").Value,
                                                     link = x.Element("link").Value,
                                                     pubDate = ParseDate(x.Element("pubDate").Value),
                                                     description = x.Element("description").Value
                                                 });
                                    if (items != null)
                                    {
                                        foreach (var i in items)
                                        {
                                            RSS f = new RSS
                                            {
                                                Title = i.title,
                                                Link = i.link,
                                                PublishDate = i.pubDate,
                                                Description = i.description
                                            };
                                            feeds.Add(f);
                                        }
                                    }


                                }
                            }
                            feeds.Sort(CompareDates);
                            gvRss.DataSource = feeds;
                            gvRss.DataBind();

                        }

                       
                    }

                 
                    

                }
            }
        }
    }

    internal class GoogleScope
    {   
    }

