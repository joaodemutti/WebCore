// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
window.conn = new signalR.HubConnectionBuilder().withUrl('/game').build();

//startPromise = conn.start().then(e =>
//    conn.invoke("Enter")
//)

// Write your JavaScript code.
