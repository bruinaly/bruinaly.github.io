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

public partial class Transaction : System.Web.UI.Page
{
    public string ContentString;

    protected void Page_Load(object sender, EventArgs e)
    {
        OleDbConnection con = null;

        SiteUser CurrentUser = null;

        if (Session["User"] != null)
        {
            CurrentUser = (SiteUser)Session["User"];
        }
        else
        {
            Response.Redirect("UserError.aspx");
        }
        try
        {
            //Jet OLEDB:Database Password=corn;
            con = new OleDbConnection(Resources.Resource.ConnString);
            con.Open();
            OleDbCommand cmd = con.CreateCommand();
            OleDbDataReader reader;
   
            Int32 dayId = 0;
            if (Request["dayid"] != null)
            {
                Int32.TryParse(Request["dayid"], out dayId);
            }

            bool leadChange = false;
            bool removePatroller = false;

            int pid = 0;
            foreach (string key in Request.Form.AllKeys)
            {
                if (key.StartsWith("lead"))
                {
                    Int32.TryParse(key.Substring(4), out pid);
                    leadChange = true;
                    break;
                }

                if (key.StartsWith("removepatroller") && !key.StartsWith("removepatrolleragree"))
                {
                    Int32.TryParse(key.Substring(15), out pid);
                    removePatroller = true;
                    break;
                }
            }

            if (Request["makeinactive"] != null)
            {
                ContentString += @"<input type=""hidden"" name=""dayid"" value=""" + dayId.ToString() + @""">";

                ActiveDay adToMakeInactive = CurrentUser.ActiveDayFromID(dayId);

                ContentString += "Are you sure you would like to make " + adToMakeInactive.Date.ToShortDateString() +
                                 " inactive?<br>"
                               + @"<input id=""simplebuttons"" type=""submit"" name=""makeinactiveagree"" value=""Yes"">"
                               + @"&nbsp <input id=""simplebuttons"" type=""submit"" name=""makeinactiveagree"" value=""No"">";
            }

            if (Request["makeinactiveagree"] != null)
            {
                if ((String)Request["makeinactiveagree"] == "Yes")
                {
                    cmd.CommandText = @"delete from assignments where dayid=" + dayId.ToString();
                    cmd.ExecuteNonQuery();
                    
                    cmd.CommandText = @"delete from days where dayid=" + dayId.ToString();
                    cmd.ExecuteNonQuery();

                }
                Response.Redirect("Administrator.aspx");
            }

            if (Request["adddaybtn"] != null)
            {
                DateTime dateToMakeActive;
                DateTime.TryParse(Request["datetoadd"], out dateToMakeActive);

                ContentString += @"<input type=""hidden"" name=""datetoadd"" value=""" + dateToMakeActive.ToShortDateString() + @""">";

                ContentString += "Day: " + dateToMakeActive.ToShortDateString() + "<br>" +
                                 "Holiday: <br>"
                               + @"<input id=""simplebuttons"" type=""radio"" name=""holiday"" value=""yes""> Yes<br>"
                               + @"<input id=""simplebuttons"" type=""radio"" name=""holiday"" value=""no""> No<br>"
                               + "Minimum number of patrollers: <br>"
                               + @"<input id=""simplebuttons"" type=""text"" name=""minpatrollers""><br>"
                               + @"<input id=""simplebuttons"" type=""submit"" name=""adddayagree"" value=""OK"">"
                               + @"&nbsp <input id=""simplebuttons"" type=""submit"" name=""adddayagree"" value=""Cancel"">";

            }

            if (Request["adddayagree"] != null)
            {
                if ((String)Request["adddayagree"] == "OK")
                {
                    DateTime dateToMakeActive;
                    DateTime.TryParse(Request["datetoadd"], out dateToMakeActive);

                    int minPatrollers;
                    Int32.TryParse(Request["minpatrollers"], out minPatrollers);

                    bool isHoliday = ((string)Request["holiday"] == "yes");

                    cmd.CommandText = @"insert into Days (ActiveDay, IsHoliday, MinPatrollers, SiteID) values (" 
                                        + Baldy.AccessStringFromDateTime(dateToMakeActive) + ", "
                                        + isHoliday.ToString() + ", "
                                        + minPatrollers.ToString() + ", " 
                                        + CurrentUser.SiteID + ")";

                    cmd.ExecuteNonQuery();
                }
                Response.Redirect("Administrator.aspx");
            }

            if (leadChange)
            {
                cmd.CommandText = "update days set lead = " + pid + " where dayid = " + dayId.ToString();
                cmd.ExecuteNonQuery();
                Response.Redirect("Administrator.aspx");
            }

            if (removePatroller || Request["removepatrolleragree"] != null)
            {
                ContentString += @"<input type=""hidden"" name=""dayid"" value=""" + dayId.ToString() + @""">";
                ContentString += @"<input type=""hidden"" name=""patroller"" value=""" + pid.ToString() + @""">";

                string pName = "";

                if (Request["removepatrolleragree"] != null)
                {
                    Int32.TryParse((string)Request["patroller"], out pid);
                }

                foreach (PatrollerPerson pp in CurrentUser.AllPatrollers)
                {
                    if (pp.ID == pid)
                        pName = pp.FullName;
                }

                cmd.CommandText = @"select ActiveDay from days, assignments where days.dayid = assignments.dayid and days.dayid = " + dayId + " and patrollerid = " + pid;

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    DateTime dateToRemove;
                    DateTime.TryParse(reader["ActiveDay"].ToString(), out dateToRemove);
                    reader.Close();

                    if (removePatroller)
                    {
                        ContentString += "Are you sure you would like to remove " + pName + " from " + dateToRemove.ToShortDateString() + @"? <br>" +
                                        @"<input id=""simplebuttons"" type=""submit"" name=""removepatrolleragree"" value=""Yes""> &nbsp <input id=""simplebuttons"" type=""submit"" name=""removepatrolleragree"" value=""No"">";
                    }
                    else
                    {
                        if ((string)Request["removepatrolleragree"] == "Yes")
                        {
                            cmd.CommandText = "delete from Assignments where patrollerid =" + pid.ToString() + " and dayid=" + dayId.ToString();
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "select lead from days where dayid=" + dayId.ToString();
                            reader = cmd.ExecuteReader();
                            reader.Read();
                            if (reader.HasRows && !reader.IsDBNull(0))
                            {
                                if (reader.GetInt32(0) == pid)
                                {
                                    reader.Close();
                                    cmd.CommandText = "update days set lead=0 where dayid=" + dayId.ToString();
                                    cmd.ExecuteNonQuery();
                                }
                            }                            
                        }
                        Response.Redirect("Administrator.aspx");
                    }                        
                }
                else
                {
                    ContentString += pName + @" is not signed up for this day.&nbsp <a href=""Administrator.aspx"">Back</a>";
                }
                reader.Close();
            }

            if (Request["addpatrollerbtn"] != null || Request["addpatrolleragree"] != null)
            {
                ContentString += @"<input type=""hidden"" name=""dayid"" value=""" + dayId.ToString() + @""">";               

                if ((string)Request["patroller"] != " ")
                {
                    Int32.TryParse((string)Request["patroller"], out pid);
                }

                string pName = "";

                foreach (PatrollerPerson pp in CurrentUser.AllPatrollers)
                {
                    if (pp.ID == pid)
                        pName = pp.FullName;
                }

                ContentString += @"<input type=""hidden"" name=""patroller"" value=""" + pid.ToString() + @""">";

                cmd.CommandText = @"select ActiveDay from days, assignments where days.dayid = assignments.dayid and days.dayid = " + dayId + " and patrollerid = " + pid.ToString();

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    ContentString += pName + @" is already signed up for this date.&nbsp <a href=""Administrator.aspx"">Back</a>";
                    reader.Close();
                }
                else
                {
                    reader.Close();

                    cmd.CommandText = "select ActiveDay from days where dayId = " + dayId;
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    DateTime dateToAdd;
                    DateTime.TryParse(reader["ActiveDay"].ToString(), out dateToAdd);
                    reader.Close();
                    if (Request["addpatrollerbtn"] != null)
                    {
                        ContentString += "Are you sure you would like to sign " + pName + " up on " + dateToAdd.ToShortDateString() + @"? <br>" +
                                        @"<input id=""simplebuttons"" type=""submit"" name=""addpatrolleragree"" value=""Yes""> &nbsp <input id=""simplebuttons"" type=""submit"" name=""addpatrolleragree"" value=""No"">";
                    }
                    else
                    {
                        if ((string)Request["addpatrolleragree"] == "Yes")
                        {
                            cmd.CommandText = "insert into Assignments (DayID, PatrollerID) values(" + dayId.ToString() + ", " + pid.ToString() + ");";
                            cmd.ExecuteNonQuery();
                        }
                        Response.Redirect("Administrator.aspx");
                    }
                }
            }         
            else if (Request["signup"] != null || Request["signupagree"] != null)
            {
                ContentString += @"<input type=""hidden"" name=""dayid"" value=""" + dayId.ToString() + @""">";

                cmd.CommandText = @"select ActiveDay from days, assignments where days.dayid = assignments.dayid and days.dayid = " + dayId + " and patrollerid = " + CurrentUser.PatrollerID.ToString();
                
                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    ContentString += @"You are already signed up for this date.&nbsp <a href=""Patroller.aspx"">Back</a>";
                    reader.Close();
                }
                else
                {
                    reader.Close();

                    cmd.CommandText = "select ActiveDay from days where dayId = " + dayId;
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    DateTime dateToAdd;
                    DateTime.TryParse(reader["ActiveDay"].ToString(), out dateToAdd);
                    reader.Close();
                    if (Request["signup"] != null)
                    {
                        ContentString += "Are you sure you would like to signup on " + dateToAdd.ToShortDateString() + @"? <br>" +
                                        @"<input id=""simplebuttons"" type=""submit"" name=""signupagree"" value=""Yes""> &nbsp <input id=""simplebuttons"" type=""submit"" name=""signupagree"" value=""No"">";
                    }
                    else
                    {
                        if ((string)Request["signupagree"] == "Yes")
                        {
                            cmd.CommandText = "insert into Assignments (DayID, PatrollerID) values(" + dayId.ToString() + ", " + CurrentUser.PatrollerID + ");";
                            cmd.ExecuteNonQuery();
                        }
                        Response.Redirect("Patroller.aspx");
                    }
                }
            }
            else if (Request["remove"] != null || Request["removeagree"] != null)
            {
                ContentString += @"<input type=""hidden"" name=""dayid"" value=""" + dayId.ToString() + @""">";

                cmd.CommandText = @"select ActiveDay from days, assignments where days.dayid = assignments.dayid and days.dayid = " + dayId + " and patrollerid = " + CurrentUser.PatrollerID.ToString();

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    DateTime dateToRemove;
                    DateTime.TryParse(reader["ActiveDay"].ToString(), out dateToRemove);
                    reader.Close();

                    if (dateToRemove < DateTime.Now)
                    {
                        ContentString += @"You cannot remove assignments that have passed.&nbsp <a href=""Patroller.aspx"">Back</a>";
                    }
                    else
                    {
                        cmd.CommandText = @"select * from days, assignments where " +
                                           "days.dayid = assignments.dayid and assignments.patrollerid = " + CurrentUser.PatrollerID.ToString();

                        reader = cmd.ExecuteReader();
                        int nDays = 0;
                        while (reader.Read())
                        {
                            if (((DateTime)reader["ActiveDay"]).Year == dateToRemove.Year &&
                                ((DateTime)reader["ActiveDay"]).Month == dateToRemove.Month)
                            {
                                nDays++;
                            }
                        }

                        reader.Close();

                        if (nDays <= CurrentUser.MinDays)
                        {
                            ContentString += @"You cannot remove " + dateToRemove.ToShortDateString() + " from your assignments because you are assigned to " +
                                             nDays.ToString() + @" assignments in that month.&nbsp <a href=""Patroller.aspx"">Back</a>";
                        }
                        else if (CurrentUser.ActiveDayFromDate(dateToRemove).NonCandidateCount <= CurrentUser.ActiveDayFromDate(dateToRemove).MinPatrollers)
                        {
                            ContentString += @"There are not enough patrollers on " + dateToRemove.ToShortDateString() + @".  Another patroller must signup for this day before you can remove yourself.&nbsp <a href=""Patroller.aspx"">Back</a>";                        
                        }
                        else
                        {
                            if (Request["remove"] != null)
                            {
                                ContentString += "Contact Mt Baldy management to remove " + dateToRemove.ToShortDateString() + @" from your assignments? <br>" +
                                                @"<input id=""simplebuttons"" type=""submit"" name=""removeagree"" value=""Yes""> &nbsp <input id=""simplebuttons"" type=""submit"" name=""removeagree"" value=""No"">";
                            }
                            else
                            {
                                if ((string)Request["removeagree"] == "Yes")
                                {
                               //     cmd.CommandText = "delete from Assignments where patrollerid =" + CurrentUser.PatrollerID //+ " and dayid=" + dayId.ToString();
                                //    cmd.ExecuteNonQuery();
                                }
                                Response.Redirect("Patroller.aspx");
                            }
                        }
                    }
                }
                else
                {
                    ContentString += @"You are not signed up for this day.&nbsp <a href=""Patroller.aspx"">Back</a>";
                }
                reader.Close();
            }
      
        }
        finally
        {
            con.Close();
        }
         
    }
}