"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString());
});


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
    chatContentDiv.scrollTo({
        top: chatContentDiv.scrollHeight,
        behavior: 'smooth'
    });
});

connection.on("NotificationMessage", (notification, message, senderName) => {

    document.getElementById('toastMessageContent').innerText = `${senderName}: ${message.content}`;
    document.getElementById('messageFrom').innerText = `New message from ${senderName.toUpperCase()}`;

    const toastElement = document.getElementById('liveToast');
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    //mark unread on user chat
    const userChat = document.getElementById(`${notification.senderId.toLowerCase()}`);
    if (userChat.getAttribute('is-open') == 'false') {
        userChat.classList.add('new-message');
    }
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

connection.on("userConnection", (usersOnline) => { 
    const allChatUsers = document.querySelectorAll('.chat-user');
    
    allChatUsers.forEach(user => {
        const id = user.getAttribute('id').toUpperCase();

        usersOnline.forEach(userOnline => { 
            if (userOnline.value.userId == id) {
                const userOnlineEle = user.querySelector('.user-online');

                userOnlineEle.classList.add('active-user-online');
            }
        })
    });
});

connection.on("userDisconnect", (userIdDisconnect) => {
    const allChatUsers = document.querySelectorAll('.chat-user');

    allChatUsers.forEach(user => {
        const id = user.getAttribute('id').toUpperCase();

        if (userIdDisconnect == id) {
            const userOnlineEle = user.querySelector('.user-online');
            userOnlineEle.classList.remove('active-user-online');
        }
    });
});


function sendMessages(room, toUserId, event) {
    event.preventDefault()
    var message = document.getElementById("messageInput");

    connection.invoke("SendMessage", `${room}`, message.value, toUserId.toUpperCase()).then(() => {
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

function openChatRoom(userId, userName, currentUserId) {
    const allChatUsers = document.querySelectorAll('.chat-user');
    const userChat = document.getElementById(`${userId}`);
    
     if (userChat.getAttribute('is-open') == 'false') {

        allChatUsers.forEach(user => {
            user.classList.remove('active');
            user.setAttribute('is-open', false);
        });

        userChat.setAttribute('is-open', true);
        userChat.classList.remove('new-message');

        const clickedUserChat = event.currentTarget;
        clickedUserChat.classList.add('active');

        fetch('api/ChatApi/FindOrCreateChatRoom', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ UserId: userId }),
        }).then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
            .then(data => {

                const chatContentDiv = document.getElementById('chat-content');
                const chatRoom = document.querySelector('.chat-room');
                const chatInput = document.querySelector('.chat-input');

                // Clear Chat content (if any)
                chatContentDiv.innerHTML = '';
                chatInput.innerHTML = '';

                if (!data.messages || data.messages.length === 0) {

                    const html = `
                        <div class="user-joined">
                            <p><b>No messages yet</b></p>
                        </div>
                    `;

                    const chatContentDiv = document.getElementById('chat-content');
                    const divElement = document.createElement('div');
                    divElement.innerHTML = html;
                    chatContentDiv.appendChild(divElement);
                } else {
                    const chatWith = document.querySelector('.chat-with');
                    chatWith.innerHTML = `Chat with ${userName}`;

                    // Add each message to chatContentDiv
                    data.messages.forEach(message => {
                        const messageElement = document.createElement('div');
                        messageElement.classList.add('chat-message');

                        messageElement.innerHTML = `
                            <div class="message ${message.senderId == currentUserId ? "sent-by-me" : ""}">
                                <p>${message.content}</p>
                                <span>${new Date(message.createdAt).toLocaleString()}</span>
                            </div>
                        `;

                        chatContentDiv.appendChild(messageElement);
                    });

                    // Scroll to the bottom
                    chatContentDiv.scrollTop = chatContentDiv.scrollHeight;
                }

                // HTML nhập tin nhắn
                chatInput.innerHTML += `
                    <div>
                        <form id="form-message" class="message-input" action="#" onsubmit="sendMessages(${data.chatRoom.id}, '${userId}', event)">
                            <input type="text" id="messageInput" placeholder="Type a message..." />
                            <button type="submit" id="sendMessageButton" onclick="sendMessages(${data.chatRoom.id}, '${userId}', event)">Send</button>
                        </form>
                    </div>
                `;

                joinRoom(data.chatRoom.id);
            })

    }
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