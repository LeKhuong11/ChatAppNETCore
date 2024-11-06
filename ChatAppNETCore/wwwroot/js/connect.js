"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.start()
    .catch(function (err) {
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

connection.on("NotificationMessage", (notification, message, senderName, messagesUnRead) => {

    document.getElementById('toastMessageContent').innerText = `${senderName}: ${message.content}`;
    document.getElementById('messageFrom').innerText = `New message from ${senderName.toUpperCase()}`;

    const toastElement = document.getElementById('liveToast');
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Mark unread on user chat
    const userChat = document.getElementById(`${notification.senderId.toLowerCase()}`);
    if (userChat.getAttribute('is-open') == 'false') {
        userChat.classList.add('new-message');
        userChat.querySelector('.notification').innerHTML = messagesUnRead;
    }
    // Update message content in message child
    userChat.querySelector('.message-child').innerHTML = `${notification.ReceiveId == currentUserId ? 'You: ' + message.content : message.content}`;
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