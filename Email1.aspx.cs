using System.Collections.Generic;
using System;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;

public partial class Email : System.Web.UI.Page
{
    private SiteUser CurrentUser;

    public class EmailRecipient
    {
        public int PatrollerID;
        public string FirstName;
        public string LastName;
        public string Email;
        public int MinDays;
        public List<DateTime> Assignments;
    }

    private List<EmailRecipient> EmailRecipientsForAllPatrollers()
    {
        List<EmailRecipient> recps = new List<EmailRecipient>();
        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd  = con.CreateCommand();

        cmd.CommandText = "select PatrollerID, FirstName, LastName, Email, MinDays from Patroller where siteid = " + CurrentUser.SiteID;
        OleDbDataReader reader = cmd.ExecuteReader();
      
        while (reader.Read())
        {
            int pId = (Int32)reader["PatrollerID"];
            string firstName = reader["firstname"].ToString();
            string lastName = reader["lastname"].ToString();
            string email = reader["email"].ToString();
            int minDays = (int)reader["MinDays"];

            EmailRecipient er = new EmailRecipient();
            er.FirstName = firstName;
            er.LastName = lastName;
            er.Email = email;
            er.Assignments = new List<DateTime>();
            er.PatrollerID = pId;
            er.MinDays = minDays;
            recps.Add(er);
        }        
        reader.Close();
        return recps;
    }

