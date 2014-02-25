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
                    <asp:ServiceReference path="admin.asmx" />
                </Services>
            </asp:ScriptManager>
            <div>
                <h2>Start Crawling</h2>
                <input id="website" type="text" placeholder="www.cnn.com"/>
                <input id="startButton" type="button" value="Start" onclick="startCrawling()" />
                <span id="startMessage"></span>

                <h2>Retrieve Name of Website</h2>
                <input id="url" type="text" placeholder="Insert URL"/>
                <input id="queryButton" type="button" value="Search" onclick="retrieveWebsite()" />
                <span id="queryMessage"></span>

                <h2>Stop Crawler</h2>
                <input id="stopButton" type="button" value="Stop" onclick="stopCrawling()" />
                <span id="stopMessage"></span>

                <h2>Clear Everything</h2>
                <input id="clearButton" type="button" value="Clear" onclick="clearAll()" />
                <span id="clearMessage"></span>

                <h2>Crawler Status</h2>
                <span id="statusMessage"></span>

                <hr/>

                <h2>RAM Available And CPU Utilization%</h2>
                <span id="performanceMessage"></span>

                <h2>Urls Waiting To Be Crawled</h2>
                <span id="queueMessage"></span>

                <h2>HTML Pages Saved</h2>
                <span id="tableMessage"></span>

                <h2># Of URLs Crawled</h2>
                <span id="crawledMessage"></span>

                <h2>Last 10 URLs Crawled</h2>
                <span id="lastTenMessage"></span>

                <h2>Error Log</h2>
                <span id="errorMessage"></span>
                   
            </div>
        </form>  
        <script src="JavaScript.js"></script>
    </body>
</html>
