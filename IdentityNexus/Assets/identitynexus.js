window.identityNexus = {
    ready: function () {
        var boot = document.getElementById('nx-boot');
        if (boot) {
            boot.remove();
        }
    },
    theme: function () {
        return document.documentElement.getAttribute('data-theme') || 'light';
    },
    toggleTheme: function () {
        var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        try { localStorage.setItem('identitynexus.theme', next); } catch (e) { }
        return next;
    }
};
