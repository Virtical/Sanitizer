function openProfileEdit() {
    const editPanel = document.getElementById('profileEditPanel');
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');
    const editRulesContainer = document.getElementById('editRulesContainer');
    
    editRulesContainer.innerHTML = '';
    if (chatPanel) chatPanel.style.display = 'none';
    if (profileCreationPanel) profileCreationPanel.style.display = 'none';
    if (editPanel) editPanel.style.display = 'flex';

    loadProfileForEdit();
}

// Закрытие страницы редактирования
function closeProfileEdit() {
    const editPanel = document.getElementById('profileEditPanel');
    const chatPanel = document.getElementById('chatPanel');
    const editRulesContainer = document.getElementById('editRulesContainer');

    editRulesContainer.innerHTML = '';
    editPanel.style.display = 'none';
    chatPanel.style.display = 'flex';
    
    updateAvailableDataTypes();
}

function loadProfileForEdit() {
    if (!currentProfile) return;

    const editNameInput = document.getElementById('editProfileName');
    if (editNameInput) editNameInput.value = currentProfile.name;

    const rulesContainer = document.getElementById('editRulesContainer');
    if (rulesContainer) rulesContainer.innerHTML = '';

    const rules = currentProfile.rules || {};
    const ruleTypes = Object.keys(rules);

    if (ruleTypes.length > 0) {
        const rulesHeader = document.getElementById('editRulesHeader');
        if (rulesHeader) rulesHeader.style.display = 'block';

        ruleTypes.forEach(ruleType => {
            const rule = rules[ruleType];
            addEditRuleCard(ruleType, ruleType, rule.strategy);
        });
    } else {
        const rulesHeader = document.getElementById('editRulesHeader');
        if (rulesHeader) rulesHeader.style.display = 'none';
    }

    updateEditSaveButtonState();
    updateEditMoveButtonsState();
    updateEditAvailableDataTypes();
}

