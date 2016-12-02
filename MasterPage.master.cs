using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using SkiPatrolSchedule;

public partial class MasterPage : System.Web.UI.MasterPage
{
    public string SiteName;
    protected void Page_Load(object sender, EventArgs e)
    {
        SiteName = Baldy.SiteName;
    }
}
