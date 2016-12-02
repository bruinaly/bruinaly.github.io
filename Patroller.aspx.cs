using System;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;

public partial class Patroller : System.Web.UI.Page
{        
    public String UserSection;
    public String MonthSection;
    public String MessageSection;    
    public String AssignmentSection;  
    private SiteUser CurrentUser;
    public String PatrollerName;
    public String NavigatorSection;

    private string FillSmallDay(DateTime d)
    {
        bool userSignedUp = false;
        string s = string.Empty;

        foreach (DateTime i in CurrentUser.Assignments)
        {
            if (i.Equals(d))
            {
                userSignedUp = true;
            }
        }

        if (userSignedUp)
        {
            s += "<td id=\"mysmallday\">";
        }
        else
        {
            s += "<td id=\"smallday\">";
        }

        s += d.Day;

        s += "</td>";
        return s;
    }


    private string FillDayCell(DateTime d)
    {
        ActiveDay ad = CurrentUser.ActiveDayFromDate(d);
        string s = String.Empty;

        
        Boolean userSignedUp = false;

        if (ad != null)
        {
            s += @"<form method=""post"" action=""Transaction.aspx""><input type=""hidden"" name=""dayid"" value=""" + ad.DayId + @""">";
            foreach (PatrollerPerson p in ad.Patrollers)
            {
                if (p.ID == CurrentUser.PatrollerID)
                {
                    userSignedUp = true;
                }
            }

            if (userSignedUp)
            {
                if (ad.IsHoliday)
                    s += "<td id=\"myholiday\">";
                else
                    s += "<td id=\"myday\">";
            }
            else
            {
                if (ad.IsHoliday)
                    s += "<td id=\"activeholiday\">";
                else
                    s += "<td id=\"activeday\">";
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
            s += "<td id=\"day\"><span id=\"dayheader\">" + d.Day.ToString() + "</span><br>";
        }

        if (ad != null)
        {
            if (ad.Date >= DateTime.Now)
            {
                s += "<center>";
                if (!userSignedUp)
                {
                    s += @"<input id=""simplebuttons"" type=""submit"" name=""signup"" value=""Signup""><br>";
                }
                else
                {
                    s += @"<input id=""simplebuttons"" type=""submit"" name=""remove"" value=""Remove""><br>";
                }

                if (ad.NonCandidateCount < ad.MinPatrollers)
                {
                    s += @"<font color=""Red""><img src=""images/mediumwarning.gif"" align=""absmiddle""> Patrollers Needed!</font><br>";
                }
                s += "</center>";
            }
            foreach (PatrollerPerson p in ad.Patrollers)
            {
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
        }

        if (ad != null)
            s += "</td></form>";
        else
            s += "</td>";
        return s;
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        if (Session["User"] != null)
        {
            CurrentUser = (SiteUser)Session["User"];
        }
        else
        {
            Response.Redirect("UserError.aspx");
        }

        if (Request["datechanged"] != null)
        {
            int month, year;
            Int32.TryParse(Request["month"].ToString(), out month);
            Int32.TryParse(Request["year"].ToString(), out year);

            CurrentUser.ViewDate = new DateTime(year, month, 1);            
        }

        Baldy.UpdateUser(CurrentUser);

        UserSection =
            CurrentUser.PType + "<br>" +
            "Email: " + CurrentUser.Email + "<br>" +
            @"<a href=""UserSettings.aspx"">Profile</a><br>";
        if (CurrentUser.IsAdministrator)
            UserSection += @"<a href=""Administrator.aspx"">Administrative Tools</a><br>";        

        PatrollerName = CurrentUser.FirstName + " " + CurrentUser.LastName;

        DateTime curMon = DateTime.Now;

        AssignmentSection += "<table><tr>";
        
        String[] monthStrings = Enum.GetNames(typeof(Months));

        for (int i = 0; i < 3; i++)
        {
            DateTime mon = curMon.AddMonths(i);

            AssignmentSection += "<td><center>" + monthStrings[mon.Month - 1] + " " + mon.Year.ToString() + "</center></td>";
        }

        AssignmentSection += "</tr><tr>";

        for (int i = 0; i < 3; i++)
        {
            DateTime mon = curMon.AddMonths(i);

            AssignmentSection += "<td>";
            AssignmentSection += Baldy.MakeMonthCalendar(mon, FillSmallDay, true);
            AssignmentSection += "</td>";
        }

        AssignmentSection += "</tr>";

        for (int i = 0; i < 3; i++)
        {
            DateTime mon = curMon.AddMonths(i);

            AssignmentSection += "<td>";

            int assignmentsInMonth = 0;

            foreach (DateTime dt in CurrentUser.Assignments)
            {
                if (dt.Month == mon.Month && dt.Year == mon.Year)
                {
                    assignmentsInMonth++;
                }
            }

            if (assignmentsInMonth < CurrentUser.MinDays)
            {
                AssignmentSection += @"<font color=""Red""><img src=""images/mediumwarning.gif"" align=""absmiddle""> Signup for more days!</font><br>";
            }
            AssignmentSection += "</td>";
        }

        AssignmentSection += "</tr></table>";

        if (CurrentUser.Assignments.Count < CurrentUser.MinDays)
        {
            MessageSection += "You are currently below your minimum number of days.";
        }
        else
        {
            MessageSection += "There are no new messages.";
        }

        NavigatorSection = @"<form action=""Patroller.aspx"" method=""post"">Month:&nbsp<select id=""simplebuttons"" name=""month"">";
        string[] months = Enum.GetNames(typeof(Months));
        for (int i = 0; i < 12; i++)
        {
            if (CurrentUser.ViewDate.Month == i+1)
            {
                NavigatorSection += @"<option value=""" + (i+1).ToString() + @""" selected=""selected"">" + months[i] + "</option>";
            }
            else
            {
                NavigatorSection += @"<option value=""" + (i+1).ToString() + @""">" + months[i] + "</option>";
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
    
        MonthSection += Baldy.MakeMonthCalendar(CurrentUser.ViewDate, new Baldy.DayProcessor(FillDayCell), false);
    }
}