<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="WebRole.Dashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
<link rel="stylesheet" href="Styles.css"/>

    <title>Crawler Dashboard</title>

</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager runat="server" ID="scriptManager">
                <Services>
                    <asp:ServiceReference path="WebService.asmx" />
                </Services>
            </asp:ScriptManager>
            <div>
                <h1>Crawler Dashboard</h1>
                <h3>Crawler Controls</h3>
                <input id="website" type="text" placeholder="www.cnn.com"/>
                <br />
                <input id="startButton" type="button" value="Start" onclick="startCrawling()" />
                
                <div>
                    <div id="queryMessage"></div>
                    <h2>Crawler Status</h2>
                    <div id="commandMessage"></div>
                    <div id="statusMessage"></div>
                </div>

                <input id="stopButton" type="button" value="Stop / Clear" onclick="stopCrawling()" />

                <hr/>
                <h3>Titles Loaded Into Trie</h3>
                <div id="trieMessege"></div>

                <h3>Last Title Loaded Into Trie</h3>
                <div id="lastTrieMessege"></div>


                <h3>RAM Available And CPU Utilization%</h3>
                <div id="performanceMessage"></div>

                <h3>Urls Waiting To Be Crawled</h3>
                <div id="queueMessage"></div>

                <h3>HTML Pages Saved</h3>
                <div id="tableMessage"></div>

                <h3># Of URLs Crawled</h3>
                <div id="crawledMessage"></div>

                <h3>Last 10 URLs Crawled</h3>
                <div id="lastTenMessage"></div>

                <h3>Error Log</h3>
                <div id="errorMessage"></div>
                   
            </div>
        </form>  
        <script src="JavaScript.js"></script>
    </body>
</html>
