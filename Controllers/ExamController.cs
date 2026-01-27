using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Web.Mvc.Ajax;

namespace ExamSystem.Controllers
{
    public class ExamController : Controller
    {
        public ActionResult CreateExam()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [HttpPost]
        public ActionResult CreateExam(string examName, int totalMarks, int duration)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("Insert into Exams values(@Ename, @Marks, @Duration, GetDate())", con);
            cmd.Parameters.AddWithValue("@Ename", examName);
            cmd.Parameters.AddWithValue("@Marks", totalMarks);
            cmd.Parameters.AddWithValue("@Duration", duration);
            cmd.ExecuteNonQuery();
            ViewBag.Msg = "Exam Created";
            return View();
        }
        public ActionResult ExamList()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
           
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open ();
            SqlCommand cmd = new SqlCommand("Select * from Exams", con);
            SqlDataReader dr = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Load(dr);
            return View(dt);
        }
        [HttpGet]
        public ActionResult AddQuestion(int id)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() !="Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.ExamId = id;
            return View();
        }
        [HttpPost]
        public ActionResult AddQuestion(
            int examId,
            string questionText,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open ();
            SqlCommand cmd = new SqlCommand("Insert into Questions values(@eid, @q, @a, @b, @c, @d, @co)", con);
            cmd.Parameters.AddWithValue("@eid", examId);
            cmd.Parameters.AddWithValue("@q", questionText);
            cmd.Parameters.AddWithValue("@a", optionA);
            cmd.Parameters.AddWithValue("@b", optionB);
            cmd.Parameters.AddWithValue("@c", optionC);
            cmd.Parameters.AddWithValue("@d", optionD);
            cmd.Parameters.AddWithValue("@co", correctOption);
            cmd.ExecuteNonQuery();

            ViewBag.Msg = "Question Added Successfully";
            ViewBag.ExamId = examId;
            return View();
        }
        [HttpGet]
        public ActionResult EditQuestion(int id)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("Select * from Questions Where QuestionId= @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            return View(dt.Rows[0]);
        }
        [HttpPost]
        public ActionResult EditQuestion(
            int QuestionId,
            int ExamId,
            string QuestionText,
            string OptionA,
            string OptionB,
            string OptionC,
            string OptionD,
            string CorrectOption)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("Update Questions set QuestionText=@q, OptionA=@a, OptionB=@b, OptionC=@c, OptionD=@d, CorrectOption=@co Where QuestionId=@id", con);
            cmd.Parameters.AddWithValue("@q", QuestionText);
            cmd.Parameters.AddWithValue("@a", OptionA);
            cmd.Parameters.AddWithValue("@b", OptionB);
            cmd.Parameters.AddWithValue("@c", OptionC);
            cmd.Parameters.AddWithValue("@d", OptionD);
            cmd.Parameters.AddWithValue("@co", CorrectOption);
            cmd.Parameters.AddWithValue("@id", QuestionId);
            cmd.ExecuteNonQuery();
            return RedirectToAction("QuestionList", new { id = ExamId });
        }
        public ActionResult DeleteQuestion(int id)
        {
            if (Session["UserRole"]==null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("Delete from Questions WHERE QuestionId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            return RedirectToAction("ExamList");
        }

        public ActionResult QuestionList(int id)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand("Select * from Questions Where ExamId=@eid", con);
            cmd.Parameters.AddWithValue("eid", id);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            ViewBag.ExamId = id;
            return View(dt);
        }

        public ActionResult StudentExamList()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Student")
            {
                return RedirectToAction("Login", "Account");
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("Select * from Exams", con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            return View(dt);

        }
        [HttpGet]
        public ActionResult StartExam(int id)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Student")
            {
                return RedirectToAction("Login", "Account");
            }
            Session["ExamId"] = id;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();
           //1. Get Questions
            SqlCommand cmd = new SqlCommand("Select * from Questions where ExamId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();

            //2. Get Exam Duration
            SqlCommand cmd2 = new SqlCommand("Select duration From Exams Where ExamId=@id", con);
            cmd2.Parameters.AddWithValue("@id", id);
            int duration = Convert.ToInt32(cmd2.ExecuteScalar());
            ViewBag.Duration = duration;
            return View(dt);
        }
        [HttpPost]
        public ActionResult SubmitExam(FormCollection form)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Student")
            {
                return RedirectToAction("Login", "Account");
            }
            int ExamId = Convert.ToInt32(Session["ExamId"]);
            string studentEmail = Session["UserEmail"].ToString();

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("SELECT QuestionId, CorrectOption FROM Questions WHERE ExamId=@eid", con);
            cmd.Parameters.AddWithValue("@eid", ExamId);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);

            int totalQuestions = dt.Rows.Count;
            int correctAnswers = 0;


            foreach(DataRow row in dt.Rows)
            {
                string qid = "q_" + row["QuestionId"].ToString();
                if (form[qid] != null)
                {
                    string studentAnswer = form[qid];
                    string correctAnswer = row["CorrectOption"].ToString();

                    if(studentAnswer == correctAnswer)
                    {
                        correctAnswers++;
                    }
                }
            }
            int marks = correctAnswers;

            SqlCommand saveCmd = new SqlCommand("Insert into Results values(@eid, @email, @tq, @ca, @m, GETDATE())", con);
            saveCmd.Parameters.AddWithValue("@eid", ExamId);
            saveCmd.Parameters.AddWithValue("@email", studentEmail);
            saveCmd.Parameters.AddWithValue("@tq", totalQuestions);
            saveCmd.Parameters.AddWithValue("@ca", correctAnswers);
            saveCmd.Parameters.AddWithValue("@m", marks);

            saveCmd.ExecuteNonQuery();

            ViewBag.Total = totalQuestions;
            ViewBag.Correct = correctAnswers;
            ViewBag.Marks = marks;
            return View("Result");
        }
        public ActionResult MyResults()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Student")
            {
                return RedirectToAction("Login", "Account");
            }
            string email = Session["UserEmail"].ToString();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("Select * from Results where StudentEmail=@email order by ResultDate Desc", con);
            cmd.Parameters.AddWithValue("@email", email);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            return View(dt);
        }
        public ActionResult AllResults()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("Select * from Results order by ResultDate Desc", con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            return View(dt);

        }
    }
}