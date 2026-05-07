// ==================== ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ====================
function getDateGroup(date) {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    const weekAgo = new Date(today);
    weekAgo.setDate(weekAgo.getDate() - 7);
    const monthAgo = new Date(today);
    monthAgo.setDate(monthAgo.getDate() - 30);
    const dialogDate = new Date(date);
    dialogDate.setHours(0, 0, 0, 0);

    if (dialogDate.getTime() === today.getTime()) return 'today';
    if (dialogDate.getTime() === yesterday.getTime()) return 'yesterday';
    if (dialogDate > weekAgo) return 'last7days';
    if (dialogDate > monthAgo) return 'last30days';
    return dialogDate.toLocaleString('ru', { month: 'long', year: 'numeric' });
}

function getGroupTitle(group) {
    switch(group) {
        case 'today': return 'Сегодня';
        case 'yesterday': return 'Вчера';
        case 'last7days': return 'Последние 7 дней';
        case 'last30days': return 'Последние 30 дней';
        default: return group;
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function(m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}

function truncateText(text, maxLength = 20) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength - 3) + '...';
}

function getNextProfileId() {
    return nextProfileId++;
}

// ==================== КАСТОМНЫЙ СКРОЛЛБАР ====================
function createCustomScrollbar(container, wrapper) {
    if (!container || !wrapper) return null;

    const existingScrollbar = wrapper.querySelector('.custom-scrollbar');
    if (existingScrollbar) existingScrollbar.remove();

    const scrollbar = document.createElement('div');
    scrollbar.className = 'custom-scrollbar';
    const thumb = document.createElement('div');
    thumb.className = 'custom-scrollbar-thumb';
    scrollbar.appendChild(thumb);
    wrapper.appendChild(scrollbar);

    let isDragging = false;
    let startY = 0;
    let startScrollTop = 0;

    function updateScrollbar() {
        const visibleRatio = container.clientHeight / container.scrollHeight;
        const thumbHeight = Math.max(40, visibleRatio * container.clientHeight);
        thumb.style.height = thumbHeight + 'px';
        const maxThumbTop = container.clientHeight - thumbHeight;
        const scrollPercent = container.scrollTop / (container.scrollHeight - container.clientHeight);
        const thumbTop = scrollPercent * maxThumbTop;
        thumb.style.top = thumbTop + 'px';
        scrollbar.style.height = container.clientHeight + 'px';

        if (container.scrollHeight <= container.clientHeight) {
            scrollbar.style.opacity = '0';
            scrollbar.style.pointerEvents = 'none';
        } else {
            scrollbar.style.opacity = '1';
            scrollbar.style.pointerEvents = 'auto';
        }
    }

    function onScroll() { updateScrollbar(); }

    function onMouseDown(e) {
        isDragging = true;
        startY = e.clientY;
        startScrollTop = container.scrollTop;
        document.body.style.userSelect = 'none';
        document.body.style.cursor = 'grabbing';
        e.preventDefault();
    }

    function onMouseMove(e) {
        if (!isDragging) return;
        const deltaY = e.clientY - startY;
        const scrollAmount = (deltaY / container.clientHeight) * container.scrollHeight;
        container.scrollTop = startScrollTop + scrollAmount;
    }

    function onMouseUp() {
        isDragging = false;
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
    }

    function onScrollbarClick(e) {
        if (e.target === thumb) return;
        const rect = scrollbar.getBoundingClientRect();
        const clickY = e.clientY - rect.top;
        const clickPercent = clickY / container.clientHeight;
        const targetScrollTop = clickPercent * (container.scrollHeight - container.clientHeight);
        container.scrollTop = targetScrollTop;
    }

    container.addEventListener('scroll', onScroll);
    thumb.addEventListener('mousedown', onMouseDown);
    scrollbar.addEventListener('click', onScrollbarClick);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    const resizeObserver = new ResizeObserver(() => updateScrollbar());
    resizeObserver.observe(container);
    const mutationObserver = new MutationObserver(() => updateScrollbar());
    mutationObserver.observe(container, { childList: true, subtree: true, attributes: true });

    setTimeout(updateScrollbar, 100);
    window.addEventListener('resize', updateScrollbar);
    return updateScrollbar;
}

