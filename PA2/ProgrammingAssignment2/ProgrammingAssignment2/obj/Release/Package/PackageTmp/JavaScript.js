function UseJSON() {
    var text = $("#text").val();
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
            for(idx = 0; idx < results.length; ++idx) {
                instance = template.clone();
                instance.find('.result').html(results[idx]);
                instance.removeClass('template');
                suggestions.fadeIn(1000).append(instance);
            }
        },
        error: function (data) {
            alert("error");
        }
    });
}

$(function () {



}); //doc ready()