// Добавление карточки правила в режиме редактирования
function addEditRuleCard(dataType, dataTypeDisplay, selectedMethod = null) {
    const rulesHeader = document.getElementById('editRulesHeader');
    if (rulesHeader) rulesHeader.style.display = 'block';

    const rulesContainer = document.getElementById('editRulesContainer');
    if (!rulesContainer) return;

    const existingCard = document.querySelector(`#editRulesContainer .rule-card[data-data-type="${dataType}"]`);
    if (existingCard) return;

    const template = document.getElementById('rule-card-template');
    const cardClone = template.content.cloneNode(true);

    const ruleCard = cardClone.querySelector('.rule-card');
    const ruleIcon = cardClone.querySelector('.rule-icon');
    const ruleTypeSpan = cardClone.querySelector('.rule-type');
    const methodSelectorText = cardClone.querySelector('.method-text');
    const methodSelectorBtn = cardClone.querySelector('.method-btn');
    const methodDropdown = cardClone.querySelector('.method-dropdown');
    const deleteBtn = cardClone.querySelector('.delete-rule-btn');
    const moveUpBtn = cardClone.querySelector('.move-up-btn');
    const moveDownBtn = cardClone.querySelector('.move-down-btn');

    const iconMap = {
        'Email': 'bi-envelope',
        'Phone': 'bi-telephone',
        'Card': 'bi-credit-card',
        'IpAddress': 'bi-globe',
        'Guid': 'bi-person-lines-fill',
        'Name': 'bi-person',
        'Url': 'bi-link-45deg',
        'ApiKey': 'bi-key',
        'Context': 'bi-lock',
        'Regex': 'bi-regex'
    };

    ruleIcon.className = `bi ${iconMap[dataType] || 'bi-question-circle'} rule-icon`;
    ruleTypeSpan.textContent = dataTypeDisplay;
    ruleCard.dataset.dataType = dataType;

    if (selectedMethod) {
        const selectedOption = Array.from(cardClone.querySelectorAll('.method-option')).find(
            opt => opt.dataset.value === selectedMethod
        );
        if (selectedOption) {
            methodSelectorText.textContent = selectedOption.textContent;
            methodSelectorText.classList.add('selected');
            ruleCard.dataset.method = selectedMethod;
        }
    }

    // Обработчик удаления карточки
    deleteBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        ruleCard.remove();
        updateEditMoveButtonsState();
        updateEditAvailableDataTypes();
        updateEditSaveButtonState();

        const remainingCards = document.querySelectorAll('#editRulesContainer .rule-card');
        if (remainingCards.length === 0) rulesHeader.style.display = 'none';
    });

    // Обработчик перемещения вверх
    moveUpBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        const prevCard = ruleCard.previousElementSibling;
        if (prevCard && prevCard.classList.contains('rule-card')) {
            rulesContainer.insertBefore(ruleCard, prevCard);
            updateEditMoveButtonsState();
        }
    });

    // Обработчик перемещения вниз
    moveDownBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        const nextCard = ruleCard.nextElementSibling;
        if (nextCard && nextCard.classList.contains('rule-card')) {
            rulesContainer.insertBefore(nextCard, ruleCard);
            updateEditMoveButtonsState();
        }
    });

    // Выпадающий список методов
    methodSelectorBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        document.querySelectorAll('#editRulesContainer .method-dropdown.show').forEach(dd => {
            if (dd !== methodDropdown) dd.classList.remove('show');
        });
        document.querySelectorAll('#editRulesContainer .method-btn.active').forEach(btn => {
            if (btn !== methodSelectorBtn) btn.classList.remove('active');
        });
        methodDropdown.classList.toggle('show');
        methodSelectorBtn.classList.toggle('active');
    });

    // Выбор метода
    const methodOptions = cardClone.querySelectorAll('.method-option');
    methodOptions.forEach(option => {
        option.addEventListener('click', () => {
            const methodValue = option.dataset.value;
            methodSelectorText.textContent = option.textContent;
            methodSelectorText.classList.add('selected');
            methodDropdown.classList.remove('show');
            methodSelectorBtn.classList.remove('active');
            ruleCard.dataset.method = methodValue;
            updateEditSaveButtonState();
        });
    });

    rulesContainer.appendChild(cardClone);
    updateEditMoveButtonsState();
    updateEditAvailableDataTypes();
    updateEditSaveButtonState();
}

// Обновление доступных опций в модальном окне редактирования
function updateEditAvailableDataTypes() {
    const selectedTypes = new Set();
    const rulesContainer = document.getElementById('editRulesContainer');
    const cards = rulesContainer.querySelectorAll('.rule-card');
    cards.forEach(card => {
        const dataType = card.dataset.dataType;
        if (dataType) selectedTypes.add(dataType);
    });
    
    const modalOptions = document.querySelectorAll('#editDataTypeModal .data-type-option');
    modalOptions.forEach(option => {
        const dataType = option.dataset.value;
        if (selectedTypes.has(dataType)) {
            option.style.display = 'none';
        } else {
            option.style.display = 'flex';
        }
    });
}

// Обновление состояния кнопок перемещения
function updateEditMoveButtonsState() {
    const cards = document.querySelectorAll('#editRulesContainer .rule-card');
    const cardsCount = cards.length;

    cards.forEach((card, index) => {
        const moveButtons = card.querySelector('.move-buttons');
        const moveUpBtn = card.querySelector('.move-up-btn');
        const moveDownBtn = card.querySelector('.move-down-btn');

        if (cardsCount >= 2) {
            moveButtons.classList.add('visible');
        } else {
            moveButtons.classList.remove('visible');
        }

        if (index === 0) {
            moveUpBtn.classList.add('disabled');
        } else {
            moveUpBtn.classList.remove('disabled');
        }

        if (index === cardsCount - 1) {
            moveDownBtn.classList.add('disabled');
        } else {
            moveDownBtn.classList.remove('disabled');
        }
    });
}