function createProfilesScrollbar() {
    const profilesList = document.querySelector('.profiles-list.has-scroll');
    if (!profilesList) return;

    let wrapper = profilesList.parentElement;
    if (wrapper.classList.contains('profiles-list-wrapper') && wrapper.querySelector('.custom-scrollbar-profiles')) {
        return;
    }

    if (!wrapper.classList.contains('profiles-list-wrapper')) {
        const newWrapper = document.createElement('div');
        newWrapper.className = 'profiles-list-wrapper';
        profilesList.parentNode.insertBefore(newWrapper, profilesList);
        newWrapper.appendChild(profilesList);
        wrapper = newWrapper;
        wrapper.style.position = 'relative';
        wrapper.style.overflow = 'hidden';
    }

    const existingScrollbar = wrapper.querySelector('.custom-scrollbar-profiles');
    if (existingScrollbar) existingScrollbar.remove();

    profilesList.style.overflowY = 'auto';
    profilesList.style.scrollbarWidth = 'none';
    profilesList.style.msOverflowStyle = 'none';
    profilesList.style.overflow = 'auto';

    const scrollbar = document.createElement('div');
    scrollbar.className = 'custom-scrollbar-profiles';
    const thumb = document.createElement('div');
    thumb.className = 'custom-scrollbar-thumb-profiles';
    scrollbar.appendChild(thumb);
    wrapper.appendChild(scrollbar);

    let isDragging = false;
    let startY = 0;
    let startScrollTop = 0;

    function updateScrollbar() {
        if (profilesList.scrollHeight <= profilesList.clientHeight) {
            scrollbar.style.opacity = '0';
            scrollbar.style.pointerEvents = 'none';
            return;
        }
        const visibleRatio = profilesList.clientHeight / profilesList.scrollHeight;
        const thumbHeight = Math.max(40, visibleRatio * profilesList.clientHeight);
        thumb.style.height = thumbHeight + 'px';
        const maxThumbTop = profilesList.clientHeight - thumbHeight;
        const scrollPercent = profilesList.scrollTop / (profilesList.scrollHeight - profilesList.clientHeight);
        const thumbTop = scrollPercent * maxThumbTop;
        thumb.style.top = thumbTop + 'px';
        scrollbar.style.height = profilesList.clientHeight + 'px';
        scrollbar.style.opacity = '1';
        scrollbar.style.pointerEvents = 'auto';
    }

    function onScroll() { updateScrollbar(); }
    function onMouseDown(e) {
        isDragging = true;
        startY = e.clientY;
        startScrollTop = profilesList.scrollTop;
        document.body.style.userSelect = 'none';
        document.body.style.cursor = 'grabbing';
        e.preventDefault();
    }
    function onMouseMove(e) {
        if (!isDragging) return;
        const deltaY = e.clientY - startY;
        const scrollAmount = (deltaY / profilesList.clientHeight) * profilesList.scrollHeight;
        profilesList.scrollTop = startScrollTop + scrollAmount;
    }
    function onMouseUp() {
        isDragging = false;
        document.body.style.userSelect = '';
        document.body.style.cursor = '';
    }
    function onScrollbarClick(e) {
        if (e.target === thumb) return;
        const rect = scrollbar.getBoundingClientRect();
        const clickY = e.clientY - rect.top;
        const clickPercent = clickY / profilesList.clientHeight;
        const targetScrollTop = clickPercent * (profilesList.scrollHeight - profilesList.clientHeight);
        profilesList.scrollTop = targetScrollTop;
    }

    profilesList.addEventListener('scroll', onScroll);
    thumb.addEventListener('mousedown', onMouseDown);
    scrollbar.addEventListener('click', onScrollbarClick);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    const resizeObserver = new ResizeObserver(() => updateScrollbar());
    resizeObserver.observe(profilesList);
    const mutationObserver = new MutationObserver(() => updateScrollbar());
    mutationObserver.observe(profilesList, { childList: true, subtree: true, attributes: true });

    setTimeout(updateScrollbar, 100);
    window.addEventListener('resize', updateScrollbar);
}

