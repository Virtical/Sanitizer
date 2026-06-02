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

async function sendMessage() {
    const messageInput = document.getElementById('messageInput');
    const emptyMessageInput = document.getElementById('emptyMessageInput');
    const text = (messageInput && messageInput.value.trim()) ? messageInput.value.trim() :
        (emptyMessageInput ? emptyMessageInput.value.trim() : '');
    
    if (text) {
        await addMessage(text);
        if (messageInput) messageInput.value = '';
        if (emptyMessageInput) emptyMessageInput.value = '';
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
    if (messageInput) {
        messageInput.addEventListener('input', () => {
            updateSendButtonState();
            autoResizeTextarea(messageInput);
        });
        messageInput.addEventListener('keypress', async (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                await sendMessage();
            }
        });
    }
    if (emptyMessageInput) {
        emptyMessageInput.addEventListener('input', () => {
            updateSendButtonState();
            autoResizeTextarea(emptyMessageInput);
        });
        emptyMessageInput.addEventListener('keypress', async (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                await sendMessage();
            }
        });
    }
    if (newChatBtn) newChatBtn.addEventListener('click', createNewDialog);
    if (userProfileBtn) userProfileBtn.addEventListener('click', () => {});
    document.addEventListener('click', handleClickOutside);
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

async function init() {
    await loadDialogs();
    createNewDialog();
    await initProfiles();
    renderDialogs();
    initEventListeners();
    initCollapseListeners();
    initDataMethodHandlers();
    initEditHandlers();
    showChatPanel();
    initScrollbars();
    updateSendButtonState();
}

// Запуск после загрузки DOM
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => init());
} else {
    (async () => {
        await init();
    })();
}
