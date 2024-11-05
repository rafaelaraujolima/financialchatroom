"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/chatroomhub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

const messages = [];

connection.on("ReceiveMessage", function (user, message, dateTime) {
    const utcDate = new Date(dateTime);

    const localDate = new Date(utcDate.getTime() - utcDate.getTimezoneOffset() * 60000);

    const localDateTime = localDate.toLocaleString([], {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });

    messages.push({ user, message, localDateTime });
    messages.sort((t1, t2) => new Date(t2.localDateTime) - new Date(t1.localDateTime));

    if (messages.length > 50) {
        messages.pop();
    }

    displayMessages();

    //let li = document.createElement("li");
    //document.getElementById("messagesList").appendChild(li);
    //li.textContent = `${user}: ${message} - ${localDateTime}`;
});

function displayMessages() {
    const messagesList = document.getElementById("messagesList");
    messagesList.innerHTML = ''; // Clear the existing list

    // Display each message in order
    messages.forEach(message => {
        const li = document.createElement("li");
        li.textContent = `${message.user}: ${message.message} - ${message.localDateTime}`;
        messagesList.appendChild(li);
    });

}

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    let user = document.getElementById("userInput").value;
    let message = document.getElementById("messageInput").value;
    connection.invoke("SendMessageAsync", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});