// ==================== УПРАВЛЕНИЕ ДИАЛОГАМИ ====================
function renderDialogs() {
    const dialogsList = document.getElementById('dialogsList');
    if (!dialogsList) return;

    if (dialogs.length === 0) {
        dialogsList.innerHTML = '<div class="no-dialogs">Нет диалогов</div>';
        return;
    }

    const grouped = {};
    dialogs.forEach(dialog => {
        const group = getDateGroup(dialog.createdAt);
        if (!grouped[group]) grouped[group] = [];
        grouped[group].push(dialog);
    });

    const groupOrder = ['today', 'yesterday', 'last7days', 'last30days'];
    const sortedGroups = Object.keys(grouped).sort((a, b) => {
        const aIndex = groupOrder.indexOf(a);
        const bIndex = groupOrder.indexOf(b);
        if (aIndex !== -1 && bIndex !== -1) return aIndex - bIndex;
        if (aIndex !== -1) return -1;
        if (bIndex !== -1) return 1;
        return a.localeCompare(b);
    });

    dialogsList.innerHTML = '';
    sortedGroups.forEach(group => {
        const groupDialogs = grouped[group];
        const groupDiv = document.createElement('div');
        groupDiv.className = 'dialogs-group';
        const titleDiv = document.createElement('div');
        titleDiv.className = 'group-title';
        titleDiv.textContent = getGroupTitle(group);
        groupDiv.appendChild(titleDiv);
        const dialogsContainer = document.createElement('div');
        dialogsContainer.className = 'group-dialogs';

        groupDialogs.forEach(dialog => {
            const dialogDiv = document.createElement('div');
            dialogDiv.className = 'dialog-item';
            if (dialog.id === currentDialogId) dialogDiv.classList.add('active');
            dialogDiv.dataset.dialogId = dialog.id;
            dialogDiv.innerHTML = `<div class="dialog-name">${escapeHtml(dialog.name)}</div>`;
            dialogDiv.addEventListener('click', (e) => {
                e.stopPropagation();
                currentDialogId = dialog.id;
                renderDialogs();
                renderMessages();
                if (isProfileCreationVisible) showChatPanel();
            });
            dialogsContainer.appendChild(dialogDiv);
        });
        groupDiv.appendChild(dialogsContainer);
        dialogsList.appendChild(groupDiv);
    });
}

function createNewDialog() {
    const newId = Date.now().toString();
    const now = new Date();
    dialogs.unshift({
        id: newId,
        name: 'Новый диалог',
        createdAt: now.toISOString()
    });
    messages[newId] = [];
    currentDialogId = newId;
    renderDialogs();
    renderMessages();
    if (isProfileCreationVisible) showChatPanel();
}

function updateDialogName(dialogId) {
    const dialog = dialogs.find(d => d.id === dialogId);
    if (!dialog || dialog.name !== 'Новый диалог') return;
    const dialogMessages = messages[dialogId];
    if (!dialogMessages) return;
    const firstUserMessage = dialogMessages.find(msg => msg.type === 'sent' && !msg.isSanitizedCopy);
    if (firstUserMessage && firstUserMessage.text) {
        let newName = firstUserMessage.text.trim();
        if (newName.length > 30) newName = newName.substring(0, 27) + '...';
        dialog.name = newName;
        renderDialogs();
    }
}

function createInitialDialog() {
    const initialId = 'initial_1';
    const now = new Date();
    dialogs.push({
        id: initialId,
        name: 'Новый диалог',
        createdAt: now.toISOString()
    });
    messages[initialId] = [];
    currentDialogId = initialId;
}

