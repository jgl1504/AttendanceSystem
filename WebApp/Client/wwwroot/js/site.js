window.scrollToElement = function (selector) {
    var el = document.querySelector(selector);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

// Simple info popup for non-working days
window.showInfo = function (message) {
    alert(message);
};
