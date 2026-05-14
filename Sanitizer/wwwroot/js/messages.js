// ==================== УПРАВЛЕНИЕ СООБЩЕНИЯМИ ====================
async function renderMessages() {
    const serverData = await apiGetMessages(currentDialogId);
    const hasMessages = serverData.messages.length > 0;
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
    const originalMessages = serverData.messages.filter(m => m.type === 'Sent' || m.type === 'Answer');

    originalMessages.forEach((msg) => {
        const msgClone = cloneTemplate('message-template');
        if (!msgClone) return;
        
        const msgDiv = msgClone.querySelector('.message');
        const bubble = msgClone.querySelector('.message-bubble');
        
        msgDiv.classList.add(msg.type.toLowerCase());
        bubble.textContent = msg.text;

        if (msg.type === 'Sent') {
            const actionsClone = cloneTemplate('message-actions-template');
            if (actionsClone) {
                const eyeBtn = actionsClone.querySelector('.eye-btn');
                
                eyeBtn.addEventListener('click', async (e) => {
                    e.stopPropagation();
                    await toggleSanitizedMessage(serverData.messages, msg.id);
                });
                
                msgDiv.appendChild(actionsClone);
            }
        }

        const sanitizedCopy = sanitizedMessages.find(m => m.originalMessageId === msg.id);
        if (sanitizedCopy) {
            const sanitizedClone = cloneTemplate('sanitized-message-template');
            if (sanitizedClone) {
                const sanitizedBubble = sanitizedClone.querySelector('.sanitized-bubble');
                sanitizedBubble.textContent = sanitizedCopy.text;
                msgDiv.appendChild(sanitizedClone);
            }
        }
        
        messagesArea.appendChild(msgClone);
    });
    
    messagesArea.scrollTop = messagesArea.scrollHeight;
}

async function toggleSanitizedMessage(messages, originalMsgId) {
    const sanitizedIndex = sanitizedMessages.findIndex(m => m.originalMessageId === originalMsgId);
    const originalMsg = messages.find(m => m.id === originalMsgId);
    if (!originalMsg) return;

    if (sanitizedIndex !== -1) {
         sanitizedMessages.splice(sanitizedIndex, 1);
    } else {
        const sanitizedMsg = messages.find(m => m.originalMessageId === originalMsgId);
        const newMsg = {
            id: sanitizedMsg.id,
            text: sanitizedMsg.text,
            type: sanitizedMsg.type,
            originalMessageId: sanitizedMsg.originalMessageId,
        };
        sanitizedMessages.push(newMsg);
    }
    await renderMessages();
    await updateDialogName(currentDialogId);
}

async function updateDialogName(dialogId) {
    const dialog = dialogs.find(d => d.id === dialogId);
    if (!dialog || dialog.name !== 'Новый диалог') return;
    const serverData = await apiGetMessages(dialogId);
    const firstUserMessage = serverData.messages.find(msg => msg.type === 'Sent');
    let newName = firstUserMessage.text.trim();
    if (newName.length > 30) newName = newName.substring(0, 27) + '...';
    try {
        dialogs = await apiUpdateDialogName(dialogId, newName);
        renderDialogs();
    } catch (error) {
        console.error('Ошибка обновления названия диалога:', error);
    }
}

async function addMessage(text) {
    if (!text.trim() || !currentDialogId) return;
    await apiSendMessage(currentDialogId, text);
    await renderMessages();
    await updateDialogName(currentDialogId);
}
