if (!String.format) {
    String.format = function(format) {
        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function(match, number) {
            return typeof args[number] != 'undefined'
                    ? args[number]
                    : match
                ;
        });
    };
}

function removeEventPrefix(start, end, ev){
    if(ev.text.indexOf('#*') !== -1){
        return ev.text.split('#*')[1];
    }

    return ev.text;
}

function getAbteilungNameByEventText(text){
    if(text.indexOf('#*') !== -1){
        var filtered = abteilungsList.filter(function(value) {
            return text.startsWith(value.calendarId);
        });

        return filtered.length === 1 ? filtered[0].caption : '';
    }

    return '';
}

function getAbteilungBadgeByEventText(text){
    if(text.indexOf('#*') !== -1){
        var filtered = abteilungsList.filter(function(value) {
            return text.startsWith(value.calendarId);
        });

        return filtered.length === 1 ? filtered[0].badge : '';
    }

    return '';
}

var abteilungsList = [];

function init() {

    var xobj = new XMLHttpRequest();
    xobj.overrideMimeType("application/json");
    xobj.open('GET', 'abteilungen.json', true); // Replace 'my_data' with the path to your file
    xobj.onreadystatechange = function () {
        if (xobj.readyState == 4 && xobj.status == "200") {
            // Required use of an anonymous callback as .open will NOT return a value but simply returns undefined in asynchronous mode
            abteilungsList = JSON.parse(xobj.responseText);
        }
    };
    xobj.send(null);
}
