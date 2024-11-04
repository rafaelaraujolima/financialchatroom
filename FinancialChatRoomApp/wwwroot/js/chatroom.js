"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/chatroomhub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message, dateTime) {
    const utcDate = new Date(dateTime);

    const localDate = new Date(utcDate.getTime() - utcDate.getTimezoneOffset() * 60000);

    const localDateTime = localDate.toLocaleString([], {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });

    let li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user}: ${message} - ${localDateTime}`;
});

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