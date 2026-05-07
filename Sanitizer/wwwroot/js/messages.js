// ==================== УПРАВЛЕНИЕ СООБЩЕНИЯМИ ====================
function renderMessages() {
    const hasMessages = currentDialogId && messages[currentDialogId] && messages[currentDialogId].length > 0;
    const emptyState = document.getElementById('chatEmptyState');
    const messagesWrapper = document.getElementById('messagesWrapper');
    const messagesArea = document.getElementById('messagesArea');

    if (!hasMessages) {
        if (emptyState) emptyState.style.display = 'flex';
        if (messagesWrapper) messagesWrapper.style.display = 'none';
        return;
    }

    if (emptyState) emptyState.style.display = 'none';
    if (messagesWrapper) messagesWrapper.style.display = 'flex';
    if (!messagesArea) return;

    messagesArea.innerHTML = '';
    const originalMessages = messages[currentDialogId].filter(msg => !msg.isSanitizedCopy);

    originalMessages.forEach((msg, idx) => {
        const msgDiv = document.createElement('div');
        msgDiv.className = `message ${msg.type}`;
        const bubble = document.createElement('div');
        bubble.className = 'message-bubble';
        bubble.textContent = msg.text;
        msgDiv.appendChild(bubble);

        if (msg.type === 'sent') {
            const actionsDiv = document.createElement('div');
            actionsDiv.className = 'message-actions';
            const eyeBtn = document.createElement('button');
            eyeBtn.className = 'eye-btn';
            eyeBtn.innerHTML = '<img src="images/si_eye-line.svg" alt="Показать санитизированный текст" class="eye-icon">';
            const originalMsgId = msg.id;
            eyeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                toggleSanitizedMessage(currentDialogId, originalMsgId);
            });
            actionsDiv.appendChild(eyeBtn);
            msgDiv.appendChild(actionsDiv);
        }

        const sanitizedCopy = messages[currentDialogId].find(m => m.isSanitizedCopy && m.originalMessageId === msg.id);
        if (sanitizedCopy) {
            const sanitizedDiv = document.createElement('div');
            sanitizedDiv.className = 'sanitized-message';
            const sanitizedBubble = document.createElement('div');
            sanitizedBubble.className = 'message-bubble sanitized-bubble';
            sanitizedBubble.textContent = sanitizedCopy.text;
            sanitizedDiv.appendChild(sanitizedBubble);
            msgDiv.appendChild(sanitizedDiv);
        }
        messagesArea.appendChild(msgDiv);
    });
    messagesArea.scrollTop = messagesArea.scrollHeight;
}

function toggleSanitizedMessage(dialogId, originalMsgId) {
    if (!messages[dialogId]) return;
    const sanitizedIndex = messages[dialogId].findIndex(m => m.isSanitizedCopy && m.originalMessageId === originalMsgId);
    const originalMsg = messages[dialogId].find(m => m.id === originalMsgId);
    if (!originalMsg) return;

    if (sanitizedIndex !== -1) {
        const newMessages = [...messages[dialogId]];
        newMessages.splice(sanitizedIndex, 1);
        messages[dialogId] = newMessages;
    } else {
        const newMsg = {
            id: `sanitized_${Date.now()}_${originalMsgId}`,
            text: originalMsg.sanitized,
            type: originalMsg.type,
            isSanitizedCopy: true,
            originalMessageId: originalMsg.id
        };
        const newMessages = [...messages[dialogId]];
        newMessages.push(newMsg);
        messages[dialogId] = newMessages;
    }
    renderMessages();
}

function addMessage(text, type = 'sent') {
    if (!text.trim() || !currentDialogId) return;
    const newId = 'msg_' + Date.now();
    const sanitizedText = text.replace(/</g, '&lt;').replace(/>/g, '&gt;');
    const newMsg = {
        id: newId,
        text: text,
        type: type,
        sanitized: sanitizedText,
        isSanitizedCopy: false
    };
    if (!messages[currentDialogId]) messages[currentDialogId] = [];
    messages[currentDialogId].push(newMsg);
    renderMessages();

    if (type === 'sent') {
        updateDialogName(currentDialogId);
        setTimeout(() => {
            const replyMsg = {
                id: 'reply_' + Date.now(),
                text: 'Автоответ',
                type: 'received',
                sanitized: 'Автоответ',
                isSanitizedCopy: false
            };
            if (messages[currentDialogId]) {
                messages[currentDialogId].push(replyMsg);
                renderMessages();
            }
        }, 300);
    }
}
