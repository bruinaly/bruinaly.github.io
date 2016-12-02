<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Patroller.aspx.cs" Inherits="Patroller" %>    
    
    
    
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
   
   
   <div id="content">
    <form id="form2" runat="server">
        
            
            <h2><%=PatrollerName %></h2>
            <%=UserSection %>
            <h3>
                Outlook</h3>
            <%=AssignmentSection %>
            <h3>Calendar</h3>
            
            </form>
            <%=NavigatorSection %>
            <form method="post" action="Transaction.aspx">
            <%=MonthSection %>
            </form>
            </div>
    </asp:Content>        
    
    