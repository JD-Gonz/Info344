$(function(){

    function UseJSON() {
        var num = $("#num").val();
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/GetNumbersThatStartsWith",
            data: "{input:" + num + "}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                $("#result").html(JSON.stringify(msg));
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("error");
            }
        });
    }
}); //doc ready()