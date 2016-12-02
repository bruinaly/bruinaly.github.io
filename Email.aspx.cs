//These are the email sending routines. There are 2 types of emails:
// Weekly, sent out on Monday and Friday to the patrollers that are assigned.
// weekly will also blast the patrol if we are light.
// Monthly, sent out after the 25th for the next month, and on the 1st of the month
//  Monthly also sends out a blast if there are days less than min requirement +2 patrollers
//  It also notifies the administrators if there are any days that need plds... right now
//  administrtators has temp to only Jodi.
//These routines are designed to be called continually and only act on the appropriate dates.



// using  SERVER = "relay-hosting.secureserver.net";

using System.Collections.Generic;
using System;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Collections;
using System.Net;
//using System.Net.Mail;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;
using System.Web.Mail;

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
        public const string PatrolName = "Mt. Baldy Patrol";
        public const string Signature ="\r\n\r\nThanks,\r\n"+PatrolName+"\r\n\r\n\r\nThis is an automated message from www.skipatrolschedule.com";


// copied from godaddy example
private void SendEmail1(MailMessage oMail)
{
   const string SERVER = "relay-hosting.secureserver.net";
   System.Net.Mail.MailMessage tMail = new System.Net.Mail.MailMessage("do_not_reply@skipatrolschedule.com", "robertbible@gmail.com");
  
//mailMessage.From = new MailAddress("do_not_reply@skipatrolschedule.com");
//    mailMessage.
//  mailMessage.To = "robertbible@gmail.com";
  tMail.Subject = "sent email subject" + oMail.To ;
  //mailMessage.BodyFormat = MailFormat.Text; // enumeration
  tMail.Priority = System.Net.Mail.MailPriority.High; // enumeration
  tMail.Body = "Sent at: " + DateTime.Now+ oMail.To + oMail.Body;

  System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(SERVER);
   //SmtpMail.SmtpServer = SERVER;
   //SmtpMail.Send(mailMessage);
   client.Send(tMail);
   //System.Threading.Thread.Sleep(3000);
   //SmtpMail.Send(oMail);
   //System.Threading.Thread.Sleep(1000);
   //ContentString =oMail.To +".<br>";

 tMail = null; // free up resources
}

    void SendEmail(string to, string subject, string body)
    {
        const string SERVER = "relay-hosting.secureserver.net";
        System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage("do_not_reply@skipatrolschedule.com",to);

        mailMessage.Subject = subject+" "+PatrolName;
        mailMessage.Priority = System.Net.Mail.MailPriority.High; // enumeration
        mailMessage.Body = body;

        Trace.Write("Debugging", "Message was sent to: " + to + " at " + DateTime.Now);

        System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(SERVER);
        client.Send(mailMessage);

        //System.Net.Mail.MailMessage mailMessageToRobert = new System.Net.Mail.MailMessage("do_not_reply@skipatrolschedule.com", "robertbible@gmail.com");
        //client.Send(mailMessageToRobert);
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
        // sends out the monthly schedule uses this month if before the 25th
        // uses next month if after the 25th.. so it can be used to send
        // out the the schedule before days have been assigned
    {
        const string SERVER = "relay-hosting.secureserver.net";

        System.Web.Mail.MailMessage oMail = new System.Web.Mail.MailMessage();
        oMail.From = "do_not_reply@skipatrolschedule.com";
        oMail.BodyFormat = MailFormat.Text; // enumeration
        oMail.Priority = MailPriority.High; // enumeration

        List<EmailRecipient> recps = EmailRecipientsForAllPatrollers();
        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (DateTime.Now.Day < 25)
        {
         //leave it from above //DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }
        else // this is the end of the month must point at the next month
        {
            //DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
          startDate = startDate.AddMonths(1);
        }
        DateTime endDate = startDate.AddMonths(1);
        UpdateEmailRecipientListFromDateRange(recps, startDate, endDate);        

        foreach (EmailRecipient er in recps)
        {
            if (er.Email == String.Empty)
                continue;
            else
            {
                oMail.To="";
               oMail.To=(er.Email);
            }

            string Content =
                "Hello " + er.FirstName + " " + er.LastName + ",\r\n" +
                "\r\nYou are currently scheduled for " + er.Assignments.Count + " days this month.\r\n\r\n";
            foreach (DateTime dt in er.Assignments)
            {
                Content += dt.ToLongDateString() + "\r\n";
            }

            if (er.Assignments.Count == 1)
            {
                Content +=
                    "\r\nIf you can not make it to this scheduled day please login to www.skipatrolschedule.com " +
                    "and make your changes.\r\n";
            }
            else if (er.Assignments.Count > 1)
            {
                Content +=
                    "\r\nIf you can not make it to these scheduled days please login to www.skipatrolschedule.com " +
                    "and make your changes.\r\n";
            }

            if (er.Assignments.Count < er.MinDays)
            {
                Content += "You are currently below your commitment of " + er.MinDays.ToString() +
                    ". You must signup for additional days or days will be assigned for you automatically.\r\n";
            }

            Content += Signature;
            if (er.Assignments.Count < er.MinDays)
                oMail.Subject = "This month's schedule for " + er.FirstName + " " + er.LastName + ": Signup for more days!!";
            else
                oMail.Subject = "This month's schedule for " + er.FirstName + " " + er.LastName;
            oMail.Body = Content;
           // oMail.To = "robertbible@gmail.com";
            //SendEmail(oMail);
            SendEmail(oMail.To, oMail.Subject, oMail.Body);
        }
// now we need to tell administrators and patrol which days are light and have no PLD
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

                if (ad.NonCandidateCount < ad.MinPatrollers + 2)
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
                    oMail.To = "";
                    oMail.To = (er.Email);
                    oMail.To = "jodib@measurepcb.com"; //temp
                }

                string Content =
                    "Hello " + er.FirstName + " " + er.LastName + ",\r\n" +
                    "\r\nThere are no leads assigned for " + activeDaysWithNoLead.Count + " day(s) this week.\r\n";

                foreach (ActiveDay ad in activeDaysWithNoLead)
                {
                    Content += "\r\n" + ad.Date.ToLongDateString() + "\r\n\r\n";
                }

                Content += Signature;

                oMail.Subject = "No Leads this week!!";
                oMail.Body = Content;
                // oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);
                SendEmail(oMail.To, oMail.Subject, oMail.Body);
                oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);
                SendEmail(oMail.To, oMail.Subject, oMail.Body);
            }
        }

        List<EmailRecipient> allPatrollers = EmailRecipientsForAllPatrollers();
        // if we don't have enough assigned send out to the patrol
        if (activeDaysBelowMin.Count > 0)
        {
            foreach (EmailRecipient er in allPatrollers)
            {
                if (er.Email == String.Empty)
                    continue;
                else
                {
                    oMail.To = "";
                    oMail.To = er.Email;
                }

                string Content =
                    "Hello " + er.FirstName + " " + er.LastName + ",\r\n" +
                    "\r\nMore patrolers are needed on the following days this week:\r\n";

                foreach (ActiveDay ad in activeDaysBelowMin)
                {
                    Content += "\r\n" + ad.Date.ToLongDateString() + "\r\n\r\n";
                }

                Content += Signature;

                oMail.Subject = "Patrollers needed this week!!";
                oMail.Body = Content;

                // oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);
                SendEmail(oMail.To, oMail.Subject, oMail.Body);
                //oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);  

            }
        }



        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "update site set LastMonthlyEmail = " + Baldy.AccessStringFromDateTime(DateTime.Now) + " WHERE siteid = " + CurrentUser.SiteID;
        cmd.ExecuteNonQuery();
        oMail = null; // free up resources
    }

    protected void WeeklyEmail()
    {

      MailMessage oMail = new System.Web.Mail.MailMessage();
      oMail.From = "do_not_reply@skipatrolschedule.com";
      oMail.BodyFormat = MailFormat.Text; // enumeration
      oMail.Priority = MailPriority.High; // enumeration
 


        List<EmailRecipient> recps = new List<EmailRecipient>();
        DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        DateTime endDate = startDate.AddDays(7); 
        UpdateEmailRecipientListFromDateRange(recps, startDate, endDate);


        foreach (EmailRecipient er in recps)
        {
            if (er.Email == String.Empty)
                continue;
            else
            {
                oMail.To="";
                oMail.To = (er.Email);
                
            }

            string Content =
                "Hello " + er.FirstName + " " + er.LastName + ",\r\n" +
                "\r\nYou are currently scheduled for " + er.Assignments.Count + " day(s) this week.\r\n";
            
            foreach (DateTime dt in er.Assignments)
            {
                Content += "\r\n" + dt.ToLongDateString() + "\r\n\r\n";
                ActiveDay ad = CurrentUser.ActiveDayFromDate(dt);
                foreach (PatrollerPerson pp in ad.Patrollers)
                { 
                    Content += pp.FullName + ", \r\n";
                }
            }            

            Content += Signature;           
            
            oMail.Subject = "This week's schedule for " + er.FirstName + " " + er.LastName;
            oMail.Body = Content;

            //oMail.To = "robertbible@gmail.com";
            //SendEmail(oMail);
            SendEmail(oMail.To, oMail.Subject, oMail.Body);
            //oMail.To = "robertbible@gmail.com"; // send a copy
            //SendEmail(oMail);  

        }

        // now we need to tell administrators and patrol which days are light and have no PLD
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

                if (ad.NonCandidateCount < ad.MinPatrollers + 2)
                {
                    activeDaysBelowMin.Add(ad);
                }
            }
        }


        List<EmailRecipient> allPatrollers = EmailRecipientsForAllPatrollers();
        // if we don't have enough assigned send out to the patrol
        if (activeDaysBelowMin.Count > 0)
        {
            foreach (EmailRecipient er in allPatrollers)
            {
                if (er.Email == String.Empty)
                    continue;
                else
                {
                    oMail.To = "";
                    oMail.To = er.Email;
                }

                string Content =
                    "Hello " + er.FirstName + " " + er.LastName + ",\r\n" +
                    "\r\nMore patrolers are needed on the following days this week:\r\n";

                foreach (ActiveDay ad in activeDaysBelowMin)
                {
                    Content += "\r\n" + ad.Date.ToLongDateString() + "\r\n\r\n";
                }

                Content += Signature;

                oMail.Subject = "Patrollers needed this week!!";
                oMail.Body = Content;

                // oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);
                SendEmail(oMail.To, oMail.Subject, oMail.Body);
                //oMail.To = "robertbible@gmail.com";
                //SendEmail(oMail);  

            }
        }



        OleDbConnection con = new OleDbConnection(Resources.Resource.ConnString);
        con.Open();
        OleDbCommand cmd = con.CreateCommand();

        cmd.CommandText = "update site set LastWeeklyEmail = " + Baldy.AccessStringFromDateTime(DateTime.Now) + " WHERE siteid = " + CurrentUser.SiteID;
        cmd.ExecuteNonQuery();
        oMail = null;
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
        //send weekly out on Monday and Friday... will also send out if it has been greater than 4 days in case we missed
        if (((DateTime.Now.AddDays(-1) > lastWeeklyDate) &&
                                        ((DateTime.Now.DayOfWeek == DayOfWeek.Monday) || (DateTime.Now.DayOfWeek == DayOfWeek.Friday)))
            || (DateTime.Now.AddDays(-4) > lastWeeklyDate))

        //(DateTime.Now.AddDays(-3) > lastWeeklyDate && DateTime.Now.DayOfWeek.ToString() >= dayOfWeekToSend)
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
            //ContentString += "Weekly emails are sent on " + dayOfWeekToSend + "s.<br>&nbsp<br>";
            ContentString += "Weekly emails are sent on Mondays and Fridays" + "s.<br>&nbsp<br>";

        else
            ContentString += "Weekly emails are not sent for this site.<br>&nbsp<br>";

        if (((DateTime.Now.Day >= 1) && (DateTime.Now.AddDays(-27) > lastMonthlyDate)) ||
           (DateTime.Now.Day >= 25) && (DateTime.Now.AddDays(-6) > lastMonthlyDate)) // send an end of the month for the next month
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
        if (DateTime.Now.Day >= 25)
        {
            ContentString += "The next monthly email will be sent on " + (new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1)).ToLongDateString() + ".<br>";
        }
        else // we will send it out on 25th
        {
            DateTime moment = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            moment = moment.AddDays(24);

            ContentString += "The next monthly email will be sent on " + (moment.ToLongDateString()) + ".<br>";

        }
         }
}
