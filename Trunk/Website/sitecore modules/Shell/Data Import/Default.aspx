<%@ Page Language="C#" AutoEventWireup="true" Debug="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Import</title>
	<link href="css/global.css?id=dded" rel="stylesheet" type="text/css">
</link>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="scrptManager" runat="server"></asp:ScriptManager>
    <div class="MainWrapper">
        <h1>Data Import Utility</h1>
        <div class="Section">
			<div class="SectionTitle">SQL Import</div>
			<div>
				<label>Connection String:</label>
				<br />
				<asp:UpdatePanel ID="upRefresh" runat="server">
					<ContentTemplate>
						<asp:DropDownList ID="ddlConnStr" runat="server"></asp:DropDownList>
						<br /><br />
						<label>Import Setting:</label>
						<br />	
						<asp:DropDownList ID="ddlSQLImport" runat="server"></asp:DropDownList>
						<asp:Button ID="btnRefresh" Text="Refresh" OnClick="btnRefresh_Click" runat="server" />
					</ContentTemplate>
				</asp:UpdatePanel>
				<br />
				<asp:Button ID="btnSQLImport" Text="Import" OnClick="btnSQLImport_Click" runat="server" />
				<br /><br />
			</div>
			<div class="Message">
				<asp:Literal ID="ltlSQLMessage" runat="server"></asp:Literal>
			</div>
        </div>
        <div class="Section">
			<div class="SectionTitle">Sitecore Import</div>
			<div>
				<label>Import Setting:</label>
				<br />
				<asp:DropDownList ID="ddlSCImport" runat="server"></asp:DropDownList>
				<br /><br />
				<asp:Button ID="btnSCImport" Text="Import" OnClick="btnSCImport_Click" runat="server" />
				<br /><br />
			</div>
			<div class="Message">
				<asp:Literal ID="ltlSCMessage" runat="server"></asp:Literal>
			</div>
        </div>
    </div>
    </form>
</body>
</html>
