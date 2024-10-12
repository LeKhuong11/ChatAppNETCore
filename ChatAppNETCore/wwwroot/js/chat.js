"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


connection.on("ReceiveMessage", (message) => {
    const chatContentDiv = document.getElementById('chat-content');
    const messageElement = document.createElement('div');
    messageElement.classList.add('chat-message');

    messageElement.innerHTML = `
        <div class="message ${message.senderId == currentUserId ? 'sent-by-me' : ''}">
            <p>${message.content}</p>
            <span>${new Date(message.createdAt).toLocaleString()}</span>
        </div>
    `;

    chatContentDiv.appendChild(messageElement);
});

connection.on("JoinRoomMessage", (userName, userId) => {
    const html = `
        <div class="user-joined">
            <p><b>${userId == currentUserId ? "You" : userName}</b> has joined the room</p>
        </div>
    `;

    const chatContentDiv = document.getElementById('chat-content');

    const divElement = document.createElement('div');
    divElement.innerHTML = html;

    chatContentDiv.appendChild(divElement);
    chatContentDiv.scrollTo({
        top: chatContentDiv.scrollHeight,
        behavior: 'smooth'
    });
})
    
connection.start().then(function () {
    console.log('connected')
}).catch(function (err) {
    return console.error(err.toString());
});

    
function sendMessages(room) {
    var message = document.getElementById("messageInput");

    connection.invoke("SendMessage", `${room}`, message.value).then(() => {
        const chatContentDiv = document.getElementById('chat-content');
        chatContentDiv.scrollTo({
            top: chatContentDiv.scrollHeight,
            behavior: 'smooth' 
        });
        message.value = "";
    }).catch(function (err) {
        return console.error(err.toString());
    });
}


function joinRoom(room) {
    connection.invoke("JoinRoom", `${room}`).then(() => {
       
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

//document.getElementById("joinRoomButton").addEventListener("click", function (event) {
//    console.log("room")
//    var room = document.getElementById("roomInput").value;
//    var user = document.getElementById("userInput").value;
//    connection.invoke("JoinRoom", room).catch(function (err) {
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