// ==================== УПРАВЛЕНИЕ СВОРАЧИВАНИЕМ БЛОКА 2 ====================
let isDialogsCollapsed = false;

function toggleDialogsCollapse() {
    const dialogsFull = document.getElementById('dialogsFull');
    const dialogsCollapsed = document.getElementById('dialogsCollapsed');
    const dialogsPanel = document.getElementById('dialogsPanel');
    const hideBtn = document.getElementById('hideDialogsBtn');

    if (!dialogsFull || !dialogsCollapsed || !dialogsPanel) return;

    isDialogsCollapsed = !isDialogsCollapsed;

    if (isDialogsCollapsed) {
        dialogsFull.style.display = 'none';
        dialogsCollapsed.style.display = 'flex';
        dialogsPanel.style.width = '79px';
        dialogsPanel.style.minWidth = '79px';
        dialogsPanel.style.maxWidth = '79px';
    } else {
        dialogsFull.style.display = 'flex';
        dialogsCollapsed.style.display = 'none';
        dialogsPanel.style.width = '320px';
        dialogsPanel.style.minWidth = '320px';
        dialogsPanel.style.maxWidth = '320px';
        if (hideBtn) hideBtn.style.display = 'flex';
    }
    setTimeout(() => {
        if (typeof updateScrollbars === 'function') updateScrollbars();
    }, 100);
}

function showDialogsFromCollapsed() {
    if (isDialogsCollapsed) toggleDialogsCollapse();
}

function initCollapseListeners() {
    const hideDialogsBtn = document.getElementById('hideDialogsBtn');
    const showDialogsFromCollapsedBtn = document.getElementById('showDialogsFromCollapsedBtn');
    const collapsedNewChatBtn = document.getElementById('collapsedNewChatBtn');
    const collapsedUserProfileBtn = document.getElementById('collapsedUserProfileBtn');
    const userProfileBtn = document.getElementById('userProfileBtn');
    const newChatBtn = document.getElementById('newChatBtn');

    if (hideDialogsBtn) {
        const newHideBtn = hideDialogsBtn.cloneNode(true);
        hideDialogsBtn.parentNode.replaceChild(newHideBtn, hideDialogsBtn);
        const freshHideBtn = document.getElementById('hideDialogsBtn');
        if (freshHideBtn) freshHideBtn.addEventListener('click', toggleDialogsCollapse);
    }
    if (showDialogsFromCollapsedBtn) showDialogsFromCollapsedBtn.addEventListener('click', showDialogsFromCollapsed);
    if (collapsedNewChatBtn) collapsedNewChatBtn.addEventListener('click', () => createNewDialog());
    if (collapsedUserProfileBtn) collapsedUserProfileBtn.addEventListener('click', () => {});
    if (userProfileBtn) userProfileBtn.addEventListener('click', () => {});
    if (newChatBtn) newChatBtn.addEventListener('click', createNewDialog);
}

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
            eyeBtn.innerHTML = '<img src="images/eye.svg" alt="Показать санитизированный текст" class="eye-icon">';
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

// ==================== УПРАВЛЕНИЕ ПРОФИЛЯМИ ====================
function updateProfileDropdowns() {
    updateProfileDropdown(document.getElementById('profileDropdown'), document.getElementById('profileChatBtn'));
    updateProfileDropdown(document.getElementById('profileDropdownCreation'), document.getElementById('profileChatBtnCreation'));
}

