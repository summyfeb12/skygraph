using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SkyGraphNG.Controllers
{
    public class LoginController : Controller
    {
        static string message = "";
        DBLayer d = new DBLayer();

        // GET: Login
        public ActionResult Index()
        {
            ViewBag.Message = message;
            message = "";
            if(Request.Cookies.Keys.Count != 0)
            {
                Response.Redirect("/");
            }
            return View();
        }

        public void LoginUser()
        {
            var keys = Request.Form.Keys;
            int x;
            d.VerifyLogin(Request.Form.Get(keys[0]), Request.Form.Get(keys[1]), "Login", out x);
            if(x < 0)
            {
                message = "Error: Please enter valid username and password";
                Response.Redirect("/Login");
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("BId", x.ToString()));
                Response.Cookies.Add(new HttpCookie("IsLoggedIn", "true"));
                Response.Cookies.Add(new HttpCookie("Email", Request.Form.Get(keys[0])));
                Response.Redirect("/");
            }
        }

        public void RegisterUser()
        {
            int x,y;
            var keys = Request.Form.Keys;

            d.SignUp(Request.Form.Get(keys[0]), Request.Form.Get(keys[1]), Request.Form.Get(keys[2]), Request.Form.Get(keys[3]), Request.Form.Get(keys[5]), Convert.ToInt32(Request.Form.Get(keys[6]).ToString()),out y);
            if(y>0)
            {
                d.VerifyLogin(Request.Form.Get(keys[2]), Request.Form.Get(keys[3]), "Login", out x);
                Response.Cookies.Add(new HttpCookie("BId", x.ToString()));
                Response.Cookies.Add(new HttpCookie("IsLoggedIn", "true"));
                Response.Redirect("/");
            }
            else
            {
                message = "Error: UserId already exists";
                Response.Redirect("/Login");
            }
            
        }

        public void LogoutUser()
        {
            //.Expires = DateTime.Now.AddDays(-1);  
            Response.Cookies["BId"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["IsLoggedIn"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["Email"].Expires = DateTime.Now.AddDays(-1);
            Response.Redirect("/");
        }
    }
}