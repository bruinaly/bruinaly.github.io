using System;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Web;
using System.Web.Mail;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;

public partial class Administrator : System.Web.UI.Page
{
    public String MonthSection;
    private SiteUser CurrentUser;
    public String NavigatorSection;
    public String OptionsSection;

    public delegate string DayProcessor(DateTime d);

    private string FillDayCell(DateTime d)
    {        
        ActiveDay ad = CurrentUser.ActiveDayFromDate(d);
        string s = String.Empty;        

        

        if (ad != null)
        {
            s += @"<form method=""post"" action=""Transaction.aspx""><input type=""hidden"" name=""dayid"" value=""" + ad.DayId + @""">";
            if (ad.NonCandidateCount < ad.MinPatrollers || ad.LeadID == 0)
            {
                s += "<td id=\"adminwarningday\">";
            }
            else
            {
                s += "<td id=\"myday\">";
            }
            

            if (ad.IsHoliday)
            {
                s += "<span id=\"holidayheader\">" + d.Day.ToString() + "</span><br>";
            }
            else
            {
                s += "<span id=\"dayheader\">" + d.Day.ToString() + "</span><br>";
            }
        }
        else
        {
            s += @"<form method=""post"" action=""Transaction.aspx"">"
              + "<td id=\"day\"><span id=\"dayheader\">" + d.Day.ToString() + "</span><br>"
              + @"<table width =""130""><tr><td id=""divadminactiveday""><center>" 
              + @"<input type=""hidden"" name=""datetoadd"" value=""" + d.ToShortDateString() + @""">"
              + @"<input id=""simplebuttons"" type=""submit"" name=""adddaybtn"" value=""Activate"">" +
               @"</center></td></tr></table>";
        }

        if (ad != null)
        {            
            if (ad.NonCandidateCount < ad.MinPatrollers || ad.LeadID == 0)
            {
                if (ad.NonCandidateCount < ad.MinPatrollers)
                {
                    s += @"<img src=""images/mediumwarning.gif"" align=""absmiddle""> Minimum = " + ad.MinPatrollers.ToString() + "<br>";
                }

                if (ad.LeadID == 0)
                {
                    s += @"<img src=""images/mediumwarning.gif"" align=""absmiddle""> No lead assigned!<br>";
                }
                s += "<hr>";
            }
        
            s += "<font color=\"black\">";

            foreach (PatrollerPerson p in ad.Patrollers)
            {
                if (p.ID == ad.LeadID)
                {
                    s += @"<input id=""simplebuttons"" type=""submit"" name=""removepatroller" + p.ID + @""" value=""-""> ";
                }
                else
                {
                    s += @"<input id=""simplebuttons"" type=""submit"" name=""removepatroller" + p.ID + @""" value=""-"">"
                       + @"&nbsp<input id=""simplebuttons"" type=""submit"" name=""lead" + p.ID + @""" value=""L""> ";
                }

                if (p.ID == ad.LeadID)
                {
                    s += @"<font color=""blue"">" + p.FullName + "</font><br>";
                }
                else if (p.IsCandidate)
                {
                    s += @"<font color=""green"">" + p.FullName + "</font><br>";
                }
                else
                {
                    s += "<font color=\"black\">" + p.FullName + "</font><br>";
                }
            }
            if (ad.Patrollers.Count > 0)
                s += "</font><hr>";
            else
                s += "</font>";

            s += @"<select id = ""simplebuttons"" name=""patroller""><option value="" "">";            

            foreach (PatrollerPerson p in CurrentUser.AllPatrollers)
            {
                Boolean userSignedUp = false;
                foreach (PatrollerPerson p1 in ad.Patrollers)
                {
                    if (p.ID == p1.ID)
                    {
                        userSignedUp = true;
                        break;
                    }
                }
                if (!userSignedUp)
                {
                    s += @"<option value=""" + p.ID + @""">" + p.FullName + "</option>";
                }
            }
            s += @"</select>&nbsp<input id=""simplebuttons"" type=""submit"" name=""addpatrollerbtn"" value=""Add"">"
               + @"&nbsp<input type=""submit"" id=""simplebuttons"" name=""makeinactive"" value=""Inactivate"">";

        }


        s += "</form></td>";
        return s;
    }

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

        Baldy.UpdateUser(CurrentUser);

        if (Request["datechanged"] != null)
        {
            int month, year;
            Int32.TryParse(Request["month"].ToString(), out month);
            Int32.TryParse(Request["year"].ToString(), out year);

            CurrentUser.ViewDate = new DateTime(year, month, 1);
        }

        NavigatorSection += @"<a href=""Administrator.aspx"">Schedule Manager</a><br>";
        NavigatorSection += @"<a href=""AdminPatrollers.aspx"">Patroller Manager</a><br>";
        NavigatorSection += @"<a href=""Email.aspx"">Email</a><br>";
        NavigatorSection += @"<a href=""Patroller.aspx"">Exit</a><br>";

        NavigatorSection += @"<form action=""Administrator.aspx"" method=""post"">Month:&nbsp<select id=""simplebuttons"" name=""month"">";
        string[] months = Enum.GetNames(typeof(Months));
        for (int i = 0; i < 12; i++)
        {
            if (CurrentUser.ViewDate.Month == i + 1)
            {
                NavigatorSection += @"<option value=""" + (i + 1).ToString() + @""" selected=""selected"">" + months[i] + "</option>";
            }
            else
            {
                NavigatorSection += @"<option value=""" + (i + 1).ToString() + @""">" + months[i] + "</option>";
            }
        }

        NavigatorSection += @"</select>&nbsp<select id=""simplebuttons"" name=""year"">";

        for (int y = 2006; y <= 2025; y++)
        {
            if (CurrentUser.ViewDate.Year == y)
            {
                NavigatorSection += @"<option value=""" + y.ToString() + @""" selected=""selected"">" + y.ToString() + "</option>";
            }
            else
            {
                NavigatorSection += @"<option value=""" + y.ToString() + @""">" + y.ToString() + "</option>";
            }
        }

        NavigatorSection += @"</select>&nbsp<input type=""submit"" id=""simplebuttons"" name=""datechanged"" value=""Go""></form><br>";

        MonthSection = Baldy.MakeMonthCalendar(CurrentUser.ViewDate, new Baldy.DayProcessor(FillDayCell), false);
    }

    protected void AccessDataSource1_Deleted(object sender, SqlDataSourceStatusEventArgs e)
    {
        Int32 pid;

        if (Int32.TryParse(e.Command.Parameters["patrollerid"].Value.ToString(), out pid))
        {
            OleDbConnection con = null;
            try
            {
                con = new OleDbConnection(Resources.Resource.ConnString);
                con.Open();
                OleDbCommand cmd = con.CreateCommand();

                cmd.CommandText = "delete from assignments where patrollerid=" + pid.ToString();
                cmd.ExecuteNonQuery();

                cmd.CommandText = "update days set lead=0 where lead=" + pid.ToString();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        Baldy.UpdateUser(CurrentUser);
        Response.Redirect("Administrator.aspx");
    }
}