function updateProfileDropdown(dropdownElement, buttonElement) {
    if (!dropdownElement) return;
    const dropdownContent = dropdownElement;
    dropdownContent.innerHTML = '';

    const selectedDiv = document.createElement('div');
    selectedDiv.className = 'profile-selected-item';
    const selectedName = document.createElement('span');
    selectedName.className = 'profile-selected-name';
    selectedName.textContent = truncateText(currentProfile.name, 25);
    selectedName.title = currentProfile.name;
    selectedDiv.appendChild(selectedName);
    const editIcon = document.createElement('div');
    editIcon.className = 'profile-selected-edit';
    editIcon.innerHTML = '<img src="images/editing.svg" alt="Edit">';
    editIcon.addEventListener('click', (e) => {
        e.stopPropagation();
        console.log('Редактировать профиль:', currentProfile.name);
    });
    selectedDiv.appendChild(editIcon);
    selectedDiv.addEventListener('click', () => {
        dropdownElement.classList.remove('show');
    });
    dropdownContent.appendChild(selectedDiv);

    const otherProfiles = allProfiles.filter(p => p.id !== currentProfile.id);
    if (otherProfiles.length > 0) {
        const profilesList = document.createElement('div');
        profilesList.className = 'profiles-list';
        otherProfiles.forEach(profile => {
            const option = document.createElement('div');
            option.className = 'profile-option-item';
            option.textContent = truncateText(profile.name, 30);
            option.title = profile.name;
            option.addEventListener('click', () => {
                currentProfile = profile;
                dropdownElement.classList.remove('show');
                if (buttonElement) {
                    const profileChatText = buttonElement.querySelector('.profile-chat-text');
                    if (profileChatText) profileChatText.textContent = truncateText(currentProfile.name, 20);
                }
                updateProfileDropdowns();
            });
            profilesList.appendChild(option);
        });
        if (otherProfiles.length >= 8) {
            profilesList.classList.add('has-scroll');
        } else {
            profilesList.classList.remove('has-scroll');
        }
        dropdownContent.appendChild(profilesList);
        if (otherProfiles.length >= 8) {
            setTimeout(() => createProfilesScrollbar(), 10);
        }
    }

    const divider = document.createElement('div');
    divider.className = 'profile-divider-line';
    dropdownContent.appendChild(divider);

    const createBtn = document.createElement('button');
    createBtn.className = 'create-new-profile-btn';
    createBtn.innerHTML = `
        <div class="create-icon-box">
            <span class="create-icon-plus">+</span>
        </div>
        <span class="create-btn-text">Создать новый профиль</span>
    `;
    createBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdownElement.classList.remove('show');
        openProfileCreation();
    });
    dropdownContent.appendChild(createBtn);
}

function toggleProfileDropdown(dropdownElement) {
    if (!dropdownElement) return;
    if (dropdownElement.classList.contains('show')) {
        dropdownElement.classList.remove('show');
    } else {
        dropdownElement.classList.add('show');
    }
}

function openProfileCreation() {
    isProfileCreationVisible = true;
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');
    if (chatPanel) chatPanel.style.display = 'none';
    if (profileCreationPanel) profileCreationPanel.style.display = 'flex';
}

function closeProfileCreation() {
    showChatPanel();
    const newProfileName = document.getElementById('newProfileName');
    if (newProfileName) newProfileName.value = '';
}

function showChatPanel() {
    isProfileCreationVisible = false;
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');
    if (chatPanel) chatPanel.style.display = 'flex';
    if (profileCreationPanel) profileCreationPanel.style.display = 'none';
}

