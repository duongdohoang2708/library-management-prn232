document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("ai-chat-toggle");
    const chatBox = document.getElementById("ai-chat-box");
    const closeBtn = document.getElementById("ai-chat-close");
    const expandBtn = document.getElementById("ai-chat-expand");
    const sendBtn = document.getElementById("ai-chat-send");
    const input = document.getElementById("ai-chat-input");
    const messages = document.getElementById("ai-chat-messages");

    if (!toggleBtn || !chatBox || !closeBtn || !sendBtn || !input || !messages) return;

    let isExpanded = false;

    toggleBtn.addEventListener("click", function () {
        chatBox.classList.remove("hidden");
        toggleBtn.classList.add("hidden");
        input.focus();
    });

    closeBtn.addEventListener("click", function () {
        chatBox.classList.add("hidden");
        toggleBtn.classList.remove("hidden");
    });

    if (expandBtn) {
        expandBtn.addEventListener("click", function () {
            isExpanded = !isExpanded;
            const icon = expandBtn.querySelector("span");
            if (isExpanded) {
                // Expand
                chatBox.classList.remove("w-[380px]", "h-[560px]", "bottom-24");
                chatBox.classList.add("w-[33vw]", "h-[90vh]", "bottom-6");
                if (icon) icon.textContent = "close_fullscreen";
                expandBtn.title = "Thu nhỏ";
            } else {
                // Collapse
                chatBox.classList.remove("w-[33vw]", "h-[90vh]", "bottom-6");
                chatBox.classList.add("w-[380px]", "h-[560px]", "bottom-24");
                if (icon) icon.textContent = "open_in_full";
                expandBtn.title = "Phóng to";
            }
        });
    }

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
            div.textContent = text;
        } else {
            div.className =
                "mr-auto max-w-[85%] rounded-2xl rounded-tl-md border border-white/8 bg-white/10 px-4 py-3 text-sm leading-6 text-slate-100 shadow-sm prose prose-invert prose-sm max-w-none";
            if (typeof marked !== 'undefined') {
                div.innerHTML = marked.parse(text);
            } else {
                div.textContent = text;
            }
        }

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
            const response = await fetch("/api/ai/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ message: question })
            });

            const data = await response.json();
            if (typeof marked !== 'undefined' && data.responseText) {
                loadingMessage.innerHTML = marked.parse(data.responseText);
            } else {
                loadingMessage.textContent = data.responseText || "Mình chưa có phản hồi phù hợp.";
            }
        } catch (error) {
            loadingMessage.textContent = "Đã có lỗi khi kết nối tới AI.";
            console.error(error);
        } finally {
            sendBtn.disabled = false;
            input.focus();
        }
    }
});