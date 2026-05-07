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
