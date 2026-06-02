document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("ai-chat-toggle");
    const chatBox = document.getElementById("ai-chat-box");
    const closeBtn = document.getElementById("ai-chat-close");
    const sendBtn = document.getElementById("ai-chat-send");
    const input = document.getElementById("ai-chat-input");
    const messages = document.getElementById("ai-chat-messages");

    if (!toggleBtn || !chatBox || !closeBtn || !sendBtn || !input || !messages) return;

    toggleBtn.addEventListener("click", function () {
        chatBox.classList.remove("hidden");
        toggleBtn.classList.add("hidden");
        input.focus();
    });

    closeBtn.addEventListener("click", function () {
        chatBox.classList.add("hidden");
        toggleBtn.classList.remove("hidden");
    });

    sendBtn.addEventListener("click", sendMessage);

    input.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            sendMessage();
        }
    });

    function appendMessage(text, role) {
        const div = document.createElement("div");

        if (role === "user") {
            div.className =
                "ml-auto max-w-[85%] rounded-2xl rounded-br-md bg-[#e7d80f] px-4 py-3 text-sm leading-6 text-slate-900 shadow-[0_8px_20px_rgba(231,216,15,0.18)]";
        } else {
            div.className =
                "mr-auto max-w-[85%] rounded-2xl rounded-tl-md border border-white/8 bg-white/10 px-4 py-3 text-sm leading-6 text-slate-100 shadow-sm";
        }

        div.textContent = text;
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
        return div;
    }

    async function sendMessage() {
        const question = input.value.trim();
        if (!question) return;

        appendMessage(question, "user");
        input.value = "";
        sendBtn.disabled = true;

        const loadingMessage = appendMessage("Mình đang tìm câu trả lời phù hợp...", "bot");

        try {
            const response = await fetch("/api/AI/ask", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ question: question })
            });

            const data = await response.json();
            loadingMessage.textContent = data.answer || "Mình chưa có phản hồi phù hợp.";
        } catch (error) {
            loadingMessage.textContent = "Đã có lỗi khi kết nối tới AI.";
            console.error(error);
        } finally {
            sendBtn.disabled = false;
            input.focus();
        }
    }
});