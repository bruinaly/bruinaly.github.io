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

public partial class AdminPatrollers : System.Web.UI.Page
{
    private SiteUser CurrentUser;
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

        Baldy.UpdateUser(CurrentUser);

        NavigatorSection += @"<a href=""Administrator.aspx"">Schedule Manager</a><br>";
        NavigatorSection += @"<a href=""AdminPatrollers.aspx"">Patroller Manager</a><br>";
        NavigatorSection += @"<a href=""Email.aspx"">Email</a><br>";
        NavigatorSection += @"<a href=""Patroller.aspx"">Exit</a><br>";     
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
    protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}