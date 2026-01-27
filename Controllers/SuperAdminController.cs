using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExamSystem.Controllers
{
    public class SuperAdminController : Controller
    {
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
            {
                return RedirectToAction("Login", "Account");
            }

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
                "UPDATE Users SET Role='Admin', IsApproved=1 WHERE UserId=@id", con);
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
                "DELETE FROM Users WHERE UserId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction("PendingAdmins");
        }
    }

}