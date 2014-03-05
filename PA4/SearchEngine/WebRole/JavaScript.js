setInterval(function () {
    crawlerStatus();
    performanceCounters();
    crawledNum();
    lastTenUrls();
    queueSize();
    tableSize();
    errorLog();
}, 1000);

function SearchTrie() {
    var text = $("#text").val();
    if (text == "")
        $('.suggestions').hide();
    else {
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/querySuggestions",
            data: '{word:"' + text + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                var idx;
                var template = $('.template');
                var suggestions = $('.suggestions');
                suggestions.empty();
                var results = JSON.parse(data["d"]);
                if (results == 0) {
                    instance = template.clone();
                    instance.find('.result').html("Sorry, No Suggestions Were Found");
                    instance.removeClass('template');
                    suggestions.fadeIn(500).append(instance);
                }
                for (idx = 0; idx < results.length; ++idx) {
                    instance = template.clone();
                    instance.find('.result').html(results[idx]);
                    instance.removeClass('template');
                    suggestions.fadeIn(500).append(instance);
                }
            },
            error: function (data) {
                $('.suggestions').empty();
            }
        });
        $('.suggestions').show();
    }
}

function startCrawling() {
    var elem = document.getElementById("website");
    WebRole1.admin.StartCrawling(elem.value, startCallback);
}

function startCallback(result) {
    var RsltElem = document.getElementById("commandMessage");
    RsltElem.innerHTML = result;
}

function retrieveWebsite() {
    var elem = document.getElementById("website");
    WebRole1.admin.GetPageTitle(elem.value, websiteCallback);
}

function websiteCallback(result) {
    var RsltElem = document.getElementById("queryMessage");
    RsltElem.innerHTML = result;
}

function stopCrawling() {
    WebRole1.admin.StopCrawling(
        stopCallback);
}

function stopCallback(result) {
    var RsltElem = document.getElementById("commandMessage");
    RsltElem.innerHTML = result;
}

function crawlerStatus() {
    WebRole1.admin.CrawlerStatus(statusCallback);
}

function statusCallback(result) {
    var RsltElem = document.getElementById("statusMessage");
    RsltElem.innerHTML = result;
}

function performanceCounters() {
    WebRole1.admin.MachineCounters(
        performanceCallback);
}

function performanceCallback(result) {
    var RsltElem = document.getElementById("performanceMessage");
    RsltElem.innerHTML = result;
}

function crawledNum() {
    WebRole1.admin.numberOfURLsCrawled(crawledCallback);
}

function crawledCallback(result) {
    var RsltElem = document.getElementById("crawledMessage");
    RsltElem.innerHTML = result;
}

function lastTenUrls() {
    WebRole1.admin.LastTenURLsCrawled(lastTenCallback);
}

function lastTenCallback(result) {
    var RsltElem = document.getElementById("lastTenMessage");
    var res = result
    RsltElem.innerHTML = res;
}

function queueSize() {
    WebRole1.admin.SizeOfQueue(queueSizeCallback);
}

function queueSizeCallback(result) {
    var RsltElem = document.getElementById("queueMessage");
    RsltElem.innerHTML = result;
}

function tableSize() {
    WebRole1.admin.SizeOfTable(tableSizeCallback);
}

function tableSizeCallback(result) {
    var RsltElem = document.getElementById("tableMessage");
    RsltElem.innerHTML = result;
}

function errorLog() {
    WebRole1.admin.Errors(errorLogCallback);
}

function errorLogCallback(result) {
    var RsltElem = document.getElementById("errorMessage");
    RsltElem.innerHTML = result;
}

$(function () {

}); //doc ready()
