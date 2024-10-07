
    $(document).ready(function () {
        var callToActionTexts = ["Hello! How can i help you today?", "Create an Order?", "Find an Office Address"];
    function shuffleArray(array) {
            for (let i = array.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
            }
        }
    shuffleArray(callToActionTexts);
    $("#idlePrompts").text(callToActionTexts[0]);
    });
    const urlParams = new URLSearchParams(window.location.search);
    const userName = urlParams.get('userName');
    var xhr = new XMLHttpRequest();
    xhr.open('GET', window.encodeURI("/api/token/fetch?id=Sudarshan"));
    xhr.send();
    xhr.onreadystatechange = function () { var x = this; m = x.m; processRequest(x, m) };


    function processRequest(x, m) {
        if (x.readyState == 4 && x.status == 200 && x.getResponseHeader("Content-Type") == 'application/json; charset=utf-8') {
            var response = JSON.parse(x.responseText);
    var cookieValue = validateString(getCookie("NAWC_SSID"));
    if (cookieValue == '' || cookieValue == 'null' || cookieValue == 'undefined')
    setCookie("NAWC_SSID", window.encodeURIComponent(validateString(response.conversationId)));
    document.getElementById("bot").src = window.encodeURI('/chatbot/index?token=' + window.encodeURIComponent(validateString(response.token)));
        }
    }

    function hide() {
        document.getElementById("botsection").style.display = "none";
    document.getElementById('boticon').style.display = "block";
    }

    function show() {
        document.getElementById("botsection").style.display = "block";
    document.getElementById('boticon').style.display = "none";

    }

    function setCookie(cname, cvalue) {
        cvalue = validateString(cvalue);
    const d = new Date();
    d.setTime(d.getTime() + (15 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = window.encodeURIComponent(cname) + "=" + window.encodeURIComponent(cvalue) + ";" + expires + ";path=/";
    }

    function validateString(input) {
        var reg = /[^-a-zA-ZÀ-ÿ0-9\s_.]/;
    if (input != "" && reg.test(input)) {
            return "Invalid";
        }
    return input;
    }

    function getCookie(cname) {
        let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
    while (c.charAt(0) == ' ') {
        c = c.substring(1);
            }
    if (c.indexOf(name) == 0) {
                return c.substring(name.length, c.length);
            }
        }
    return "";
    }
