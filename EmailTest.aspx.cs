using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class EmailTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        MailMessage msg = new MailMessage();
        msg.From = new MailAddress("webmaster@skipatrolschedule.com", "Mt. Baldy");
        msg.IsBodyHtml = false;
        msg.Priority = MailPriority.High;
        //SmtpClient smtp = new SmtpClient("smtp-server.socal.rr.com");
        SmtpClient smtp = new SmtpClient("smtpout.secureserver.net");
        smtp.Credentials = new NetworkCredential("webmaster@skipatrolschedule.com", "Honey123456");
        msg.Subject = "Test";
        msg.To.Add("bruinaly@gmail.com");
        smtp.Send(msg);
    }
}