    private List<EmailRecipient> EmailRecipientsAdministrators()
    {
        List<EmailRecipient> recps = new List<EmailRecipient>();
        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "select PatrollerID, FirstName, LastName, Email, MinDays from Patroller where IsAdmin=TRUE and siteid = " + CurrentUser.SiteID;
        OleDbDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            int pId = (Int32)reader["PatrollerID"];
            string firstName = reader["firstname"].ToString();
            string lastName = reader["lastname"].ToString();
            string email = reader["email"].ToString();
            int minDays = (int)reader["MinDays"];

            EmailRecipient er = new EmailRecipient();
            er.FirstName = firstName;
            er.LastName = lastName;
            er.Email = email;
            er.Assignments = new List<DateTime>();
            er.PatrollerID = pId;
            er.MinDays = minDays;
            recps.Add(er);
        }
        reader.Close();
        return recps;
    }

    private void UpdateEmailRecipientListFromDateRange(List<EmailRecipient>recps, DateTime startDate, DateTime endDate)
    {

        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "select Patroller.PatrollerID, FirstName, LastName, Email, MinDays, ActiveDay from Assignments, Days, Patroller where " +
                  "patroller.patrollerid=assignments.patrollerid and assignments.dayid=days.dayid and patroller.siteid = " + CurrentUser.SiteID;
        OleDbDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            int pId = (Int32)reader["PatrollerID"];
            string firstName = reader["firstname"].ToString();
            string lastName = reader["lastname"].ToString();
            string email = reader["email"].ToString();
            DateTime date = ((DateTime)reader["activeday"]);
            int minDays = (int)reader["MinDays"];

            if (date >= startDate && date <= endDate)
            {
                bool found = false;
                for (int i = 0; i < recps.Count; i++)
                {
                    if (recps[i].PatrollerID == pId)
                    {
                        recps[i].Assignments.Add(date);
                        found = true;
                    }
                }
                if (!found)
                {
                    EmailRecipient er = new EmailRecipient();
                    er.FirstName = firstName;
                    er.LastName = lastName;
                    er.Email = email;
                    er.Assignments = new List<DateTime>();
                    er.Assignments.Add(date);
                    er.PatrollerID = pId;
                    er.MinDays = minDays;
                    recps.Add(er);
                }
            }
        }
    }

    protected void MonthlyEmail()
    {
        List<EmailRecipient> recps = EmailRecipientsForAllPatrollers();
        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        DateTime endDate = startDate.AddMonths(1);
        UpdateEmailRecipientListFromDateRange(recps, startDate, endDate);        

        MailMessage msg = new MailMessage();
        msg.From = new MailAddress("webmaster@skipatrolschedule.com", Baldy.SiteName);        
        msg.IsBodyHtml = false;
        msg.Priority = MailPriority.High;
        //SmtpClient smtp = new SmtpClient("smtp-server.socal.rr.com");
        SmtpClient smtp = new SmtpClient("smtpout.secureserver.net");
        smtp.Credentials = new NetworkCredential("webmaster@skipatrolschedule.com", "Honey123456"); 
        
        foreach (EmailRecipient er in recps)
        {
            if (er.Email == String.Empty)
                continue;
            else
            {
                msg.To.Clear();
                msg.To.Add(new MailAddress(er.Email, er.FirstName + " " + er.LastName));
            }

            string Content =
                "Hello " + er.FirstName + " " + er.LastName + ",\n" +
                "\nYou are currently scheduled for " + er.Assignments.Count + " days this month.\n\n";
            foreach (DateTime dt in er.Assignments)
            {
                Content += dt.ToLongDateString() + "\n";
            }

            if (er.Assignments.Count == 1)
            {
                Content +=
                    "\nIf you can not make it to this scheduled day please login to www.skipatrolschedule.com " +
                    "and make your changes.\n";
            }
            else if (er.Assignments.Count > 1)
            {
                Content +=
                    "\nIf you can not make it to these scheduled days please login to www.skipatrolschedule.com " +
                    "and make your changes.\n";
            }

            if (er.Assignments.Count < er.MinDays)
            {
                Content += "You are currently below your commitment of " + er.MinDays.ToString() +
                    ". You must signup for additional days or days will be assigned for you automatically.\n";
            }

            Content += "\n\nThanks,\nMt. Baldy Ski Patrol\n\n\nThis is an automated message from www.skipatrolschedule.com";
            if (er.Assignments.Count < er.MinDays)
                msg.Subject = "This month's schedule for " + er.FirstName + " " + er.LastName + ": Signup for more days!!";
            else
                msg.Subject = "This month's schedule for " + er.FirstName + " " + er.LastName;
            msg.Body = Content;

            smtp.Send(msg);
        }
        
        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "update site set LastMonthlyEmail = " + Baldy.AccessStringFromDateTime(DateTime.Now) + " WHERE siteid = " + CurrentUser.SiteID;
        cmd.ExecuteNonQuery();
    }

    protected void WeeklyEmail()
    {
        List<EmailRecipient> recps = new List<EmailRecipient>();
        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        DateTime endDate = startDate.AddDays(7); 
        UpdateEmailRecipientListFromDateRange(recps, startDate, endDate);

        MailMessage msg = new MailMessage();
        msg.From = new MailAddress("webmaster@skipatrolschedule.com", Baldy.SiteName);        
        msg.IsBodyHtml = false;
        msg.Priority = MailPriority.High;
        //SmtpClient smtp = new SmtpClient("smtp-server.socal.rr.com");
        SmtpClient smtp = new SmtpClient("smtpout.secureserver.net");
        smtp.Credentials = new NetworkCredential("webmaster@skipatrolschedule.com", "Honey123456"); 

        foreach (EmailRecipient er in recps)
        {
            if (er.Email == String.Empty)
                continue;
            else
            {
                msg.To.Clear();
                msg.To.Add(new MailAddress(er.Email, er.FirstName + " " + er.LastName));
            }

            string Content =
                "Hello " + er.FirstName + " " + er.LastName + ",\n" +
                "\nYou are currently scheduled for " + er.Assignments.Count + " day(s) this week.\n";
            
            foreach (DateTime dt in er.Assignments)
            {
                Content += "\n" + dt.ToLongDateString() + "\n\n";
                ActiveDay ad = CurrentUser.ActiveDayFromDate(dt);
                foreach (PatrollerPerson pp in ad.Patrollers)
                { 
                    Content += pp.FullName + "\n";
                }
            }            

            Content += "\n\nThanks,\nMt. Baldy Ski Patrol\n\n\nThis is an automated message from www.skipatrolschedule.com";           
            
            msg.Subject = "This week's schedule for " + er.FirstName + " " + er.LastName;
            msg.Body = Content;

            smtp.Send(msg);
        }

        List<ActiveDay> activeDaysBelowMin = new List<ActiveDay>();
        List<ActiveDay> activeDaysWithNoLead = new List<ActiveDay>();

        foreach (ActiveDay ad in CurrentUser.SiteActiveDays)
        {
            if (ad.Date >= startDate && ad.Date < endDate)
            {
                if (ad.LeadID <= 0)
                {
                    activeDaysWithNoLead.Add(ad);
                }

                if (ad.NonCandidateCount < ad.MinPatrollers)
                {
                    activeDaysBelowMin.Add(ad);
                }
            }
        }

        List<EmailRecipient> admins = EmailRecipientsAdministrators();

        if (activeDaysWithNoLead.Count > 0)
        {
            foreach (EmailRecipient er in admins)
            {
                if (er.Email == String.Empty)
                    continue;
                else
                {
                    msg.To.Clear();
                    msg.To.Add(new MailAddress(er.Email, er.FirstName + " " + er.LastName));
                }

                string Content =
                    "Hello " + er.FirstName + " " + er.LastName + ",\n" +
                    "\nThere are no leads assigned for " + activeDaysWithNoLead.Count + " day(s) this week.\n";

                foreach (ActiveDay ad in activeDaysWithNoLead)
                {
                    Content += "\n" + ad.Date.ToLongDateString() + "\n\n";
                }

                Content += "\n\nThanks,\nMt. Baldy Ski Patrol\n\n\nThis is an automated message from www.skipatrolschedule.com";

                msg.Subject = "No Leads this week!!";
                msg.Body = Content;

                smtp.Send(msg);
            }
        }

        List<EmailRecipient> allPatrollers = EmailRecipientsForAllPatrollers();

        if (activeDaysBelowMin.Count > 0)
        {
            foreach (EmailRecipient er in allPatrollers)
            {
                if (er.Email == String.Empty)
                    continue;
                else
                {
                    msg.To.Clear();
                    msg.To.Add(new MailAddress(er.Email, er.FirstName + " " + er.LastName));
                }

                string Content =
                    "Hello " + er.FirstName + " " + er.LastName + ",\n" +
                    "\nMore patrolers are needed on the following days this week:\n";

                foreach (ActiveDay ad in activeDaysWithNoLead)
                {
                    Content += "\n" + ad.Date.ToLongDateString() + "\n\n";
                }

                Content += "\n\nThanks,\nMt. Baldy Ski Patrol\n\n\nThis is an automated message from www.skipatrolschedule.com";

                msg.Subject = "Patrollers needed this week!!";
                msg.Body = Content;

                smtp.Send(msg);
            }
        }

        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "update site set LastWeeklyEmail = " + Baldy.AccessStringFromDateTime(DateTime.Now) + " WHERE siteid = " + CurrentUser.SiteID;
        cmd.ExecuteNonQuery();
    }

    public string ContentString;
    public string NavigatorSection;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] != null)
        {
            CurrentUser = (SiteUser)Session["User"];
            if (!CurrentUser.IsAdministrator)
                Response.Redirect("Default.aspx");
        }
        else
        {
            Response.Redirect("UserError.aspx");
            return;
        }

        NavigatorSection += @"<a href=""Administrator.aspx"">Schedule Manager</a><br>";
        NavigatorSection += @"<a href=""AdminPatrollers.aspx"">Patroller Manager</a><br>";
        NavigatorSection += @"<a href=""Email.aspx"">Email</a><br>";
        NavigatorSection += @"<a href=""Patroller.aspx"">Exit</a><br>";

        Baldy.UpdateUser(CurrentUser);

        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "select * from site where siteid = " + CurrentUser.SiteID;
        OleDbDataReader reader = cmd.ExecuteReader();

        DateTime lastMonthlyDate = DateTime.MinValue;
        DateTime lastWeeklyDate = DateTime.MinValue;

        string dayOfWeekToSend = String.Empty;

        while (reader.Read())
        {
            if (!reader.IsDBNull(2))
                lastMonthlyDate = (DateTime)reader["LastMonthlyEmail"];
            if (!reader.IsDBNull(3))
                lastWeeklyDate = (DateTime)reader["LastWeeklyEmail"];

            if (!reader.IsDBNull(4))
                dayOfWeekToSend = (string)reader["DayOfWeek"];
            else
                dayOfWeekToSend = String.Empty;
        }

        reader.Close();

        if (DateTime.Now.AddDays(-3) > lastWeeklyDate && DateTime.Now.DayOfWeek.ToString() == dayOfWeekToSend)
        {
            WeeklyEmail();
            ContentString += "<b>Weekly email sent!</b><br>";
        }
        else
        {
            if (lastWeeklyDate == DateTime.MinValue)
                ContentString += "First weekly email has not been sent yet.<br>";
            else
                ContentString += "Weekly email was last sent on " + lastWeeklyDate.ToLongDateString() + "<br>";
        }

        if (dayOfWeekToSend != String.Empty)
            ContentString += "Weekly emails are sent on " + dayOfWeekToSend + "s.<br>&nbsp<br>";
        else
            ContentString += "Weekly emails are not sent for this site.<br>&nbsp<br>";

        if (DateTime.Now.Day == 1 && (DateTime.Now.AddDays(-27) > lastMonthlyDate))
        {
            MonthlyEmail();
            ContentString += "<b>Monthly email sent!</b><br>";
        }
        else
        {
            if (lastMonthlyDate == DateTime.MinValue)
                ContentString += "First monthly email has not been sent yet.<br>";
            else
                ContentString += "Monthly email was last sent on " + lastMonthlyDate.ToLongDateString() + "<br>";
        }
        
        ContentString += "The next monthly email will be sent on " + (new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1)).ToLongDateString() + ".<br>";
    }
}
