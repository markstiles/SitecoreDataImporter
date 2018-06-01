<%@ Page Language="C#" AutoEventWireup="true" Debug="true" CodeBehind="Default.aspx.cs" Inherits="Sitecore.SharedSource.DataImporter.Editors.Default" %>
<%@ Import namespace="Sitecore" %>
<%@ Import namespace="Sitecore.Jobs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Import</title>
	<link href="css/global.css?id=dded" rel="stylesheet" type="text/css"></link>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>
    <script type="text/javascript">
    	$(document).ready(function () {
    		$('h2').dblclick(function () {
    			$(this).next(".Section").toggle();
    		});
    	});
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div class="MainWrapper">
        <h2>Import Settings</h2>
        <div class="Section">
            <div class="Controls">
			    <div class="formRow">
                    <div class="rowTitle noButtons">Connection String:</div>
                    <asp:DropDownList ID="ddlConnStr" runat="server"></asp:DropDownList>
				    <div class="clear"></div>
                </div>
            </div>
        </div>
        <div class="clear"></div>
        <h2>Status</h2>
        <div class="Section">
            <div class="Controls">
                <div class="formRow">
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh" BackColor="#474747" ForeColor="White" BorderWidth="0" Width="100px" Height="30px" />
                </div>
                <div class="formRow">
                    <asp:Repeater ID="repJobs" runat="server">
                        <HeaderTemplate>
                            <table style="width:100%">
                            <thead style="background-color:#eaeaea">
                            <td>Job</td>
                            <td>Category</td>
                            <td>Status</td>
                            <td>Processed</td>
                            <td>Priority</td>
                            <td>Start Time</td>
                            </thead>
                        </HeaderTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                        <ItemTemplate>
                            <tr style="background-color:#fff;">
                                <td>
                                    <%# StringUtil.Clip((Container.DataItem as Job).Name, 50, true) %>
                                </td>
                                <td>
                                    <%# StringUtil.Clip((Container.DataItem as Job).Category, 50, true) %>
                                </td>
                                <td>
                                    <%# (Container.DataItem as Job).Status.State %>
                                </td>
                                <td>
                                    <%# (Container.DataItem as Job).Status.Processed %> /
                                    <%# (Container.DataItem as Job).Status.Total %>
                                </td>
                                <td>
                                    <%# (Container.DataItem as Job).Options.Priority %>
                                </td>
                                <td>
                                    <%# (Container.DataItem as Job).QueueTime.ToLocalTime() %>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
        <div class="clear"></div>
        <h2>Results</h2>
        <div class="Section">
            <div class="Controls">
			    <div class="formRow">
                    <asp:Button ID="btnImport" Text="Run Import" OnClick="btnImport_Click" runat="server" BackColor="#474747" ForeColor="White" BorderWidth="0" Width="100px" Height="30px"  />
					<asp:Button ID="btnMediaImport" Text="Run Media Import" OnClick="btnMediaImport_Click" runat="server" BackColor="#474747" ForeColor="White" BorderWidth="0" Width="120px" Height="30px"  />
                    <asp:Button ID="btnPostImport" Text="Run Post Import Cleanup" OnClick="btnPostImport_Click" runat="server" BackColor="#474747" ForeColor="White" BorderWidth="0" Width="150px" Height="30px" />
                </div>
                <div class="rowSpacer"></div>
                <div class="formRow">
                    <div class="rowTitle">Messages:</div>
                    <div class="Message">
                        <asp:TextBox ID="txtMessage" TextMode="MultiLine" runat="server"></asp:TextBox>
		            </div>
                    <div class="clear"></div>
                </div>
                <div class="clear"></div>
            </div>
        </div>
    </div>
    </form>
</body>
</html>
