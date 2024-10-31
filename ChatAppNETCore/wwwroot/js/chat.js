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

    // Mark unread on user chat
    const userChat = document.getElementById(`${notification.senderId.toLowerCase()}`);
    if (userChat.getAttribute('is-open') == 'false') {
        userChat.classList.add('new-message');

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


function sendMessages(room, toUserId, event) {
    event.preventDefault()
    var message = document.getElementById("messageInput");

    connection.invoke("SendMessage", `${room}`, message.value, toUserId.toUpperCase()).then(() => {
        const chatContentDiv = document.getElementById('chat-content');
        chatContentDiv.scrollTo({
            top: chatContentDiv.scrollHeight,
            behavior: 'smooth'
        });

        const userChat = document.getElementById(`${toUserId}`);
        userChat.querySelector('.message-child').innerHTML = `You: ${message.value}`;

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


async function markAsRead(chatId, userChat) {

    try {
        await fetch(`api/chatApi/MarkAsRead/${chatId}`)
            .then(response => {
                return response.json();
            }) 
            .then(data => {
                const notificationDotRed = userChat.querySelector('.notification');
                notificationDotRed.textContent = '';
                userChat.setAttribute('is-open', true);
                userChat.classList.remove('new-message');
            })
    } catch (error) {
        console.error('Error fetching message:', error);
    }
}

async function openChatRoom(chatId, userId, userName, myId) {
    const allChatUsers = document.querySelectorAll('.chat-user');
    const userChat = document.getElementById(`${userId}`);

    if (userChat.getAttribute('is-open') == 'false') {
        markAsRead(chatId, userChat);

        allChatUsers.forEach(user => {
            user.classList.remove('active');
            user.setAttribute('is-open', false);
        });

        const clickedUserChat = event.currentTarget;
        clickedUserChat.classList.add('active');


        try {
            await fetch(`/api/ChatApi/GetMessages/${chatId}`)
                .then(response => {
                    return response.json();
                })
                .then(messages => {
                    const chatContentDiv = document.getElementById('chat-content');
                    const chatInput = document.querySelector('.chat-input');
                    const chatWith = document.querySelector('.chat-with');


                    // Clear Chat content (if any)
                    chatContentDiv.innerHTML = '';
                    chatInput.innerHTML = '';
                    chatWith.innerHTML = `Chat with ${userName}`;

                    if (!messages || messages.length === 0) {

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
                        // Add each message to chatContentDiv
                        renderMessage(messages);
                    }

                    // HTML nhập tin nhắn
                    chatInput.innerHTML += `
                        <div>
                            <form id="form-message" class="message-input" action="#" onsubmit="sendMessages(${chatId}, '${userId}', event)">
                                <input type="text" id="messageInput" placeholder="Type a message..." />
                                <button type="submit" id="sendMessageButton" onclick="sendMessages(${chatId}, '${userId}', event)">Send</button>
                            </form>
                        </div>
                    `;

                    joinRoom(chatId);
                })

        } catch (error) {
            console.error('Error fetching message:', error);
        }
    }
}

function renderMessage(messages) {
    const chatContentDiv = document.getElementById('chat-content');

    messages.forEach(message => {
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

async function createChat(userId) {
    try {
        await fetch('api/ChatApi/CreateChatRoom', {
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
        }).then(chat => {
            if (chat.isNewChat) {
                addNewChatToList(chat);
            }
            offOverlay();
            createChatRoom(chat);
        })
    } catch (error) {
        console.error('Error create chat:', error);
    }
}

function createChatRoom(chat) {
    const chatContentDiv = document.getElementById('chat-content');
    const chatInput = document.querySelector('.chat-input');
    const chatWith = document.querySelector('.chat-with');
    chatWith.innerHTML = `Chat with ${chat.partner.name}`;

    // Clear Chat content (if any)
    chatContentDiv.innerHTML = '';

    if (chat.isNewChat) {
        const html = `
            <div class="user-joined">
                <p><b>${chat.partner.name} is waiting for your message!</b></p>
            </div>
        `;
        const divElement = document.createElement('div');
        divElement.innerHTML = html;
        chatContentDiv.appendChild(divElement);
    } else {
        renderMessage(chat.messages);
    }

    chatInput.innerHTML = '';
    chatInput.innerHTML += `
        <div>
            <form id="form-message" class="message-input" action="#" onsubmit="sendMessages(${chat.id}, '${chat.partner.id}', event)">
                <input type="text" id="messageInput" placeholder="Type a message..." />
                <button type="submit" id="sendMessageButton" onclick="sendMessages(${chat.id}, '${chat.partner.id}', event)">Send</button>
            </form>
        </div>  
    `;

    joinRoom(chat.id);
}

function addNewChatToList(chat) {
    const allChatUsers = document.querySelectorAll('.chat-user');
    const listChat = document.querySelector('.list-chat');

    //remove previous active
    allChatUsers.forEach(user => {
        user.classList.remove('active');
        user.setAttribute('is-open', false);
    });

    const newChatItem = document.createElement('li');
    newChatItem.classList.add('chat-user', 'active');
    newChatItem.id = chat.partner.id;
    newChatItem.setAttribute('is-open', 'true');
    newChatItem.setAttribute('onclick', `openChatRoom('${chat.id}', '${chat.partner.id}', '${chat.partner.name}', '${currentUserId}')`);

    newChatItem.innerHTML = `
        <div class="info">
            <div class="position-relative">
                <img width="50" height="50" src="/images/avatar-default.jpg" alt="Avatar default" />
                <div class="user-online"></div>
            </div>
            <p class="px-2">${chat.partner.name}</p>
        </div>
        <div class="dot-red"></div>
    `;

    listChat.appendChild(newChatItem);
}

const userList = document.querySelector('.user-list');

function openSelectUser() {
    userList.style.right = '0';
    userList.style.opacity = '1';
    onOverlay();
}

function onOverlay() {
    document.getElementById("overlay").style.display = "block";
}

function offOverlay() {
    document.getElementById("overlay").style.display = "none";
    userList.style.right = '-400px';
    userList.style.opacity = '0';
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