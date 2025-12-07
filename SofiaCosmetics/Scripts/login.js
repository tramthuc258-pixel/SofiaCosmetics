document.addEventListener("DOMContentLoaded", function () {
    const eye = document.getElementById("eye");
    const pwd = document.getElementById("pwd");

    if (eye && pwd) {
        eye.addEventListener("click", function () {
            pwd.type = pwd.type === "password" ? "text" : "password";
        });
    }
});