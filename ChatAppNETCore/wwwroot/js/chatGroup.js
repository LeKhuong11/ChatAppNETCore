"use strict";
const userCreateGroup = [];


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
    console.log(userCreateGroup)
    if (userCreateGroup.length >= 3) {
        const groupName = (Math.random() + 1).toString(36).substring(7);

        const group = {
            GroupName: groupName,
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