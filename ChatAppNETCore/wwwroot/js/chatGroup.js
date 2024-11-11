"use strict";
const userCreateGroup = [];

function sendMessagesGroup(room, event) {
    event.preventDefault()
    var message = document.getElementById("messageInput");

    connection.invoke("SendMessage", `${room}`, message.value, null).then(() => {
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


function addUserToGroupCreate(userId, userName) {
    const selectedUserList = document.querySelector('.selected-user-list');
    const userChecked = document.getElementById(`checkbox-${userId}`);

    if (!userChecked.checked) {
        const userSelected = document.createElement('div');
        userSelected.classList.add('user-select');
        userSelected.setAttribute('id', userId);

        userSelected.innerHTML = `
            <div class="d-flex">
                <img width="30" height="30" src="/images/avatar-default.jpg" alt="Avatar default" />
                <p>${userName}</p>
            </div>
            <div class="delete-user" onclick="removeUserFromGroupSelected('${userId}')">
                <i class="fa-regular fa-circle-xmark"></i>
            </div>
        `;

        selectedUserList.appendChild(userSelected);
        userCreateGroup.push(userId.toString());

        console.log(userId.toString())
    } else {
        removeUserFromGroupSelected(userId);
    }
}

function removeUserFromGroupSelected(userId) {
    const selectedUserList = document.querySelectorAll('.user-select');

    selectedUserList.forEach((item) => {
        if (item.getAttribute('id') == userId) {
            item.remove();
            const userChecked = document.getElementById(`checkbox-${userId}`);
            userChecked.checked = false;
        }
    })
}

async function createGroup() {
    userCreateGroup.push(currentUserId); 

    if (userCreateGroup.length >= 3) {
        const groupName = (Math.random() + 1).toString(36).substring(7);
        const getGroupName = document.querySelector('.input-group-name');

        const group = {
            GroupName: getGroupName.value ? getGroupName.value : groupName,
            Members: userCreateGroup
        }

        await fetch('api/ChatApi/CreateGroup', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(group),
        }).then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        }).then(group => {
            console.log(group);
        })

    } else {
        toastr.error("Group must have 3 or more members");
    }
}

async function openGroup(groupId) {
    const allChatGroups = document.querySelectorAll('.chat-group');

    await fetch(`api/ChatApi/GetMessageGroup/${groupId}`)
        .then((response) => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        }) 
        .then(group => {
            const chatContentDiv = document.getElementById('chat-content');
            const chatInput = document.querySelector('.chat-input');
            const chatTitle = document.querySelector('.chat-with');

            chatContentDiv.innerHTML = '';
            chatInput.innerHTML = '';
            chatTitle.innerHTML = group.chat.groupName;

            if (!group.messages || group.messages.length === 0) {

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
                renderMessage(group.messages);
            }

            // HTML nhập tin nhắn
            chatInput.innerHTML += `
                <div>
                    <form id="form-message" class="message-input" action="#" onsubmit="sendMessagesGroup(${group.chat.id}, event)">
                        <input type="text" id="messageInput" placeholder="Type a message..." />
                        <button type="submit" id="sendMessageButton" onclick="sendMessagesGroup(${group.chat.id}, event)">Send</button>
                    </form>
                </div>
            `;

            joinRoom(group.chat.id);
        })
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


const modalCreateGroup = document.querySelector('.create-group-model');
function openModelCreateChat() {

    modalCreateGroup.style.top = '50%';
    modalCreateGroup.style.opacity = '1';
    modalCreateGroup.style.display = 'block';

    onOverlay();
}


function onOverlay() {
    document.getElementById("overlay").style.display = "block";
}

function offOverlay() {
    document.getElementById("overlay").style.display = "none";
    modalCreateGroup.style.right = '70%';
    userList.style.opacity = '0';
    modalCreateGroup.style.display = 'none';
}