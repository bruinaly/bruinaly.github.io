<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>
    
    
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    
    <form id="form1" runat="server">
        <div id="wrapper-menu-page">
	        <div id="menu-page">
	            <center>
                <asp:Login ID="Login1" runat="server"
                    DestinationPageUrl="~/Patroller.aspx" Height="101px" OnAuthenticate="Login1_Authenticate" Width="1px" DisplayRememberMe="False" LoginButtonText="Login" PasswordLabelText="Password" TextLayout="TextOnTop" TitleText="" UserNameLabelText="Username" UserNameRequiredErrorMessage="Username is required.">
                    <LoginButtonStyle BackColor="White" Font-Names="Lucida Grande,Lucida Sans Unicode,arial,sans-serif" Font-Size="8pt" BorderStyle="Outset" />
                    <TextBoxStyle Font-Overline="False" />
                </asp:Login> 
                </center>
            </div><!--menu-page-->
        </div>

        <div id="content">
            <h3>Welcome to the Mt. Baldy Ski Patrol Schedule Website!</h3>
        
          Login using your last name as your username and your NSP number as your password.
            If you do not have an NSP number your last name will serve as your password until
            you change it in your profile.<br />
            <br />
            *It is highly recommended that you change your username and/or password to your
            liking.<br />
            <br />
            <br />
            <a href="Article.html">Click here</a> to read about skipatrolschedule.com.
         </div>
    </form>
    </asp:Content>        
    