

// This function calls the Web Service method.  
function EchoUserInput() {
    var echoElem = document.getElementById("EnteredValue");
    Samples.AspNet.SimpleWebService.EchoInput(echoElem.value,
        SucceededCallback);
}

// This is the callback function that 
// processes the Web Service return value.
function SucceededCallback(result) {
    var RsltElem = document.getElementById("Results");
    RsltElem.innerHTML = result;
}
