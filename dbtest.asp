<%
path = server.mapPath("/access_db")
database = "baldy.mdb"
username = ""
password = ""
connString=("Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" & path & "\" & database & "';User Id=" & username & ";Password=" & password)
Set objConn = CreateObject("ADODB.Connection")
objConn.open = connString
%>
<!DOCTYPE HTML PUBLIC '-//W3C//DTD HTML 4.01 Transitional//EN' 'http://www.w3.org/TR/html4/loose.dtd'>
<html><head><title>ASP with Access - By Clyde</title><meta http-equiv='Content-Type' content='text/html; charset=iso-8859-1'>
</head><body style='background-color:#ffffff;' text='000000' link='0000FF' vlink='990099' alink='990099' topmargin='0' leftmargin='0' marginheight='0' marginwidth='0'>
<center><h1>ASP with Access Database</h1><b>Make sure the database is in the access_db folder in the hosting account.</b><br><br><h3>This script written especially for the baldy.mdb database.</h3>
<br><h2>The minimum connection code is:</h2><table cellspacing=0><tbody>
<tr><td align=left style="background-color:#eee685;">path = server.mapPath(<span style="color:#8B0A50;">"/access_db"</span>)</td></tr>
<tr><td align=left style="background-color:#eee685;">database = <span style="color:#8B0A50;">"baldy.mdb"</span></td></tr>
<tr><td align=left style="background-color:#eee685;">username = <span style="color:#8B0A50;">""</span></td></tr>
<tr><td align=left style="background-color:#eee685;">password = <span style="color:#8B0A50;">""</span></td></tr>
<tr><td align=left style="background-color:#eee685;">connString=(<span style="color:#8B0A50;">"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='"</span> & path & <span style="color:#8B0A50;">"\"</span> & database & <span style="color:#8B0A50;">"';User Id="</span> & username & <span style="color:#8B0A50;">";Password="</span> & password)</td></tr>
<tr><td align=left style="background-color:#eee685;">Set objConn = CreateObject(<span style="color:#8B0A50;">"ADODB.Connection"</span>)</td></tr>
<tr><td align=left style="background-color:#eee685;">objConn.open = connString</td></tr>
<tr><td align=left>*********************************** Do Something *************************************</td></tr>
<tr><td align=left style="background-color:#eee685;">objConn.close</td></tr>
<tr><td align=left style="background-color:#eee685;">set objConn = nothing</td></tr>
</tbody></table>
<h1>Connection Test Successful!</h1>
</center></form></body></html>
<%
objConn.close
set objConn = nothing
%>
