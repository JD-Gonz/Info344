setInterval(function () {
    crawlerStatus();
    performanceCounters();
    crawledNum();
    lastTenUrls();
    queueSize();
    tableSize();
    errorLog();
    trieSize();
    lastLoaded();
}, 1000);

function SearchTrie() {
    var text = $("#text").val();
    if (text == "")
        $('.suggestions').hide();
    else {
        $.ajax({
            type: "POST",
            url: "WebService.asmx/querySuggestions",
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
    WebRole.WebService.StartCrawling(elem.value, startCallback);
}

function startCallback(result) {
    var RsltElem = document.getElementById("commandMessage");
    RsltElem.innerHTML = result;
}
/*
function retrieveWebsites() {
    var elem = document.getElementById("website");
    WebRole.WebService.GetPageTitle(elem.value, websiteCallback);
}

function websiteCallback(result) {
    var RsltElem = document.getElementById("queryMessage");
    RsltElem.innerHTML = result;
}
*/
function stopCrawling() {
    WebRole.WebService.StopCrawling(
        stopCallback);
}

function stopCallback(result) {
    var RsltElem = document.getElementById("commandMessage");
    RsltElem.innerHTML = result;
}

function crawlerStatus() {
    WebRole.WebService.CrawlerStatus(statusCallback);
}

function statusCallback(result) {
    var RsltElem = document.getElementById("statusMessage");
    RsltElem.innerHTML = result;
}

function performanceCounters() {
    WebRole.WebService.MachineCounters(performanceCallback);
}

function performanceCallback(result) {
    var RsltElem = document.getElementById("performanceMessage");
    RsltElem.innerHTML = result;
}

function crawledNum() {
    WebRole.WebService.numberOfURLsCrawled(crawledCallback);
}

function crawledCallback(result) {
    var RsltElem = document.getElementById("crawledMessage");
    RsltElem.innerHTML = result;
}

function lastTenUrls() {
    WebRole.WebService.LastTenURLsCrawled(lastTenCallback);
}

function lastTenCallback(result) {
    var RsltElem = document.getElementById("lastTenMessage");
    var res = result
    RsltElem.innerHTML = res;
}

function queueSize() {
    WebRole.WebService.SizeOfQueue(queueSizeCallback);
}

function queueSizeCallback(result) {
    var RsltElem = document.getElementById("queueMessage");
    RsltElem.innerHTML = result;
}

function tableSize() {
    WebRole.WebService.SizeOfTable(tableSizeCallback);
}

function tableSizeCallback(result) {
    var RsltElem = document.getElementById("tableMessage");
    RsltElem.innerHTML = result;
}

function errorLog() {
    WebRole.WebService.Errors(errorLogCallback);
}

function errorLogCallback(result) {
    var RsltElem = document.getElementById("errorMessage");
    RsltElem.innerHTML = result;
}

function trieSize() {
    WebRole.WebService.trieCount(trieCallback);
}

function trieCallback(result) {
    var RsltElem = document.getElementById("trieMessege");
    RsltElem.innerHTML = result;
}

function lastLoaded() {
    WebRole.WebService.lastLine(lastCallback);
}

function lastCallback(result) {
    var RsltElem = document.getElementById("lastTrieMessege");
    RsltElem.innerHTML = result;
}

$(function () {

    // $('.search-button').click(retrieveWebsites());

}); //doc ready()
