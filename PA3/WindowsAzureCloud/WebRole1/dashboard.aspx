<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="dashboard.aspx.cs" Inherits="WebRole1.dashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
<link rel="stylesheet" href="Styles.css"/>

    <title>Simple Web Service</title>

</head>

<body>
    <form id="form1" runat="server">
    <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference path="SimpleWebService.asmx" />
                </Services>
            </asp:ScriptManager>
            <div>
                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />

                <h2>Simple Web Service</h2>
                    <input id="EnteredValue" type="text" />
                    <input id="EchoButton" type="button" value="Echo" onclick="EchoUserInput()" />
            </div>
        </form>

        <hr/>

        <div>
            <span id="Results"></span>
        </div>   
        <script src="JavaScript.js"></script>
    </body>
</html>