function saveNewProfile() {
    const newProfileNameInput = document.getElementById('newProfileName');
    const newProfileNameValue = newProfileNameInput ? newProfileNameInput.value.trim() : '';
    let profileName = newProfileNameValue !== '' ? newProfileNameValue : 'Профиль общения';

    const newProfile = {
        id: getNextProfileId(),
        name: profileName,
        dataTypes: [...selectedDataTypes],
        method: selectedMethod ? selectedMethod.value : null
    };

    allProfiles.push(newProfile);
    updateProfileDropdowns();

    const profileChatText = document.querySelector('.profile-chat-text');
    if (profileChatText) profileChatText.textContent = truncateText(currentProfile.name, 20);

    selectedDataTypes = [];
    selectedMethod = null;

    const dataMethodContainer = document.getElementById('dataMethodContainer');
    if (dataMethodContainer) dataMethodContainer.style.display = 'none';
    const addDataTypeBtn = document.getElementById('addDataTypeBtn');
    if (addDataTypeBtn) addDataTypeBtn.style.background = '#037C4E';

    document.querySelectorAll('.data-type-option').forEach(opt => opt.classList.remove('selected'));
    document.querySelectorAll('.method-option').forEach(opt => opt.classList.remove('selected'));

    const dataTypeText = document.querySelector('.data-type-text');
    if (dataTypeText) dataTypeText.textContent = 'Выберите тип данных';
    const methodText = document.querySelector('.method-text');
    if (methodText) methodText.textContent = 'Выберите метод санитизации';

    showChatPanel();
    if (newProfileNameInput) newProfileNameInput.value = '';
}

function updateProfileButtonText() {
    const profileChatText = document.querySelector('.profile-chat-text');
    if (profileChatText) {
        profileChatText.textContent = truncateText(currentProfile.name, 20);
        profileChatText.title = currentProfile.name;
    }
}

// ==================== ВЫБОР ТИПА ДАННЫХ И МЕТОДА ====================
function initDataMethodHandlers() {
    const addDataTypeBtn = document.getElementById('addDataTypeBtn');
    const addDataTypeText = document.getElementById('addDataTypeText');
    const dataMethodContainer = document.getElementById('dataMethodContainer');
    const dataTypeBtn = document.getElementById('dataTypeBtn');
    const dataTypeDropdown = document.getElementById('dataTypeDropdown');
    const methodBtn = document.getElementById('methodBtn');
    const methodDropdown = document.getElementById('methodDropdown');

    if (!addDataTypeBtn || !dataMethodContainer) return;

    const toggleDataMethodContainer = () => {
        if (dataMethodContainer.style.display === 'none' || dataMethodContainer.style.display === '') {
            dataMethodContainer.style.display = 'flex';
            addDataTypeBtn.style.background = '#02A065';
        } else {
            dataMethodContainer.style.display = 'none';
            addDataTypeBtn.style.background = '#037C4E';
        }
    };
    addDataTypeBtn.addEventListener('click', toggleDataMethodContainer);
    if (addDataTypeText) addDataTypeText.addEventListener('click', toggleDataMethodContainer);

    if (dataTypeBtn && dataTypeDropdown) {
        dataTypeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            dataTypeDropdown.classList.toggle('show');
            const arrow = dataTypeBtn.querySelector('.data-type-arrow');
            if (arrow) arrow.classList.toggle('rotated');
        });
        document.querySelectorAll('.data-type-option').forEach(option => {
            option.addEventListener('click', (e) => {
                e.stopPropagation();
                const value = option.dataset.value;
                if (option.classList.contains('selected')) {
                    option.classList.remove('selected');
                    selectedDataTypes = selectedDataTypes.filter(t => t !== value);
                } else {
                    option.classList.add('selected');
                    selectedDataTypes.push(value);
                }
                const dataTypeText = document.querySelector('.data-type-text');
                if (dataTypeText) {
                    if (selectedDataTypes.length === 0) {
                        dataTypeText.textContent = 'Выберите тип данных';
                    } else {
                        dataTypeText.textContent = `Выбрано: ${selectedDataTypes.join(', ')}`;
                    }
                }
            });
        });
    }

    if (methodBtn && methodDropdown) {
        methodBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            methodDropdown.classList.toggle('show');
            const arrow = methodBtn.querySelector('.method-arrow');
            if (arrow) arrow.classList.toggle('rotated');
        });
        const options = methodDropdown.querySelectorAll('.method-option');
        options.forEach(option => {
            option.addEventListener('click', (e) => {
                e.stopPropagation();
                const value = option.dataset.value;
                const displayText = option.querySelector('span')?.textContent || option.textContent;
                options.forEach(opt => opt.classList.remove('selected'));
                option.classList.add('selected');
                selectedMethod = { value, displayText };
                const methodText = document.querySelector('.method-text');
                if (methodText) methodText.textContent = displayText;
                methodDropdown.classList.remove('show');
                const arrow = methodBtn.querySelector('.method-arrow');
                if (arrow) arrow.classList.remove('rotated');
            });
        });
    }

    document.addEventListener('click', (e) => {
        if (dataTypeBtn && !dataTypeBtn.contains(e.target) && dataTypeDropdown && !dataTypeDropdown.contains(e.target)) {
            dataTypeDropdown.classList.remove('show');
            const arrow = dataTypeBtn.querySelector('.data-type-arrow');
            if (arrow) arrow.classList.remove('rotated');
        }
        if (methodBtn && !methodBtn.contains(e.target) && methodDropdown && !methodDropdown.contains(e.target)) {
            methodDropdown.classList.remove('show');
            const arrow = methodBtn.querySelector('.method-arrow');
            if (arrow) arrow.classList.remove('rotated');
        }
    });
}

