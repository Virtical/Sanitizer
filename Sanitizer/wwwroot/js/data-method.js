// ==================== ВЫБОР ТИПА ДАННЫХ И МЕТОДА ====================

let profileRules = [];

function initDataMethodHandlers() {
    const addDataTypeBtn = document.getElementById('addDataTypeBtn');
    const backBtn = document.getElementById('backFromCreationBtn');
    const profileNameInput = document.getElementById('newProfileName');

    profileNameInput.addEventListener('input', updateSaveButtonState);
    addDataTypeBtn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        openDataTypeModal();
    });
    backBtn.addEventListener('click', () => {
        closeProfileCreation();
        resetProfileCreationForm();
    });

    initModalHandlers();
    updateSaveButtonState();
}

// Инициализация обработчиков модального окна
function initModalHandlers() {
    // Закрытие по оверлею
    const modalOverlay = document.querySelector('.modal-overlay');
    modalOverlay.addEventListener('click', closeDataTypeModal);

    // Обработчики выбора типа данных
    document.querySelectorAll('.data-type-option').forEach(option => {
        option.addEventListener('click', () => {
            const dataType = option.dataset.value;
            const titleSpan = option.querySelector('.option-title');
            const dataTypeDisplay = titleSpan ? titleSpan.textContent : dataType;
            addRuleCard(dataType, dataTypeDisplay);
            closeDataTypeModal();
        });
    });

    // Кнопка закрытия
    const closeBtn = document.querySelector('#closeDataTypeModalBtn');
    closeBtn.addEventListener('click', closeDataTypeModal);
}

// Открытие модального окна выбора типа данных
function openDataTypeModal() {
    const modal = document.getElementById('dataTypeModal');
    updateAvailableDataTypes();
    modal.style.display = 'flex';
}

// Закрытие модального окна
function closeDataTypeModal() {
    const modal = document.getElementById('dataTypeModal');
    modal.style.display = 'none';
}

// Обновление доступных опций в модальном окне (скрываем уже выбранные типы)
function updateAvailableDataTypes() {
    // Получаем все уже выбранные типы данных из карточек
    const selectedTypes = new Set();
    const cards = document.querySelectorAll('.rule-card');
    cards.forEach(card => {
        const dataType = card.dataset.dataType;
        if (dataType) {
            selectedTypes.add(dataType);
        }
    });

    // Обновляем отображение опций в модальном окне
    const modalOptions = document.querySelectorAll('.data-type-option');
    modalOptions.forEach(option => {
        const dataType = option.dataset.value;
        if (selectedTypes.has(dataType)) {
            // Уже выбранный тип - скрываем
            option.style.display = 'none';
        } else {
            // Доступный для выбора - показываем
            option.style.display = 'flex';
        }
    });
}

// Добавление карточки правила
function addRuleCard(dataType, dataTypeDisplay) {
    // Показываем заголовок правил, если он скрыт
    const rulesHeader = document.getElementById('rulesHeader');
    if (rulesHeader) {
        rulesHeader.style.display = 'block';
    }

    const rulesContainer = document.getElementById('rulesContainer');
    if (!rulesContainer) return;

    const existingCard = document.querySelector(`.rule-card[data-data-type="${dataType}"]`);
    if (existingCard) {
        return;
    }

    // Создаём новую карточку из шаблона
    const template = document.getElementById('rule-card-template');
    const cardClone = template.content.cloneNode(true);

    const ruleCard = cardClone.querySelector('.rule-card');
    const ruleIcon = cardClone.querySelector('.rule-icon');
    const ruleTypeSpan = cardClone.querySelector('.rule-type');
    const methodSelectorBtn = cardClone.querySelector('.method-btn');
    const methodSelectorText = cardClone.querySelector('.method-text');
    const methodDropdown = cardClone.querySelector('.method-dropdown');
    const deleteBtn = cardClone.querySelector('.delete-rule-btn');
    const moveUpBtn = cardClone.querySelector('.move-up-btn');
    const moveDownBtn = cardClone.querySelector('.move-down-btn');

    // Устанавливаем иконку в зависимости от типа данных
    const iconMap = {
        'Email': 'images/email-icon.svg',
        'Phone': 'images/phone-icon.svg',
        'Card': 'images/card-icon.svg',
        'IpAddress': 'images/ip-icon.svg',
        'Guid': 'images/uuid-icon.svg',
        'Name': 'images/person.svg',
        'Url': 'images/url-icon.svg',
        'ApiKey': 'images/api-icon.svg',
        'Context': 'images/context-icon.svg',
        'Regex': 'images/regex-icon.svg'
    };

    ruleIcon.src = iconMap[dataType] || 'images/default-icon.svg';
    ruleIcon.alt = dataTypeDisplay;

    // Устанавливаем тип данных (только название без скобок)
    ruleTypeSpan.textContent = dataTypeDisplay;

    // Сохраняем тип данных в data-атрибут
    ruleCard.dataset.dataType = dataType;

    // Обработчик удаления карточки
    deleteBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        ruleCard.remove();
        updateProfileRulesArray();
        updateMoveButtonsState();
        updateAvailableDataTypes();
        updateSaveButtonState();

        const remainingCards = document.querySelectorAll('.rule-card');
        if (remainingCards.length === 0 && rulesHeader) {
            rulesHeader.style.display = 'none';
        }
    });

    // Обработчик перемещения вверх
    moveUpBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        const prevCard = ruleCard.previousElementSibling;
        if (prevCard && prevCard.classList.contains('rule-card')) {
            rulesContainer.insertBefore(ruleCard, prevCard);
            updateMoveButtonsState();
            updateProfileRulesArray();
        }
    });

    // Обработчик перемещения вниз
    moveDownBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        const nextCard = ruleCard.nextElementSibling;
        if (nextCard && nextCard.classList.contains('rule-card')) {
            rulesContainer.insertBefore(nextCard, ruleCard);
            updateMoveButtonsState();
            updateProfileRulesArray();
        }
    });

    // Обработчик открытия/закрытия выпадающего списка методов
    methodSelectorBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        document.querySelectorAll('.method-dropdown.show').forEach(dd => {
            if (dd !== methodDropdown) dd.classList.remove('show');
        });
        document.querySelectorAll('.method-btn.active').forEach(btn => {
            if (btn !== methodSelectorBtn) btn.classList.remove('active');
        });

        methodDropdown.classList.toggle('show');
        methodSelectorBtn.classList.toggle('active');
    });

    // Обработчики выбора метода
    const methodOptions = cardClone.querySelectorAll('.method-option');
    methodOptions.forEach(option => {
        option.addEventListener('click', () => {
            const methodValue = option.dataset.value;
            methodSelectorText.textContent = option.textContent;
            methodSelectorText.classList.add('selected');
            methodDropdown.classList.remove('show');
            methodSelectorBtn.classList.remove('active');
            ruleCard.dataset.method = methodValue;
            updateProfileRulesArray();
            updateSaveButtonState();
        });
    });

    rulesContainer.appendChild(cardClone);
    updateMoveButtonsState();
    updateProfileRulesArray();
    updateAvailableDataTypes();
    updateSaveButtonState();
}

