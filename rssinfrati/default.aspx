<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="RSSInfraTI.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>InfraTI - Informações</title>
    <link href="RSS.css" rel="stylesheet" />
    <link rel="shortcut icon" type="image/x-icon" href="~/favicon.ico" />
    <meta http-equiv="refresh" content="10;" />
</head>
<body>
     <div id="inicio"><h4>RSS Feed</h4></div>
    <form id="form1" runat="server">
         <div style="max-height:3000px; overflow:auto">
        <asp:GridView ID="gvRss" runat="server" AutoGenerateColumns="false" ShowHeader="false" Width="100%">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <table width="100%" border="0" cellpadding="0" cellspacing="5" height="20%">
                            <tr>
                                <td>
                                    <h3 style="color:#3E7CFF"><%#Eval("Title") %></h3>
                                </td>
                                <td width="350px" style="font-weight:bold; font-size:18px; text-align:right;">
                                    <%#Eval("PublishDate") %>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <hr />
                                    <%#Eval("Description") %>
                                </td>
                            </tr>
                         
                        </table>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    </form>
</body>
</html>

