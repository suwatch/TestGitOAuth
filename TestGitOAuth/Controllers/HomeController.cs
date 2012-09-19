using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestGitOAuth.Controllers
{
    public class HomeController : Controller
    {
        // http://localhost
        const string ClientIdentifier = "fc824e457b8706cbde69";
        const string ClientSecret = "fca5f897f1a32a482c3c645cc5ba4351b7f9bdf2";

        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult RequestToken()
        {
            string state = DateTime.Now.ToString();
            return new RedirectResult("https://github.com/login/oauth/authorize" + "?client_id=" + ClientIdentifier + "&scope=repo,user&state=" + state);
        }

        public JsonResult GetAccessToken()
        {
            return Json(OAuthTokenData.GetTokenCache(HttpContext.Request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