// Обновление состояния кнопок перемещения и видимости блока
function updateMoveButtonsState() {
    const cards = document.querySelectorAll('.rule-card');
    const cardsCount = cards.length;

    cards.forEach((card, index) => {
        const moveButtons = card.querySelector('.move-buttons');
        const moveUpBtn = card.querySelector('.move-up-btn');
        const moveDownBtn = card.querySelector('.move-down-btn');

        // Показываем блок со стрелками только если карточек 2 или больше
        if (cardsCount >= 2) {
            moveButtons.classList.add('visible');
        } else {
            moveButtons.classList.remove('visible');
        }

        // Если карточка первая - верхняя стрелка серая
        if (index === 0) {
            moveUpBtn.classList.add('disabled');
        } else {
            moveUpBtn.classList.remove('disabled');
        }

        // Если карточка последняя - нижняя стрелка серая
        if (index === cardsCount - 1) {
            moveDownBtn.classList.add('disabled');
        } else {
            moveDownBtn.classList.remove('disabled');
        }
    });
}

function updateProfileRulesArray() {
    profileRules = [];
    const cards = document.querySelectorAll('.rule-card');

    cards.forEach(card => {
        const dataType = card.dataset.dataType;
        const method = card.dataset.method;

        if (dataType && method) {
            profileRules.push({
                type: dataType,
                strategy: method
            });
        }
    });
}

// Обновление состояния кнопки "Сохранить профиль"
function updateSaveButtonState() {
    const profileNameInput = document.getElementById('newProfileName');
    const saveBtn = document.getElementById('saveProfileBtn');

    if (!saveBtn || !profileNameInput) return;

    const hasName = profileNameInput.value.trim().length > 0;

    const cards = document.querySelectorAll('.rule-card');
    let allCardsHaveMethod = true;

    if (cards.length > 0) {
        cards.forEach(card => {
            const method = card.dataset.method;
            if (!method) {
                allCardsHaveMethod = false;
            }
        });
    }

    if (hasName && allCardsHaveMethod) {
        saveBtn.classList.add('active');
    } else {
        saveBtn.classList.remove('active');
    }
}

// Сброс формы создания профиля
function resetProfileCreationForm() {
    const elements = {
        nameInput: document.getElementById('newProfileName'),
        rulesContainer: document.getElementById('rulesContainer'),
        rulesHeader: document.getElementById('rulesHeader'),
        addBtn: document.getElementById('addDataTypeBtn'),
        saveBtn: document.getElementById('saveProfileBtn')
    };

    if (elements.nameInput) elements.nameInput.value = '';
    if (elements.rulesContainer) elements.rulesContainer.innerHTML = '';
    if (elements.rulesHeader) elements.rulesHeader.style.display = 'none';
    if (elements.saveBtn) {
        elements.saveBtn.classList.remove('active');
        elements.saveBtn.style.background = '';
    }
    if (elements.addBtn) {
        elements.addBtn.style.background = '';
        elements.addBtn.style.backgroundColor = '';
    }

    profileRules = [];

    const saveBtn = document.getElementById('saveProfileBtn');
    if (saveBtn) {
        saveBtn.classList.remove('active');
    }

    updateAvailableDataTypes();
    updateSaveButtonState();
}

// Закрытие дропдаунов и модального окна при клике вне
document.addEventListener('click', (e) => {
    const modal = document.getElementById('dataTypeModal');
    const methodDropdowns = document.querySelectorAll('.method-dropdown');

    // Закрытие дропдаунов методов при клике вне
    methodDropdowns.forEach(dropdown => {
        const btn = dropdown.previousElementSibling;
        if (btn && !btn.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.remove('show');
            btn.classList.remove('active');
        }
    });

    // Закрытие модального окна при клике вне его содержимого
    if (modal && modal.style.display === 'flex') {
        const modalContent = modal.querySelector('.data-type-btn');
        if (modalContent && !modalContent.contains(e.target)) {
            closeDataTypeModal();
        }
    }
});

