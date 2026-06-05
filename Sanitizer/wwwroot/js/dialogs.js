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
        const groupClone = cloneTemplate('dialog-group-template');
        if (!groupClone) return;
        
        const titleDiv = groupClone.querySelector('.group-title');
        const dialogsContainer = groupClone.querySelector('.group-dialogs');
        
        titleDiv.textContent = getGroupTitle(group);

        groupDialogs.forEach(dialog => {
            const dialogClone = cloneTemplate('dialog-item-template');
            if (!dialogClone) return;
            
            const dialogDiv = dialogClone.querySelector('.dialog-item');
            const dialogName = dialogClone.querySelector('.dialog-name');
            
            if (dialog.id === currentDialogId) dialogDiv.classList.add('active');
            dialogDiv.dataset.dialogId = dialog.id;
            dialogName.textContent = dialog.name;
            
            dialogDiv.addEventListener('click', async (e) => {
                e.stopPropagation();
                await deleteEmptyDialogIfNeeded();
                currentDialogId = dialog.id;
                isMessageSent = true;
                isSanitizeMode = false;
                originalMessageText = '';
                
                resetAllSanitizeButtons();
                renderDialogs();
                await renderMessages();
                if (isProfileCreationVisible) showChatPanel();
            });
            
            dialogsContainer.appendChild(dialogClone);
        });
        
        dialogsList.appendChild(groupClone);
    });
}

function resetAllSanitizeButtons() {
    const sanitizeToggleBtn = document.getElementById('sanitizeToggleBtn');
    const emptySanitizeToggleBtn = document.getElementById('emptySanitizeToggleBtn');

    [sanitizeToggleBtn, emptySanitizeToggleBtn].forEach(btn => {
        if (btn) {
            const circleIcon = btn.querySelector('i:first-child');
            const eyeIcon = btn.querySelector('.sanitize-icon-overlay');
            circleIcon.style.color = '';
            eyeIcon.className = 'bi bi-eye-slash sanitize-icon-overlay';
            eyeIcon.style.color = '';
        }
    });
}

async function deleteEmptyDialogIfNeeded() {
    if (currentDialogId && !isMessageSent) {
        try {
            const dialogData = await apiGetMessages(currentDialogId);
            if (dialogData.messages.length === 0) await apiDeleteDialog(currentDialogId);
        } catch (error) {
            console.error('Ошибка при проверке/удалении диалога:', error);
        }
    }
}

function createNewDialog() {
    if (currentDialogId && !isMessageSent) apiDeleteDialog(currentDialogId).catch(console.error);
    
    isMessageSent = false;
    isSanitizeMode = false;
    originalMessageText = '';
    isDialogAddedToList = false;
    currentDialogId = null;
    const messagesArea = document.getElementById('messagesArea');
    const emptyState = document.getElementById('chatEmptyState');
    const messagesWrapper = document.getElementById('messagesWrapper');

    messagesArea.innerHTML = '';
    emptyState.style.display = 'flex';
    messagesWrapper.style.display = 'none';
    
    resetAllSanitizeButtons();
    renderDialogs();
}

async function loadDialogs() {
    try {
        dialogs = await apiGetDialog();
        renderDialogs();
    } catch (error) {
        console.error('Ошибка загрузки диалогов:', error);
        dialogs = [];
        renderDialogs();
    }
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