// ==================== ОСНОВНЫЕ ФУНКЦИИ ====================
function updateScrollbars() {
    if (window.updateDialogsScrollbar) window.updateDialogsScrollbar();
    if (window.updateMessagesScrollbar) window.updateMessagesScrollbar();
}

function toggleDialogsPanel() {
    const dialogsPanel = document.getElementById('dialogsPanel');
    const hideDialogsBtn = document.getElementById('hideDialogsBtn');
    const showDialogsBtn = document.getElementById('showDialogsBtn');
    if (!dialogsPanel) return;

    isDialogsHidden = !isDialogsHidden;
    if (isDialogsHidden) {
        dialogsPanel.classList.add('hidden');
        if (hideDialogsBtn) hideDialogsBtn.style.display = 'none';
        if (showDialogsBtn) showDialogsBtn.style.display = 'inline-flex';
        const mainLayout = document.querySelector('.main-layout');
        if (mainLayout) mainLayout.style.gap = '0';
    } else {
        dialogsPanel.classList.remove('hidden');
        if (hideDialogsBtn) hideDialogsBtn.style.display = 'inline-flex';
        if (showDialogsBtn) showDialogsBtn.style.display = 'none';
        const mainLayout = document.querySelector('.main-layout');
        if (mainLayout) mainLayout.style.gap = '20px';
    }
}

function showDialogsPanel() {
    const dialogsPanel = document.getElementById('dialogsPanel');
    const hideDialogsBtn = document.getElementById('hideDialogsBtn');
    const showDialogsBtn = document.getElementById('showDialogsBtn');
    if (!dialogsPanel) return;

    if (isDialogsHidden) {
        isDialogsHidden = false;
        dialogsPanel.classList.remove('hidden');
        if (hideDialogsBtn) hideDialogsBtn.style.display = 'inline-flex';
        if (showDialogsBtn) showDialogsBtn.style.display = 'none';
        const mainLayout = document.querySelector('.main-layout');
        if (mainLayout) mainLayout.style.gap = '20px';
    }
}

function sendMessage() {
    const messageInput = document.getElementById('messageInput');
    const emptyMessageInput = document.getElementById('emptyMessageInput');
    const text = (messageInput && messageInput.value.trim()) ? messageInput.value.trim() :
        (emptyMessageInput ? emptyMessageInput.value.trim() : '');
    if (text && currentDialogId) {
        addMessage(text, 'sent');
        if (messageInput) messageInput.value = '';
        if (emptyMessageInput) emptyMessageInput.value = '';
    } else if (!currentDialogId) {
        alert('Сначала создайте или выберите диалог');
    }
}

