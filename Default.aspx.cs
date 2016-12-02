using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mail;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Baldy.SiteName = "Welcome to skipatrolschedule.com";
    }

    protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
    {
        OleDbConnection con = null;
      //string ConnString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='d:\hosting\robertbible\access_db\baldy.mdb'";        

        try
        {
            con = new OleDbConnection(Resources.Resource.ConnString);
            con.Open();
            OleDbCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from Patroller";
            OleDbDataReader reader = cmd.ExecuteReader();
            SiteUser newUser = new SiteUser();
            while (reader.Read())
            {
                if (((string)reader["Username"]).ToUpper() == Login1.UserName.ToUpper() && ((string)reader["Pass"]).ToUpper() == Login1.Password.ToUpper())
                {
                    e.Authenticated = true;
                    
                    newUser.PatrollerID = (int)reader["PatrollerID"];
                    newUser.FirstName = (string)reader["FirstName"];
                    newUser.LastName = (string)reader["LastName"];
                    newUser.MinDays = (int)reader["MinDays"];
                    newUser.ViewDate = DateTime.Today;
                    newUser.IsAdministrator = (bool)reader["IsAdmin"];
                    newUser.Email = (string)reader["Email"];
                    newUser.PType = (string)reader["PType"];
                    newUser.SiteID = (int)reader["SiteID"];
                    newUser.SiteActiveDays = new System.Collections.Generic.List<ActiveDay>();
                    Session.RemoveAll();                    
                    Session.Add("User", newUser);
                    Session.Add("PatrollerID", newUser.PatrollerID);
                    Session.Add("SiteID", newUser.SiteID);
                    break;
                }
            }
            reader.Close();
            if (e.Authenticated)
            {
                cmd.CommandText = "select sitename from site where siteid = " + newUser.SiteID;
                reader = cmd.ExecuteReader();

                reader.Read();
                Baldy.SiteName = reader.GetString(0);
            }
            else
            {
                e.Authenticated = false;
            }
        }
        finally
        {
            if (con != null)
                con.Close();
        }
    }
}