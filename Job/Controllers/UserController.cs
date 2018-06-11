using Job.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Job.Controllers
{
    public class UserController : Controller
    {
        JobPortalEntities db;
        private static int saltLengthLimit = 32;
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(SignupVM signupUser)
        {
            if(ModelState.IsValid)
            {
                using (db = new JobPortalEntities())
                {
                    User user = new User();
                    user.Name = signupUser.Name;
                    user.UserName = signupUser.Username;
                    byte[] salt = Get_SALT();
                    user.SALT = salt;
                    user.HASH = Get_HASH_SHA512(signupUser.Password, signupUser.Username, salt);
                    db.Users.Add(user);
                    db.SaveChanges();
                    ModelState.Clear();
                    user = null;
                    ViewBag.Message = "Registration Successfull";
                        
                }
            }
            return View(signupUser);
        }

        [HttpGet]
        public ActionResult Login(string returnURL)
        {
            var userInfo = new LoginVM();

            try
            {
                EnsureLoggedOut();

                userInfo.ReturnURL = returnURL;

                return View(userInfo);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM loginUser)
        {
            string OldHASHValue = string.Empty;
            byte[] SALT = new byte[saltLengthLimit];

            try
            {
                using (db = new JobPortalEntities())
                {
                    // Ensure we have a valid viewModel to work with
                    if (!ModelState.IsValid)
                        return View(loginUser);

                    //Retrive Stored HASH Value From Database According To Username (one unique field)
                    var userInfo = db.Users.Where(s => s.UserName == loginUser.Username.Trim()).FirstOrDefault();

                    //Assign HASH Value
                    if (userInfo != null)
                    {
                        OldHASHValue = userInfo.HASH;
                        SALT = userInfo.SALT;
                    }

                    bool isLogin = CompareHashValue(loginUser.Password, loginUser.Username, OldHASHValue, SALT);

                    if (isLogin)
                    {
                        //Login Success
                        //For Set Authentication in Cookie (Remeber ME Option)
                        //SignInRemember(entity.Username, entity.isRemember);

                        //Set A Unique ID in session
                        Session["UserID"] = userInfo.UserID;
                        Session["UserName"] = userInfo.UserName;
                        Session["Fullname"] = userInfo.Name;

                        // If we got this far, something failed, redisplay form
                        // return RedirectToAction("Index", "Dashboard");
                        return RedirectToLocal(loginUser.ReturnURL);
                    }
                    else
                    {
                        //Login Fail
                        TempData["ErrorMSG"] = "Access Denied! Wrong Credential";
                        return View(loginUser);
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        public static bool CompareHashValue(string password, string username, string OldHASHValue, byte[] SALT)
        {
            try
            {
                string expectedHashString = Get_HASH_SHA512(password, username, SALT);

                return (OldHASHValue == expectedHashString);
            }
            catch
            {
                return false;
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            try
            {
                FormsAuthentication.SignOut();

                HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
                Session.Clear();Session.RemoveAll();
                return RedirectToLocal();
            }
            catch
            {
                throw;
            }
        }

        //GET: RedirectToLocal

        private ActionResult RedirectToLocal(string returnURL = "")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(returnURL) && Url.IsLocalUrl(returnURL))
                    return Redirect(returnURL);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                throw;
            }
        }


        private void EnsureLoggedOut()
        {
            if (Request.IsAuthenticated)
                Logout();
        }

        #region --> Generate SALT Key

        private static byte[] Get_SALT()
        {
            return Get_SALT(saltLengthLimit);
        }

        private static byte[] Get_SALT(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];

            //Require NameSpace: using System.Security.Cryptography;
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }

        #endregion



        //CODE: Generate HASH Using SHA512
        public static string Get_HASH_SHA512(string password, string username, byte[] salt)
        {
            try
            {
                //required NameSpace: using System.Text;
                //Plain Text in Byte
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(password + username);

                //Plain Text + SALT Key in Byte
                byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + salt.Length];

                for (int i = 0; i < plainTextBytes.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainTextBytes[i];
                }

                for (int i = 0; i < salt.Length; i++)
                {
                    plainTextWithSaltBytes[plainTextBytes.Length + i] = salt[i];
                }

                HashAlgorithm hash = new SHA512Managed();
                byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
                byte[] hashWithSaltBytes = new byte[hashBytes.Length + salt.Length];

                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hashWithSaltBytes[i] = hashBytes[i];
                }

                for (int i = 0; i < salt.Length; i++)
                {
                    hashWithSaltBytes[hashBytes.Length + i] = salt[i];
                }

                return Convert.ToBase64String(hashWithSaltBytes);
            }
            catch
            {
                return string.Empty;
            }
        }



    }
}