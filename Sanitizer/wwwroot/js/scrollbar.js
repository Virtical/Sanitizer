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
