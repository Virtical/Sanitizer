/* global marked */
// ==================== УПРАВЛЕНИЕ СООБЩЕНИЯМИ ====================
async function renderMessages(data = null) {
    const serverData = data || await apiGetMessages(currentDialogId);
    const profileToShow = allProfiles.find(p => p.id === serverData.sanitizationProfileId) || currentProfile;
    if (!currentProfile || currentProfile.id !== profileToShow.id) {
        currentProfile = profileToShow;
        updateProfileButtonText();
        updateProfileDropdowns();
    }
    
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
        bubble.innerHTML = marked.parse(msg.text);

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
                sanitizedBubble.innerHTML = marked.parse(sanitizedCopy.text);
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
}

/**
 * Оптимистично отрисовывает сообщение пользователя и пустой бабл ответа.
 * Возвращает ссылку на DOM-элемент бабла ответа для дальнейшего обновления.
 * @param {string} text - текст отправленного сообщения
 * @returns {HTMLElement|null} - бабл ответа (.message-bubble внутри .answer)
 */
function appendOptimisticMessages(text) {
    const emptyState = document.getElementById('chatEmptyState');
    const messagesWrapper = document.getElementById('messagesWrapper');
    const messagesArea = document.getElementById('messagesArea');

    if (emptyState) emptyState.style.display = 'none';
    if (messagesWrapper) messagesWrapper.style.display = 'flex';
    if (!messagesArea) return null;

    // Сообщение пользователя
    const sentClone = cloneTemplate('message-template');
    if (sentClone) {
        const sentDiv = sentClone.querySelector('.message');
        const sentBubble = sentClone.querySelector('.message-bubble');
        sentDiv.classList.add('sent');
        sentBubble.innerHTML = marked.parse(text);
        messagesArea.appendChild(sentClone);
    }

    // Пустой бабл ответа ассистента
    const ansClone = cloneTemplate('message-template');
    if (!ansClone) return null;

    const ansDiv = ansClone.querySelector('.message');
    const ansBubble = ansClone.querySelector('.message-bubble');
    ansDiv.classList.add('answer');
    ansBubble.innerHTML = '';
    messagesArea.appendChild(ansClone);

    messagesArea.scrollTop = messagesArea.scrollHeight;
    return ansBubble;
}

async function addMessage(text) {
    if (!text.trim()) return;

    const messagesArea = document.getElementById('messagesArea');
    const answerBubble = appendOptimisticMessages(text);

    // Колбэк для обновления бабла ответа в реальном времени
    const onChunk = (_chunk, fullText) => {
        if (answerBubble) {
            answerBubble.innerHTML = marked.parse(fullText);
            if (messagesArea) messagesArea.scrollTop = messagesArea.scrollHeight;
        }
    };

    try {
        if (!currentDialogId) {
            // Новый диалог — получаем id из заголовка X-Chat-Id
            const { chatId } = await apiCreateDialog(text, onChunk);
            currentDialogId = chatId;
            await loadDialogs();

            if (currentProfile) {
                try {
                    await apiUpdateDialogProfile(currentDialogId, currentProfile.id);
                } catch (error) {
                    console.error('Ошибка привязки профиля к новому диалогу:', error);
                }
            }
        } else {
            // Существующий диалог
            await apiSendMessage(currentDialogId, text, onChunk);
        }

        // Финальная синхронизация с сервером:
        // получаем канонические данные (id сообщений, sanitized-копии для кнопки «глаза» и т.п.)
        const serverData = await apiGetMessages(currentDialogId);
        await renderMessages(serverData);
        renderDialogs();
    } catch (error) {
        console.error('Ошибка отправки:', error);
        if (answerBubble) {
            answerBubble.classList.remove('message-bubble');
            answerBubble.classList.add('message-error');
            answerBubble.textContent = error.message;
        }
    }
}
