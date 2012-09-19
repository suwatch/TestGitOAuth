using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace TestGitOAuth.Controllers
{
    public class WebSitesController : Controller
    {
        // http://localhost/WebSites/TokenAuthorize
        //const string ClientIdentifier = "fc824e457b8706cbde69";
        //const string ClientSecret = "fca5f897f1a32a482c3c645cc5ba4351b7f9bdf2";

        // http://testgitoauth.kudu2.antares-test.windows-int.net/WebSites/TokenAuthorize
        const string ClientIdentifier = "7b3d629aaf0876d4a90a";
        const string ClientSecret = "328ac1297d923d6aa6e01d4b80316fa0c14d1f78";


        public ActionResult RequestToken()
        {
            //return new RedirectResult("https://github.com/login/oauth/authorize" + "?client_id=" + ClientIdentifier + "&scope=repo,user&state=state");
            return new RedirectResult("https://github.com/login/oauth/authorize" + "?client_id=" + ClientIdentifier + "&scope=public_repo&state=state");
        }

        public ActionResult TokenAuthorize(string code, string state)
        {
            if (!String.IsNullOrEmpty(code))
            {
                StringBuilder strb = new StringBuilder();
                strb.AppendFormat("client_id={0}", ClientIdentifier);
                strb.AppendFormat("&client_secret={0}", ClientSecret);
                strb.AppendFormat("&code={0}", code);
                strb.AppendFormat("&state={0}", state);

                string postData = strb.ToString();

                HttpWebRequest request =
                   (HttpWebRequest)WebRequest.Create("https://github.com/login/oauth/access_token");
                request.Method = "POST";
                request.ContentLength = postData.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "application/json";
                request.Headers.Add(GetAuthorizationHeader());

                StreamWriter writer = new
                StreamWriter(request.GetRequestStream());
                writer.Write(postData);
                writer.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseData = reader.ReadToEnd();
                    reader.Close();

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    OAuthTokenData token = serializer.Deserialize<OAuthTokenData>(responseData);
                    OAuthTokenData.SetTokenCache(HttpContext.Request, HttpContext.Response, token.access_token);
                }
            }

            return RedirectToAction("Index", "home");
        }

        private String GetAuthorizationHeader()
        {
            return "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(ClientIdentifier + ":" + ClientSecret));
        }

        public JsonResult GetRepos()
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendFormat("access_token={0}", OAuthTokenData.GetTokenCache(HttpContext.Request));
            strb.AppendFormat("&type={0}", "public");
            strb.AppendFormat("&sort={0}", "updated");

            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create("https://api.github.com/user/repos?" + strb);

            request.Method = "GET";
            request.Accept = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseData = reader.ReadToEnd();
                reader.Close();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                GitHubRepoFullInfo[] repos = serializer.Deserialize<GitHubRepoFullInfo[]>(responseData);
                return Json(repos, JsonRequestBehavior.AllowGet);
            }

            throw new Exception("fail to list repo statusCode=" + response.StatusCode);
        }

        public JsonResult GetHooks(string url)
        {
            StringBuilder strb = new StringBuilder();
            strb.AppendFormat("access_token={0}", OAuthTokenData.GetTokenCache(HttpContext.Request));

            HttpWebRequest request =
               (HttpWebRequest)WebRequest.Create(url + "/hooks?" + strb);
            request.Method = "GET";
            request.Accept = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseData = reader.ReadToEnd();
                reader.Close();
                return Json(responseData, JsonRequestBehavior.AllowGet);
            }

            throw new Exception("fail to list hook statusCode=" + response.StatusCode);
        }
    }

    public class GitHubRepoFullInfo
    {
        public string url { get; set; }

        public string html_url { get; set; }

        public string clone_url { get; set; }

        public string git_url { get; set; }

        public string ssh_url { get; set; }

        public string svn_url { get; set; }

        public string mirror_url { get; set; }

        public string id { get; set; }

        //public GitHubRepoOwnerInfo owner { get; set; }

        public string name { get; set; }

        public string full_name { get; set; }

        public string description { get; set; }

        public string homepage { get; set; }

        public string language { get; set; }

        public string @private { get; set; }

        public string fork { get; set; }

        public string forks { get; set; }

        public string watchers { get; set; }

        public string size { get; set; }

        public string master_branch { get; set; }

        public string open_issues { get; set; }

        public string pushed_at { get; set; }

        public string created_at { get; set; }

        public string updated_at { get; set; }
    }

    public class OAuthTokenData
    {
        const string OAuthCookieName = "TestGitOAuth";

        public String access_token { get; set; }

        public String token_type { get; set; }

        public String expires_in { get; set; }

        public String refresh_token { get; set; }

        public static void SetTokenCache(HttpRequestBase request, HttpResponseBase response, string accessToken)
        {
            HttpCookie cookie = request.Cookies.Get(OAuthCookieName) ?? new HttpCookie(OAuthCookieName);
            //cookie.Secure = true;
            //cookie.HttpOnly = true;
            cookie.Values["AccessToken"] = accessToken;
            response.Cookies.Set(cookie);
        }

        public static string GetTokenCache(HttpRequestBase request)
        {
            HttpCookie cookie = request.Cookies.Get(OAuthCookieName);
            if (cookie == null)
            {
                return string.Empty;
            }

            return cookie.Values["AccessToken"];
        }
    }
}
