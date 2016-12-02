<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="UserSettings.aspx.cs" Inherits="UserSettings" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
        <div id="content">

            <%=OptionsSection %>
            <h3>
                Profile</h3>
                    <form id="form2" runat="server">
                        <asp:DetailsView ID="DetailsView1" runat="server" AutoGenerateRows="False" CellPadding="4"
                            DataKeyNames="PatrollerID" DataSourceID="AccessDataSource1" ForeColor="#333333"
                            GridLines="None" Height="50px" Width="158px">
                            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                            <CommandRowStyle BackColor="#E2DED6" Font-Bold="True" />
                            <EditRowStyle BackColor="#999999" />
                            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                            <Fields>
                                <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
                                <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                                <asp:BoundField DataField="Username" HeaderText="Username" SortExpression="Username" />
                                <asp:BoundField DataField="Pass" HeaderText="Password" SortExpression="Pass" />
                                <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                                <asp:CommandField ShowEditButton="True" />
                            </Fields>
                            <FieldHeaderStyle BackColor="#E9ECF1" Font-Bold="True" />
                            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                        </asp:DetailsView>
                        <asp:AccessDataSource ID="AccessDataSource1" runat="server" DataFile="access_db/baldy.mdb"
                            DeleteCommand="DELETE FROM [Patroller] WHERE [PatrollerID] = ?" InsertCommand="INSERT INTO [Patroller] ([PatrollerID], [FirstName], [LastName], [Username], [Pass], [Email]) VALUES (?, ?, ?, ?, ?, ?)"
                            SelectCommand="SELECT [PatrollerID], [FirstName], [LastName], [Username], [Pass], [Email] FROM [Patroller] WHERE ([PatrollerID] = ?)"
                            UpdateCommand="UPDATE [Patroller] SET [FirstName] = ?, [LastName] = ?, [Username] = ?, [Pass] = ?, [Email] = ? WHERE [PatrollerID] = ?">
                            <DeleteParameters>
                                <asp:Parameter Name="PatrollerID" Type="Int32" />
                            </DeleteParameters>
                            <UpdateParameters>
                                <asp:Parameter Name="FirstName" Type="String" />
                                <asp:Parameter Name="LastName" Type="String" />
                                <asp:Parameter Name="Username" Type="String" />
                                <asp:Parameter Name="Pass" Type="String" />
                                <asp:Parameter Name="Email" Type="String" />
                                <asp:Parameter Name="PatrollerID" Type="Int32" />
                            </UpdateParameters>
                            <SelectParameters>
                                <asp:SessionParameter Name="PatrollerID" SessionField="PatrollerID" Type="Int32" />
                            </SelectParameters>
                            <InsertParameters>
                                <asp:Parameter Name="PatrollerID" Type="Int32" />
                                <asp:Parameter Name="FirstName" Type="String" />
                                <asp:Parameter Name="LastName" Type="String" />
                                <asp:Parameter Name="Username" Type="String" />
                                <asp:Parameter Name="Pass" Type="String" />
                                <asp:Parameter Name="Email" Type="String" />
                            </InsertParameters>
                        </asp:AccessDataSource>
                    </form>
            </div>
       
</asp:Content>

