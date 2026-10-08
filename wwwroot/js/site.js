/* ============================================================
   HospitalMS — Client-side Interactions
   Live Theme + Animations + UX Enhancements
   ============================================================ */

(function () {
  'use strict';

  // ---------- Theme Engine ----------
  const THEME_KEY = 'hms-theme';

  function getPreferredTheme() {
    const stored = localStorage.getItem(THEME_KEY);
    if (stored) return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem(THEME_KEY, theme);
    updateThemeIcon(theme);
  }

  function updateThemeIcon(theme) {
    const thumb = document.querySelector('.theme-toggle .thumb');
    if (thumb) {
      thumb.innerHTML = theme === 'dark'
        ? '<i class="bi bi-moon-fill"></i>'
        : '<i class="bi bi-sun-fill"></i>';
    }
  }

  function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme') || 'light';
    applyTheme(current === 'dark' ? 'light' : 'dark');
  }

  // Apply immediately to avoid flash
  applyTheme(getPreferredTheme());

  // ---------- DOM Ready ----------
  document.addEventListener('DOMContentLoaded', function () {
    // Theme toggle button
    const themeBtn = document.querySelector('.theme-toggle');
    if (themeBtn) {
      themeBtn.addEventListener('click', toggleTheme);
    }

    // Sidebar collapse
    const sidebar = document.querySelector('.sidebar');
    const toggleBtn = document.querySelector('[data-sidebar-toggle]');
    const overlay = document.querySelector('.sidebar-overlay');

    if (toggleBtn && sidebar) {
      toggleBtn.addEventListener('click', function () {
        if (window.innerWidth <= 992) {
          sidebar.classList.toggle('mobile-open');
          if (overlay) overlay.classList.toggle('show');
        } else {
          sidebar.classList.toggle('collapsed');
          localStorage.setItem('hms-sidebar', sidebar.classList.contains('collapsed') ? 'collapsed' : 'open');
        }
      });
    }

    // Restore sidebar state
    if (sidebar && localStorage.getItem('hms-sidebar') === 'collapsed' && window.innerWidth > 992) {
      sidebar.classList.add('collapsed');
    }

    if (overlay) {
      overlay.addEventListener('click', function () {
        sidebar?.classList.remove('mobile-open');
        overlay.classList.remove('show');
      });
    }

    // Auto-dismiss alerts
    document.querySelectorAll('.alert[data-auto-dismiss]').forEach(function (el) {
      setTimeout(function () {
        el.style.transition = 'opacity .4s, transform .4s';
        el.style.opacity = '0';
        el.style.transform = 'translateY(-12px)';
        setTimeout(function () { el.remove(); }, 400);
      }, 4500);
    });

    // Live clock
    const clockEl = document.getElementById('live-clock');
    if (clockEl) {
      function updateClock() {
        const now = new Date();
        clockEl.textContent = now.toLocaleString('en-US', {
          weekday: 'short',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit'
        });
      }
      updateClock();
      setInterval(updateClock, 1000);
    }

    // Animate stat numbers
    document.querySelectorAll('[data-count]').forEach(function (el) {
      const target = parseFloat(el.getAttribute('data-count'));
      const isCurrency = el.getAttribute('data-currency') === 'true';
      const duration = 800;
      const start = performance.now();

      function animate(now) {
        const progress = Math.min((now - start) / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        const current = target * eased;
        el.textContent = isCurrency
          ? '$' + current.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })
          : Math.round(current).toLocaleString();
        if (progress < 1) requestAnimationFrame(animate);
      }
      requestAnimationFrame(animate);
    });

    // Confirm dialogs
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
      el.addEventListener('click', function (e) {
        if (!confirm(el.getAttribute('data-confirm'))) {
          e.preventDefault();
        }
      });
    });

    // Form validation visual feedback
    document.querySelectorAll('form').forEach(function (form) {
      form.addEventListener('submit', function () {
        form.querySelectorAll('.form-control, .form-select').forEach(function (input) {
          if (!input.checkValidity()) {
            input.style.borderColor = 'var(--brand-danger)';
            input.style.animation = 'shake .4s ease';
          }
        });
      });
    });

    // Intersection Observer for scroll animations
    if ('IntersectionObserver' in window) {
      const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            observer.unobserve(entry.target);
          }
        });
      }, { threshold: 0.1 });

      document.querySelectorAll('.animate-on-scroll').forEach(function (el) {
        observer.observe(el);
      });
    }
  });

  // Shake animation keyframes injected
  const style = document.createElement('style');
  style.textContent = `
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      20% { transform: translateX(-6px); }
      40% { transform: translateX(6px); }
      60% { transform: translateX(-4px); }
      80% { transform: translateX(4px); }
    }
    .animate-on-scroll {
      opacity: 0;
      transform: translateY(20px);
      transition: opacity .5s ease, transform .5s ease;
    }
    .animate-on-scroll.visible {
      opacity: 1;
      transform: translateY(0);
    }
  `;
  document.head.appendChild(style);

})();
