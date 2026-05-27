function openProfileEdit() {
    const editPanel = document.getElementById('profileEditPanel');
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');

    if (chatPanel) chatPanel.style.display = 'none';
    if (profileCreationPanel) profileCreationPanel.style.display = 'none';
    if (editPanel) editPanel.style.display = 'flex';

    loadProfileForEdit();
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
}

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

    rulesContainer.appendChild(cardClone);
}