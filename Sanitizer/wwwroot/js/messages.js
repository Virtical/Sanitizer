/* global marked */
// ==================== УПРАВЛЕНИЕ СООБЩЕНИЯМИ ====================

// Флаг ожидания ответа от сервера
let isWaitingForResponse = false;

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
        msgDiv.setAttribute('data-message-id', msg.id);
        bubble.innerHTML = marked.parse(msg.text);

        if (msg.type === 'Sent') {
            const actionsClone = cloneTemplate('message-actions-template');
            if (actionsClone) {
                const eyeBtn = actionsClone.querySelector('.eye-btn');

                const hasSanitized = sanitizedMessages.some(m => m.originalMessageId === msg.id);
                if (hasSanitized) eyeBtn.classList.add('active');
                
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

    const messagesArea = document.getElementById('messagesArea');
    const currentScroll = messagesArea.scrollTop || 0;
    const isAtBottom = messagesArea && (messagesArea.scrollHeight - messagesArea.scrollTop <= messagesArea.clientHeight + 50);
    const eyeBtn = document.querySelector(`.message.sent[data-message-id="${originalMsgId}"] .eye-btn`);

    if (sanitizedIndex !== -1) {
         sanitizedMessages.splice(sanitizedIndex, 1);
         eyeBtn.classList.remove('active');
    } else {
        const sanitizedMsg = messages.find(m => m.originalMessageId === originalMsgId);
        const newMsg = {
            id: sanitizedMsg.id,
            text: sanitizedMsg.text,
            type: sanitizedMsg.type,
            originalMessageId: sanitizedMsg.originalMessageId,
        };
        sanitizedMessages.push(newMsg);
        eyeBtn.classList.add('active');
    }
    await renderMessages();
    
    if (isAtBottom) {
        messagesArea.scrollTop = messagesArea.scrollHeight;
    } else {
        messagesArea.scrollTop = currentScroll;
    }
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

// Обновление состояния кнопки отправки
function updateSendButtonState() {
    const messageInput = document.getElementById('messageInput');
    const emptyMessageInput = document.getElementById('emptyMessageInput');
    const sendBtn = document.getElementById('sendBtn');
    const emptySendBtn = document.getElementById('emptySendBtn');

    const text = (messageInput.value.trim()) || (emptyMessageInput.value.trim()) || '';
    const hasText = text.length > 0;
    const isActive = hasText && !isWaitingForResponse;

    sendBtn.disabled = !isActive;
    emptySendBtn.disabled = !isActive;
}

// Блокировка кнопок во время ожидания
function blockButtons() {
    isWaitingForResponse = true;
    updateSendButtonState();
}

// Разблокировка кнопок после ответа
function unblockButtons() {
    isWaitingForResponse = false;
    updateSendButtonState();
}

// Инициализация всех textarea
function autoResizeTextarea(textarea) {
    if (!textarea) return;

    textarea.style.height = 'auto';

    // Получаем высоту содержимого
    const scrollHeight = textarea.scrollHeight;
    const maxHeight = 350;
    const minHeight = 60;

    // Устанавливаем новую высоту
    textarea.style.height = Math.min(Math.max(scrollHeight, minHeight), maxHeight) + 'px';

    // Показываем скролл только если нужно
    if (scrollHeight > maxHeight) {
        textarea.style.overflowY = 'auto';
    } else {
        textarea.style.overflowY = 'hidden';
    }
}

// Очистка полей ввода
function clearMessageInput() {
    const messageInput = document.getElementById('messageInput');
    const emptyMessageInput = document.getElementById('emptyMessageInput');

    messageInput.value = '';
    messageInput.style.height = '60px';
    messageInput.style.overflowY = 'hidden';
    emptyMessageInput.value = '';
    emptyMessageInput.style.height = '60px';
    emptyMessageInput.style.overflowY = 'hidden';
}


async function addMessage(text) {
    if (!text.trim()) return;
    if (isWaitingForResponse) return;

    blockButtons();
    clearMessageInput();

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
            currentDialogId = await apiCreateDialog(text, onChunk);
            await loadDialogs();

            if (currentProfile) {
                try {
                    await apiUpdateDialogProfile(currentDialogId, currentProfile.id);
                } catch (error) {
                    console.error('Ошибка привязки профиля к новому диалогу:', error);
                }
            }
        }
        
        // Существующий диалог
        await apiSendMessage(currentDialogId, text, onChunk);

        // Финальная синхронизация с сервером:
        // получаем канонические данные (id сообщений, sanitized-копии для кнопки «глаза» и т.п.)
        const serverData = await apiGetMessages(currentDialogId);
        await renderMessages(serverData);
        renderDialogs();
        unblockButtons();
    } catch (error) {
        console.error('Ошибка отправки:', error);
        if (answerBubble) {
            answerBubble.classList.remove('message-bubble');
            answerBubble.classList.add('message-error');
            answerBubble.textContent = error.message;
        }
        
        unblockButtons();
    }
}
