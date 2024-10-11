"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you
    // should be aware of possible script injection concerns
    console.log(message)
    li.textContent = `${message}`;
});
    
connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    console.log('connected')
}).catch(function (err) {
    return console.error(err.toString());
});


function sendMessages(room) {
    var message = document.getElementById("message").value;
    console.log(room)

    connection.invoke("SendMessage", room, message).catch(function (err) {
        return console.error(err.toString());
    });
}


//document.getElementById("joinRoomButton").addEventListener("click", function (event) {
//    var room = document.getElementById("roomInput").value;
//    var user = document.getElementById("userInput").value;
//    console.log(room)
//    connection.invoke("JoinRoom", room, user).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    console.log(event);
//    var user = document.getElementById("userInput").value;
//    var room = document.getElementById("roomInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", room, user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});