// Обновление состояния кнопки сохранения
function updateEditSaveButtonState() {
    const editNameInput = document.getElementById('editProfileName');
    const saveBtn = document.getElementById('saveEditBtn');
    
    const cards = document.querySelectorAll('#editRulesContainer .rule-card');
    let allHaveMethod = true;
    
    cards.forEach(card => {
        if (!card.dataset.method) allHaveMethod = false;
    });

    if ((editNameInput.value.trim().length > 0) && (cards.length === 0 || allHaveMethod)) {
        saveBtn.classList.add('active');
    } else {
        saveBtn.classList.remove('active');
    }
    
}

// Открытие модального окна для редактирования
function openEditDataTypeModal() {
    const modal = document.getElementById('editDataTypeModal');
    updateEditAvailableDataTypes();
    modal.style.display = 'flex';
}

// Сохранение отредактированного профиля
async function saveEditedProfile() {
    const editNameInput = document.getElementById('editProfileName');
    const saveBtn = document.getElementById('saveEditBtn');

    if (!saveBtn.classList.contains('active')) return;

    const rules = {};
    const cards = document.querySelectorAll('#editRulesContainer .rule-card');

    let allHaveMethod = true;
    cards.forEach(card => {
        if (!card.dataset.method) allHaveMethod = false;
    });

    cards.forEach((card) => {
        const dataType = card.dataset.dataType;
        const method = card.dataset.method;
        if (dataType && method) {
            rules[dataType] = {
                strategy: method
            };
        }
    });

    const payload = {
        name: editNameInput.value.trim(),
        rules: rules
    };

    try {
        const profileId = currentProfile.id;

        allProfiles = await apiUpdateProfile(profileId, payload);

        const updatedProfile = allProfiles.find(p => p.id === profileId);
        if (updatedProfile) currentProfile = updatedProfile;

        updateProfileButtonText();
        updateProfileDropdowns();

        if (currentDialogId && currentProfile) {
            try {
                await apiUpdateDialogProfile(currentDialogId, currentProfile.id);
            } catch (error) {
                console.error('Ошибка привязки профиля к диалогу:', error);
            }
        }

        closeProfileEdit();

    } catch (e) {
        console.error('Ошибка сохранения профиля:', e);
    }
}
// Сброс формы редактирования
function resetEditForm() {
    const editNameInput = document.getElementById('editProfileName');
    const rulesContainer = document.getElementById('editRulesContainer');
    const rulesHeader = document.getElementById('editRulesHeader');

    editNameInput.value = '';
    rulesContainer.innerHTML = '';
    rulesHeader.style.display = 'none';
}

// Инициализация обработчиков редактирования
function initEditHandlers() {
    const addDataTypeBtn = document.getElementById('editAddDataTypeBtn');
    const cancelBtn = document.getElementById('cancelEditBtn');
    const saveBtn = document.getElementById('saveEditBtn');
    const editNameInput = document.getElementById('editProfileName');
    const modal = document.getElementById('editDataTypeModal');
    const modalOverlay = document.querySelector('#editDataTypeModal .modal-overlay');
    const closeBtn = document.querySelector('#editDataTypeModal .data-type-close-btn');

    addDataTypeBtn.addEventListener('click', openEditDataTypeModal);
    modalOverlay.addEventListener('click', () => modal.style.display = 'none');
    closeBtn.addEventListener('click', () => {modal.style.display = 'none'});
    editNameInput.addEventListener('input', updateEditSaveButtonState);
    saveBtn.addEventListener('click', saveEditedProfile);
    cancelBtn.addEventListener('click', () => {
        closeProfileEdit();
        resetEditForm();
    });
    
    // Обработчики выбора типа данных в модальном окне
    const modalOptions = modal?.querySelectorAll('.data-type-option');
    modalOptions.forEach(option => {
        option.addEventListener('click', () => {
            const dataType = option.dataset.value;
            const titleSpan = option.querySelector('.option-title');
            const dataTypeDisplay = titleSpan ? titleSpan.textContent.split(' ')[0] : dataType;
            addEditRuleCard(dataType, dataTypeDisplay);
            modal.style.display = 'none';
            updateEditSaveButtonState();
        });
    });
}