function handleClickOutside(event) {
    const profileChatBtn = document.getElementById('profileChatBtn');
    const profileDropdown = document.getElementById('profileDropdown');
    const profileChatBtnCreation = document.getElementById('profileChatBtnCreation');
    const profileDropdownCreation = document.getElementById('profileDropdownCreation');

    if (profileChatBtn && profileDropdown && !profileChatBtn.contains(event.target) && !profileDropdown.contains(event.target)) {
        profileDropdown.classList.remove('show');
    }
    if (profileChatBtnCreation && profileDropdownCreation && !profileChatBtnCreation.contains(event.target) && !profileDropdownCreation.contains(event.target)) {
        profileDropdownCreation.classList.remove('show');
    }
}

function initEventListeners() {
    const hideDialogsBtn = document.getElementById('hideDialogsBtn');
    const showDialogsBtn = document.getElementById('showDialogsBtn');
    const profileChatBtn = document.getElementById('profileChatBtn');
    const profileChatBtnCreation = document.getElementById('profileChatBtnCreation');
    const backFromCreationBtn = document.getElementById('backFromCreationBtn');
    const saveProfileBtn = document.getElementById('saveProfileBtn');
    const sendBtn = document.getElementById('sendBtn');
    const emptySendBtn = document.getElementById('emptySendBtn');
    const newChatBtn = document.getElementById('newChatBtn');
    const userProfileBtn = document.getElementById('userProfileBtn');
    const messageInput = document.getElementById('messageInput');
    const emptyMessageInput = document.getElementById('emptyMessageInput');

    if (hideDialogsBtn) hideDialogsBtn.addEventListener('click', toggleDialogsPanel);
    if (showDialogsBtn) showDialogsBtn.addEventListener('click', showDialogsPanel);
    if (profileChatBtn) profileChatBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        toggleProfileDropdown(document.getElementById('profileDropdown'));
    });
    if (profileChatBtnCreation) profileChatBtnCreation.addEventListener('click', (e) => {
        e.stopPropagation();
        toggleProfileDropdown(document.getElementById('profileDropdownCreation'));
    });
    if (backFromCreationBtn) backFromCreationBtn.addEventListener('click', closeProfileCreation);
    if (saveProfileBtn) saveProfileBtn.addEventListener('click', saveNewProfile);
    if (sendBtn) sendBtn.addEventListener('click', sendMessage);
    if (emptySendBtn) emptySendBtn.addEventListener('click', sendMessage);
    if (messageInput) messageInput.addEventListener('keypress', (e) => { if (e.key === 'Enter') sendMessage(); });
    if (emptyMessageInput) emptyMessageInput.addEventListener('keypress', (e) => { if (e.key === 'Enter') sendMessage(); });
    if (newChatBtn) newChatBtn.addEventListener('click', createNewDialog);
    if (userProfileBtn) userProfileBtn.addEventListener('click', () => {});
    document.addEventListener('click', handleClickOutside);
}

function initDOMElements() {
    // Все DOM элементы уже используются напрямую в функциях
    // Эта функция оставлена для совместимости
}

function initScrollbars() {
    const dialogsHistory = document.getElementById('dialogsHistory');
    const dialogsWrapper = document.querySelector('.dialogs-history-wrapper');
    const messagesArea = document.getElementById('messagesArea');
    const messagesWrapper = document.getElementById('messagesWrapper');

    if (dialogsHistory && dialogsWrapper) {
        window.updateDialogsScrollbar = createCustomScrollbar(dialogsHistory, dialogsWrapper);
    }
    if (messagesArea && messagesWrapper) {
        window.updateMessagesScrollbar = createCustomScrollbar(messagesArea, messagesWrapper);
    }
}

function init() {
    console.log('Sanitizer app initializing...');
    initDOMElements();
    createInitialDialog();
    updateProfileDropdowns();
    updateProfileButtonText();
    renderDialogs();
    renderMessages();
    initEventListeners();
    initCollapseListeners();
    initDataMethodHandlers();
    showChatPanel();
    initScrollbars();
    console.log('Sanitizer app initialized');
}

// Запуск после загрузки DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}