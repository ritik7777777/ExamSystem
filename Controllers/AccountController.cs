using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
//for Password Hashing
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
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
            
            return View();
        }
        [HttpPost]
        public ActionResult Register(string email, string password, string requestedRole)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.EmailError = "Email required";
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.PasswordError = "Password required";
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(requestedRole))
            {
                ViewBag.RoleError = "Select a role";
                hasError = true;
            }

            if (hasError)
                return View();
            string hashedPassword = GetHash(password);

            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                @"INSERT INTO Users (Email, Password, RequestedRole, IsApproved)
          VALUES (@email,@pass,@reqRole,0)", con);

            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pass", hashedPassword);
            cmd.Parameters.AddWithValue("@reqRole", requestedRole);

            cmd.ExecuteNonQuery();

            ViewBag.Msg = "Registration successful. Waiting for approval.";
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
            string hashedPassword = GetHash(password);
            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
                        SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Users WHERE Email=@email AND Password=@pass", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pass", hashedPassword);
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                if (Convert.ToBoolean(dr["IsApproved"]) == false)
                {
                    ViewBag.Error = "Your account is pending approval.";
                    return View();
                }
                Session["UserEmail"] = dr["Email"].ToString();
                Session["UserRole"] = dr["Role"].ToString();

                if (Session["UserRole"].ToString() == "Admin")
                    return RedirectToAction("AdminDashboard");

                if (Session["UserRole"].ToString() == "SuperAdmin")
                    return RedirectToAction("SuperAdminDashboard");

                return RedirectToAction("StudentDashboard");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        //Pending Students
        public ActionResult PendingStudents()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM Users WHERE RequestedRole='Student' AND IsApproved=0", con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            return View(dt);
        }
        [HttpPost]
        public ActionResult ApproveStudent(int? id)
        {
            if(Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                "UPDATE Users SET Role='Student', IsApproved=1 WHERE Id=@id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("PendingStudents");
        }

        [HttpPost]
        public ActionResult RejectStudent(int id)
        {
            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Users WHERE Id=@id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("PendingStudents");
        }
        public ActionResult SuperAdminDashboard()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "SuperAdmin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        public ActionResult PendingAdmins()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "SuperAdmin")
                return RedirectToAction("Login", "Account");

            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM Users WHERE RequestedRole='Admin' AND IsApproved=0", con);

            DataTable dt = new DataTable();
            da.Fill(dt);

            return View(dt);
        }
        [HttpPost]
        public ActionResult ApproveAdmin(int id)
        {
            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                "UPDATE Users SET Role='Admin', IsApproved=1 WHERE Id=@id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("PendingAdmins");
        }

        [HttpPost]
        public ActionResult RejectAdmin(int id)
        {
            SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Users WHERE Id=@id", con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("PendingAdmins");
        }


        public ActionResult Dashboard()
        {
            if (Session["UserEmail"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
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