// ============================================================
// TRACKNGO MATI — GLOBAL JAVASCRIPT UTILITIES
// ============================================================

document.addEventListener('DOMContentLoaded', function() {
    // Focus management for form inputs
    const inputs = document.querySelectorAll('.form-input, .form-select, .form-textarea');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement?.classList?.add('is-focused');
        });
        input.addEventListener('blur', function() {
            this.parentElement?.classList?.remove('is-focused');
        });
    });

    // Button ripple effect
    const buttons = document.querySelectorAll('.btn, .login-btn');
    buttons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            ripple.style.cssText = `
                position:absolute;
                width:0;height:0;
                border-radius:50%;
                background:rgba(255,255,255,0.3);
                transform:translate(-50%,-50%);
                pointer-events:none;
                left:${e.clientX - rect.left}px;
                top:${e.clientY - rect.top}px;
            `;
            this.style.position = this.style.position || 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);

            ripple.animate([
                { width: '0px', height: '0px', opacity: 1 },
                { width: '200px', height: '200px', opacity: 0 }
            ], { duration: 400, easing: 'ease-out' });

            setTimeout(() => ripple.remove(), 400);
        });
    });
});

// ============================================================
// SIDEBAR NAVIGATION
// ============================================================

function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const isMobile = window.innerWidth <= 900;

    if (isMobile) {
        sidebar.classList.toggle('mobile-open');
        overlay.classList.toggle('active');
        document.body.style.overflow = sidebar.classList.contains('mobile-open') ? 'hidden' : '';
    } else {
        document.body.classList.toggle('sidebar-collapsed');
        // Save preference
        localStorage.setItem('sidebar-collapsed', document.body.classList.contains('sidebar-collapsed'));
    }
}

function closeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    if (sidebar) {
        sidebar.classList.remove('mobile-open');
    }
    if (overlay) {
        overlay.classList.remove('active');
    }
    document.body.style.overflow = '';
}

// Restore sidebar state on load
document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return; // Login page has no sidebar

    // Restore collapse preference on desktop
    if (window.innerWidth > 900) {
        const collapsed = localStorage.getItem('sidebar-collapsed') === 'true';
        if (collapsed) {
            document.body.classList.add('sidebar-collapsed');
        }
    }

    // Close sidebar on window resize past mobile breakpoint
    window.addEventListener('resize', function() {
        if (window.innerWidth > 900) {
            closeSidebar();
        }
    });

    // Close sidebar on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeSidebar();
        }
    });
});

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

function formatDate(date) {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(date).toLocaleDateString('en-US', options);
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.style.cssText = `
        position:fixed;bottom:80px;left:50%;transform:translateX(-50%);
        padding:10px 24px;border-radius:4px;font-family:var(--font);
        font-size:13px;font-weight:500;z-index:5000;
        box-shadow:0 4px 16px rgba(0,0,0,0.15);
        color:#FFFFFF;
        background:${type === 'success' ? '#16A34A' : type === 'error' ? '#DC2626' : '#1A56DB'};
        opacity:0;transition:opacity 0.3s ease;
    `;
    toast.textContent = message;
    document.body.appendChild(toast);

    requestAnimationFrame(() => { toast.style.opacity = '1'; });
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
