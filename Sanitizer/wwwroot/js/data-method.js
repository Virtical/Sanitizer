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
