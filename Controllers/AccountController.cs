// Collaboration Testing

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
//for Password Hashing
using System.Security.Cryptography;
using System.Text;
//using System.Configuration;

namespace ExamSystem.Controllers
{
    public class AccountController : Controller
    {
        //for Password Hashing
        private string GetHash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();

                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        //DATABASE USED: Examm
        //TABLE: Users
        [HttpGet]
        public ActionResult Register()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Register(string email, string password, string role)
        {
            //HASH THE PASSWORD HERE
            string hashedPassword = GetHash(password);

            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Users VALUES (@email, @pass, @role)", con);

            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pass", hashedPassword); // ✅ HASHED
            cmd.Parameters.AddWithValue("@role", role);

            cmd.ExecuteNonQuery();

            ViewBag.Msg = "Registration Completed";
            return View();
        }




        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string email, string password) 
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            string hashedPassword = GetHash(password);

            SqlCommand cmd = new SqlCommand("Select * from users where Email=@email And Password=@pass", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pass", hashedPassword);
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                Session["UserEmail"] = dr["Email"].ToString();
                Session["UserRole"] = dr["Role"].ToString();

                //redirect based on role
                if (Session["UserRole"].ToString() == "Admin")
                {
                    return RedirectToAction("AdminDashboard");
                }

                else return RedirectToAction("StudentDashboard");
  
            }
            ViewBag.Error = "Invalid Email or password";
            return View();
                
        }
        public ActionResult Dashboard()
        {
            if (Session["UserEmail"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult AdminDashboard()
        {
            if (Session["UserEmail"]== null|| Session["UserEmail"].ToString() == "Admin")
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        public ActionResult StudentDashboard()
        {
            if (Session["UserEmail"] == null || Session["UserEmail"].ToString() == "Student")
            {
                return RedirectToAction("Login");
            }
            return View();
        }
    }
}