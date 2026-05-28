// ==================== УПРАВЛЕНИЕ ПРОФИЛЯМИ ====================
function updateProfileDropdowns() {
    updateProfileDropdown(document.getElementById('profileDropdown'));
    updateProfileDropdown(document.getElementById('profileDropdownCreation'));
}

function updateProfileDropdown(dropdownElement) {
    if (!dropdownElement) return;
    dropdownElement.innerHTML = '';
    
    const profilesList = document.createElement('div');
    profilesList.className = 'profiles-list';
    
    if (currentProfile != null) {
        const selectedClone = cloneTemplate('profile-selected-template');
        if (!selectedClone) return;
        
        const selectedDiv = selectedClone.querySelector('.profile-selected-item');
        const selectedName = selectedClone.querySelector('.profile-selected-name');
        selectedName.textContent = currentProfile.name;
        
        const editIcon = selectedClone.querySelector('.profile-selected-edit');
        editIcon.addEventListener('click', (e) => {
            e.stopPropagation();
            openProfileEdit();
        });
        
        selectedDiv.addEventListener('click', () => {
            dropdownElement.classList.remove('show');
        });
        
        profilesList.appendChild(selectedClone);
    }

    const otherProfiles = allProfiles.filter(p => p.id !== currentProfile.id);
    if (otherProfiles.length > 0) {
        otherProfiles.forEach(profile => {
            const optionClone = cloneTemplate('profile-option-template');
            if (!optionClone) return;
            
            const option = optionClone.querySelector('.profile-option-item');
            const optionName = optionClone.querySelector('.profile-option-name');
            const editIcon = optionClone.querySelector('.profile-option-edit');

            optionName.textContent = profile.name;

            // Обработчик редактирования
            editIcon.addEventListener('click', (e) => {
                e.stopPropagation();
                currentProfile = profile;
                openProfileEdit();
            });

            option.addEventListener('click', async () => {
                currentProfile = profile;
                updateProfileButtonText();
                updateProfileDropdowns();
                dropdownElement.classList.remove('show');
                
                if (currentDialogId) {
                    try {
                        await apiUpdateDialogProfile(currentDialogId, currentProfile.id);
                    } catch (error) {
                        console.error('Ошибка привязки профиля:', error);
                    }
                }
            });
            
            profilesList.appendChild(optionClone);
        });
        
        dropdownElement.appendChild(profilesList);
    }

    if (currentProfile != null) {
        const divider = document.createElement('div');
        divider.className = 'profile-divider-line';
        dropdownElement.appendChild(divider);
    }

    const createBtnClone = cloneTemplate('create-profile-btn-template');
    if (createBtnClone) {
        const createBtn = createBtnClone.querySelector('.create-new-profile-btn');
        createBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdownElement.classList.remove('show');
            openProfileCreation();
        });
        dropdownElement.appendChild(createBtnClone);
    }
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
    const rulesContainer = document.getElementById('rulesContainer');

    rulesContainer.innerHTML = '';
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

async function saveNewProfile() {
    const newProfileNameInput = document.getElementById('newProfileName');
    const saveBtn = document.getElementById('saveProfileBtn');
    const profileName = newProfileNameInput?.value.trim();

    if (!saveBtn.classList.contains('active')) return;
    
    const rulesContainer = document.getElementById('rulesContainer');
    const rules = {};
    const cards = rulesContainer.querySelectorAll('.rule-card');

    cards.forEach(dt => {
        const rule = { strategy: dt.dataset.method };
        if (dt.dataset.dataType === 'Regex') rule.pattern = dt.dataset.pattern || '';
        rules[dt.dataset.dataType] = rule;
    });

    const payload = {
        name: profileName,
        rules
    };

    try {
        allProfiles = await apiCreateProfile(payload);
        currentProfile = allProfiles[allProfiles.length - 1];
        if (currentDialogId && currentProfile) {
            try {
                await apiUpdateDialogProfile(currentDialogId, currentProfile.id);
            } catch (error) {
                console.error('Ошибка привязки нового профиля к диалогу:', error);
            }
        }

        updateProfileDropdowns();
        updateProfileButtonText();
        resetProfileCreationForm();
        showChatPanel();
    } catch (e) {
        console.error(e);
    }
}

function updateProfileButtonText() {
    const profileChatText = document.querySelector('.profile-chat-text');
    if (profileChatText) {
        profileChatText.textContent = currentProfile == null ? "Выберите профиль" : currentProfile.name;
    }
}
