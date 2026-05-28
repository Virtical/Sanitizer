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

function cloneTemplate(templateId) {
    const template = document.getElementById(templateId);
    if (!template) {
        console.error(`Template not found: ${templateId}`);
        return null;
    }
    return template.content.cloneNode(true);
}
