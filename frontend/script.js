const chordRegex = /\[([^\]]+)\]/g;

function GetChords(text) {
    var chords = text.match(chordRegex);
    if(!chords) return ""
    chords = chords.filter(function(item, pos) {
        return chords.indexOf(item) == pos;
    })
    var text = ""
    if(chords) {
        chords.forEach(function(chord) {
            text += chord.substring(1, chord.length-1) + " ";
        });
    }
    return text;
}

function scrollToSection(sectionId) {
    var section = document.getElementById(sectionId)
    section.scrollIntoView()
}

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}


function TextBoxError(id, text) {
    ChangeTextBoxProperty(id, "#EE0000", text)
}

function TextBoxText(id, text) {
    ChangeTextBoxProperty(id, "#03cffc", text)
}

function TextBoxGood(id, text) {
    ChangeTextBoxProperty(id, "#00EE00", text)
}

function HideTextBox(id) {
    document.getElementById(id).style.visibility = "hidden"
}

function ChangeTextBoxProperty(id, color, innerHtml) {
    var text = document.getElementById(id)
    text.style.visibility = "visible"
    text.style.border = color + " 1px solid"
    text.innerHTML = innerHtml
}
function SafeFormat(text) {
    var d = document.createElement("div")
    d.innerText = text
    return d.innerHTML
}

function tfetch(url, method = "GET", body = "") {
    return ifetch(url, false, method, body)
}

function jfetch(url, method = "GET", body = "") {
    return ifetch(url, true, method, body)
}

function ifetch(url, asjson = true, method = "GET", body = "") {
    return new Promise((resolve, reject) => {
        if(method == "GET" || method == "HEAD") {
            fetch(url, {
                method: method,
                headers: {
                    "token": localStorage.token
                }
            }).then(r => {
                r.text().then(res => {
                    if(asjson) {
                        try {
                            if(r.status != 200) reject(JSON.parse(res))
                            else resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        if(r.status != 200) reject(res)
                        else resolve(res)
                    }
                })
            })
        } else {
            fetch(url, {
                method: method,
                body: body,
                headers: {
                    "token": localStorage.token
                }
            }).then(r => {
                
                r.text().then(res => {
                    if(asjson) {
                        try {
                            if(r.status != 200) reject(JSON.parse(res))
                            else resolve(JSON.parse(res))
                        } catch(e) {
                            reject(e)
                        }
                    } else {
                        if(r.status != 200) reject(res)
                        else resolve(res)
                    }
                })
            })
        }
    })
}