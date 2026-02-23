/* ═══════════════════════════════════════════════════════════════════════════
   admin.js — Cime-X Admin Dashboard
   Vanilla JS IIFE — no framework dependencies.
   ═══════════════════════════════════════════════════════════════════════════ */

var AdminApp = (function () {
  'use strict';

  // ── State ────────────────────────────────────────────────────────────────

  var currentWeekStart = null;   // ISO date string 'YYYY-MM-DD'
  var panelOpenId = null;        // id of the appointment currently in panel
  var lastFocusBeforePanel = null;

  // ── Helpers ──────────────────────────────────────────────────────────────

  function csrfToken() {
    var meta = document.querySelector('meta[name="csrf-token"]');
    return meta ? meta.getAttribute('content') : '';
  }

  function apiGet(url) {
    return fetch(url, {
      headers: { 'Accept': 'application/json' }
    }).then(function (r) {
      if (!r.ok) throw new Error('HTTP ' + r.status);
      return r.json();
    });
  }

  function apiPost(url, body) {
    return fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'RequestVerificationToken': csrfToken()
      },
      body: JSON.stringify(body)
    }).then(function (r) {
      if (!r.ok) throw new Error('HTTP ' + r.status);
      return r.json();
    });
  }

  function showToast(msg, type) {
    var container = document.getElementById('toast-container');
    if (!container) return;
    var toast = document.createElement('div');
    toast.className = 'admin-toast' + (type ? ' admin-toast--' + type : '');
    toast.textContent = msg;
    container.appendChild(toast);
    // Trigger transition
    requestAnimationFrame(function () {
      requestAnimationFrame(function () {
        toast.classList.add('is-visible');
      });
    });
    setTimeout(function () {
      toast.classList.remove('is-visible');
      setTimeout(function () { toast.remove(); }, 300);
    }, 3200);
  }

  function formatDate(iso) {
    if (!iso) return '—';
    var d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  function formatDateTime(iso) {
    if (!iso) return '—';
    var d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
      + ' ' + d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
  }

  function toDatetimeLocal(iso) {
    // Convert ISO to value for datetime-local input (YYYY-MM-DDTHH:MM)
    if (!iso) return '';
    var d = new Date(iso);
    var pad = function (n) { return String(n).padStart(2, '0'); };
    return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate())
      + 'T' + pad(d.getHours()) + ':' + pad(d.getMinutes());
  }

  function statusLabel(statusInt) {
    return ['New', 'Scheduled', 'Completed', 'Cancelled'][statusInt] || 'Unknown';
  }

  function statusClass(label) {
    var map = { New: 'new', Scheduled: 'scheduled', Completed: 'completed', Cancelled: 'cancelled' };
    return map[label] || 'new';
  }

  function escHtml(str) {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }

  // ── 1. Theme ─────────────────────────────────────────────────────────────

  function initTheme() {
    var saved = localStorage.getItem('adminTheme') || 'light';
    document.documentElement.setAttribute('data-theme', saved);

    var btn = document.getElementById('theme-toggle');
    if (!btn) return;
    btn.addEventListener('click', function () {
      var current = document.documentElement.getAttribute('data-theme');
      var next = current === 'dark' ? 'light' : 'dark';
      document.documentElement.setAttribute('data-theme', next);
      localStorage.setItem('adminTheme', next);
    });
  }

  // ── 2. Sidebar ────────────────────────────────────────────────────────────

  function initSidebar() {
    var toggle = document.getElementById('sidebar-toggle');
    var sidebar = document.getElementById('admin-sidebar');
    if (!toggle || !sidebar) return;
    toggle.addEventListener('click', function () {
      sidebar.classList.toggle('is-open');
    });
    // Close sidebar when clicking main on mobile
    var main = document.getElementById('admin-main');
    if (main) {
      main.addEventListener('click', function () {
        if (window.innerWidth <= 900) {
          sidebar.classList.remove('is-open');
        }
      });
    }
  }

  // ── 3. Calendar ───────────────────────────────────────────────────────────

  var DAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  function parseDate(str) {
    // str is YYYY-MM-DD or ISO 8601
    var parts = str.substring(0, 10).split('-');
    return new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
  }

  function renderCalendar(appointments, weekStart) {
    var grid = document.getElementById('cal-grid');
    if (!grid) return;
    grid.innerHTML = '';

    var start = parseDate(weekStart);
    var todayStr = new Date().toDateString();

    // Header row
    DAYS.forEach(function (day) {
      var h = document.createElement('div');
      h.className = 'admin-cal-header';
      h.textContent = day;
      grid.appendChild(h);
    });

    // 7 day cells
    for (var i = 0; i < 7; i++) {
      var date = new Date(start.getTime());
      date.setDate(start.getDate() + i);

      var cell = document.createElement('div');
      cell.className = 'admin-cal-cell';
      if (date.toDateString() === todayStr) cell.classList.add('admin-cal-today');

      var dateSpan = document.createElement('span');
      dateSpan.className = 'admin-cal-date';
      dateSpan.textContent = date.getDate();
      cell.appendChild(dateSpan);

      // Find appointments for this day
      var dayAppts = (appointments || []).filter(function (a) {
        var d = new Date(a.scheduledFor);
        return d.toDateString() === date.toDateString();
      });

      dayAppts.forEach(function (appt) {
        var chip = document.createElement('div');
        chip.className = 'admin-cal-chip admin-cal-chip--status-' + appt.statusInt;
        chip.textContent = appt.customerName.split(' ')[0];
        chip.title = appt.customerName + ' — ' + appt.pestType;
        chip.dataset.id = appt.id;
        chip.addEventListener('click', function (e) {
          e.stopPropagation();
          openPanel(appt.id);
        });
        cell.appendChild(chip);
      });

      grid.appendChild(cell);
    }
  }

  function initCalendar() {
    var dataEl = document.getElementById('calendarData');
    var metaEl = document.getElementById('calendarMeta');
    if (!dataEl || !metaEl) return;

    var appointments = [];
    var meta = {};
    try { appointments = JSON.parse(dataEl.textContent); } catch (e) { /* ignore */ }
    try { meta = JSON.parse(metaEl.textContent); } catch (e) { /* ignore */ }

    currentWeekStart = meta.weekStart || null;
    renderCalendar(appointments, currentWeekStart);

    // Navigation
    var prevBtn = document.getElementById('cal-prev');
    var nextBtn = document.getElementById('cal-next');
    if (prevBtn) prevBtn.addEventListener('click', function () { navigateCalendar(-7); });
    if (nextBtn) nextBtn.addEventListener('click', function () { navigateCalendar(7); });
  }

  function navigateCalendar(dayDelta) {
    if (!currentWeekStart) return;
    var d = parseDate(currentWeekStart);
    d.setDate(d.getDate() + dayDelta);
    var pad = function (n) { return String(n).padStart(2, '0'); };
    var newWeekStart = d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());

    apiGet('/Admin/api/calendar?weekStart=' + newWeekStart).then(function (data) {
      currentWeekStart = newWeekStart;
      renderCalendar(data.appointments, data.weekStart);

      // Update heading
      var heading = document.querySelector('.admin-cal-section .admin-section-title');
      if (heading && data.weekStart && data.weekEnd) {
        var s = parseDate(data.weekStart);
        var e = parseDate(data.weekEnd);
        heading.textContent =
          s.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) +
          ' – ' +
          e.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
      }
    }).catch(function () {
      showToast('Failed to load calendar', 'error');
    });
  }

  // ── 4. Table filter ───────────────────────────────────────────────────────

  function initTableFilter() {
    var pills = document.querySelectorAll('.admin-filter-pill');
    pills.forEach(function (pill) {
      pill.addEventListener('click', function () {
        pills.forEach(function (p) { p.classList.remove('is-active'); });
        pill.classList.add('is-active');
        var filter = pill.dataset.filter;
        var rows = document.querySelectorAll('#requests-tbody .admin-table-row');
        rows.forEach(function (row) {
          if (filter === 'all' || row.dataset.status === filter) {
            row.style.display = '';
          } else {
            row.style.display = 'none';
          }
        });
      });
    });
  }

  // ── 5. Table row + timeline clicks → open panel ───────────────────────────

  function initTableRowClicks() {
    var tbody = document.getElementById('requests-tbody');
    if (!tbody) return;
    tbody.addEventListener('click', function (e) {
      var row = e.target.closest('.admin-table-row');
      if (row && row.dataset.id) openPanel(parseInt(row.dataset.id));
    });
    tbody.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') {
        var row = e.target.closest('.admin-table-row');
        if (row && row.dataset.id) { e.preventDefault(); openPanel(parseInt(row.dataset.id)); }
      }
    });
  }

  function initTimelineClicks() {
    var timeline = document.querySelector('.admin-timeline');
    if (!timeline) return;
    timeline.addEventListener('click', function (e) {
      var item = e.target.closest('.admin-timeline-item');
      if (item && item.dataset.id) openPanel(parseInt(item.dataset.id));
    });
    timeline.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') {
        var item = e.target.closest('.admin-timeline-item');
        if (item && item.dataset.id) { e.preventDefault(); openPanel(parseInt(item.dataset.id)); }
      }
    });
  }

  // ── 6. Appointment panel ──────────────────────────────────────────────────

  function openPanel(id) {
    lastFocusBeforePanel = document.activeElement;
    panelOpenId = id;

    var panel = document.getElementById('appt-panel');
    var overlay = document.getElementById('appt-panel-overlay');
    var body = document.getElementById('appt-panel-body');

    if (!panel) return;

    // Show skeleton while loading
    body.innerHTML = '<div class="admin-skeleton"></div>';
    panel.setAttribute('aria-hidden', 'false');
    panel.classList.add('is-open');
    overlay.classList.add('is-open');
    document.body.style.overflow = 'hidden';

    apiGet('/Admin/api/appointment/' + id).then(function (data) {
      renderPanel(data);
    }).catch(function () {
      body.innerHTML = '<p style="color:var(--a-text-2)">Failed to load appointment.</p>';
    });
  }

  function closePanel() {
    var panel = document.getElementById('appt-panel');
    var overlay = document.getElementById('appt-panel-overlay');
    panel.classList.remove('is-open');
    overlay.classList.remove('is-open');
    panel.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    panelOpenId = null;
    if (lastFocusBeforePanel) lastFocusBeforePanel.focus();
  }

  function renderPanel(d) {
    var title = document.getElementById('appt-panel-title');
    var body = document.getElementById('appt-panel-body');
    if (title) title.textContent = d.customerName;

    var mapsUrl = 'https://maps.google.com/?q=' + encodeURIComponent(d.address);
    var schedVal = toDatetimeLocal(d.scheduledFor);
    var priceVal = d.price != null ? d.price : '';

    var statusBtns = [
      { label: 'Confirm',  statusInt: 1 },
      { label: 'Complete', statusInt: 2 },
      { label: 'Cancel',   statusInt: 3 }
    ].map(function (btn) {
      var isCurrent = d.statusInt === btn.statusInt;
      return '<button class="admin-status-btn' + (isCurrent ? ' is-current' : '') + '"'
        + (isCurrent ? ' disabled' : '')
        + ' data-status-int="' + btn.statusInt + '">'
        + btn.label + '</button>';
    }).join('');

    body.innerHTML =
      '<div class="admin-panel-section">'
        + '<p class="admin-panel-section-title">Contact</p>'
        + '<dl class="admin-panel-contact">'
          + '<dt>Phone</dt>'
          + '<dd>' + escHtml(d.phone) + '</dd>'
        + '</dl>'
        + '<div class="admin-contact-actions" style="margin-top:.35rem">'
          + '<a href="tel:' + escHtml(d.phone) + '" class="admin-contact-btn admin-contact-btn--call">'
            + '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 13a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.6 2.18h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.91 9.91a16 16 0 0 0 6.18 6.18l.95-.95a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>'
            + ' Call</a>'
          + '<a href="sms:' + escHtml(d.phone) + '" class="admin-contact-btn admin-contact-btn--text">'
            + '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>'
            + ' Text</a>'
          + '<a href="mailto:' + escHtml(d.email) + '" class="admin-contact-btn admin-contact-btn--email">'
            + '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>'
            + ' Email</a>'
          + '<a href="' + mapsUrl + '" target="_blank" rel="noopener" class="admin-contact-btn admin-contact-btn--map">'
            + '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>'
            + ' Map</a>'
        + '</div>'
        + '<dl class="admin-panel-contact" style="margin-top:.75rem">'
          + '<dt>Email</dt><dd>' + escHtml(d.email) + '</dd>'
          + '<dt>Address</dt><dd>' + escHtml(d.address) + '</dd>'
          + '<dt>Pest</dt><dd>' + escHtml(d.pestType) + '</dd>'
          + (d.description ? '<dt>Notes</dt><dd>' + escHtml(d.description) + '</dd>' : '')
        + '</dl>'
      + '</div>'

      + '<div class="admin-panel-section">'
        + '<p class="admin-panel-section-title">Scheduled For</p>'
        + '<div class="admin-inline-row">'
          + '<input type="datetime-local" id="panel-sched-input" class="admin-input" value="' + escHtml(schedVal) + '" />'
          + '<button id="panel-sched-btn" class="admin-btn admin-btn-sm admin-btn-secondary">Set</button>'
        + '</div>'
      + '</div>'

      + '<div class="admin-panel-section">'
        + '<p class="admin-panel-section-title">Price</p>'
        + '<div class="admin-inline-row">'
          + '<input type="number" id="panel-price-input" class="admin-input" value="' + escHtml(priceVal) + '" step="0.01" min="0" placeholder="0.00" />'
          + '<button id="panel-price-btn" class="admin-btn admin-btn-sm admin-btn-secondary">Save</button>'
        + '</div>'
      + '</div>'

      + '<div class="admin-panel-section">'
        + '<p class="admin-panel-section-title">Status</p>'
        + '<p style="margin:0 0 .5rem;font-size:12px;color:var(--a-text-2)">Current: <span id="panel-current-status" class="admin-status-badge admin-status-' + statusClass(d.status) + '">' + escHtml(d.status) + '</span></p>'
        + '<div class="admin-status-actions">' + statusBtns + '</div>'
      + '</div>'

      + '<div class="admin-panel-section">'
        + '<p class="admin-panel-section-title">Internal Notes</p>'
        + '<textarea id="panel-notes-input" class="admin-input admin-textarea" rows="4" placeholder="Scheduling details, access instructions…">' + escHtml(d.notes || '') + '</textarea>'
        + '<div style="display:flex;justify-content:flex-end;margin-top:.5rem">'
          + '<button id="panel-notes-btn" class="admin-btn admin-btn-sm admin-btn-secondary">Save Notes</button>'
        + '</div>'
      + '</div>'

      + '<div class="admin-panel-footer" style="padding:0;border-top:none">'
        + '<a href="/Admin/Details/' + d.id + '" class="admin-btn admin-btn-ghost" style="width:100%;justify-content:center">Open Full Page →</a>'
      + '</div>';

    // Wire up buttons
    wirePanelButtons(d.id, d.statusInt);
  }

  function wirePanelButtons(id, currentStatusInt) {
    // Schedule set
    var schedBtn = document.getElementById('panel-sched-btn');
    if (schedBtn) {
      schedBtn.addEventListener('click', function () {
        var val = document.getElementById('panel-sched-input').value;
        var priceVal = document.getElementById('panel-price-input').value;
        apiPost('/Admin/api/schedule', {
          id: id,
          scheduledFor: val ? new Date(val).toISOString() : null,
          price: priceVal !== '' ? parseFloat(priceVal) : null
        }).then(function () {
          showToast('Schedule updated', 'success');
        }).catch(function () { showToast('Failed to update schedule', 'error'); });
      });
    }

    // Price save (separate button)
    var priceBtn = document.getElementById('panel-price-btn');
    if (priceBtn) {
      priceBtn.addEventListener('click', function () {
        var schedVal = document.getElementById('panel-sched-input').value;
        var priceVal = document.getElementById('panel-price-input').value;
        apiPost('/Admin/api/schedule', {
          id: id,
          scheduledFor: schedVal ? new Date(schedVal).toISOString() : null,
          price: priceVal !== '' ? parseFloat(priceVal) : null
        }).then(function () {
          showToast('Price saved', 'success');
        }).catch(function () { showToast('Failed to save price', 'error'); });
      });
    }

    // Status quick-action buttons
    var statusBtns = document.querySelectorAll('#appt-panel-body .admin-status-btn');
    statusBtns.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var newStatusInt = parseInt(btn.dataset.statusInt);
        apiPost('/Admin/api/status', { id: id, status: newStatusInt })
          .then(function (data) {
            showToast('Status updated to ' + data.newStatus, 'success');
            // Update panel badge
            var badge = document.getElementById('panel-current-status');
            if (badge) {
              badge.textContent = data.newStatus;
              badge.className = 'admin-status-badge admin-status-' + statusClass(data.newStatus);
            }
            // Update button states
            statusBtns.forEach(function (b) {
              var isNow = parseInt(b.dataset.statusInt) === data.newStatusInt;
              b.classList.toggle('is-current', isNow);
              b.disabled = isNow;
            });
            // Update table row badge
            updateRowBadge(id, data.newStatus, data.newStatusInt);
          }).catch(function () { showToast('Failed to update status', 'error'); });
      });
    });

    // Notes save button
    var notesBtn = document.getElementById('panel-notes-btn');
    var notesInput = document.getElementById('panel-notes-input');
    if (notesBtn && notesInput) {
      function saveNotes() {
        apiPost('/Admin/api/notes', { id: id, notes: notesInput.value })
          .then(function () { showToast('Notes saved', 'success'); })
          .catch(function () { showToast('Failed to save notes', 'error'); });
      }
      notesBtn.addEventListener('click', saveNotes);
      // Auto-save on blur
      notesInput.addEventListener('blur', function () {
        apiPost('/Admin/api/notes', { id: id, notes: notesInput.value }).catch(function () { /* silent */ });
      });
    }
  }

  function updateRowBadge(id, newStatus, newStatusInt) {
    var row = document.querySelector('#requests-tbody tr[data-id="' + id + '"]');
    if (!row) return;
    // Update data-status for filtering
    row.dataset.status = newStatus;
    // Find and update the badge span
    var badge = row.querySelector('.admin-status-badge');
    if (badge) {
      badge.textContent = newStatus;
      badge.className = 'admin-status-badge admin-status-' + statusClass(newStatus);
      badge.classList.add('admin-flash');
      badge.addEventListener('animationend', function () { badge.classList.remove('admin-flash'); }, { once: true });
    }
  }

  function initPanel() {
    var closeBtn = document.getElementById('appt-panel-close');
    var overlay = document.getElementById('appt-panel-overlay');
    if (closeBtn) closeBtn.addEventListener('click', closePanel);
    if (overlay) overlay.addEventListener('click', closePanel);
  }

  // ── 7. Quick-add modal ────────────────────────────────────────────────────

  function openModal() {
    var modal = document.getElementById('quick-add-modal');
    if (!modal) return;
    modal.classList.add('is-open');
    modal.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    // Focus first field
    var first = modal.querySelector('input, select, textarea');
    if (first) setTimeout(function () { first.focus(); }, 50);
  }

  function closeModal() {
    var modal = document.getElementById('quick-add-modal');
    if (!modal) return;
    modal.classList.remove('is-open');
    modal.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
  }

  function initModal() {
    var openBtn = document.getElementById('new-booking-btn');
    var closeBtn = document.getElementById('modal-close');
    var cancelBtn = document.getElementById('modal-cancel');
    var overlay = document.getElementById('quick-add-modal');
    var form = document.getElementById('quick-add-form');

    if (openBtn)  openBtn.addEventListener('click', openModal);
    if (closeBtn) closeBtn.addEventListener('click', closeModal);
    if (cancelBtn) cancelBtn.addEventListener('click', closeModal);
    if (overlay) {
      overlay.addEventListener('click', function (e) {
        if (e.target === overlay) closeModal();
      });
    }

    if (form) {
      form.addEventListener('submit', function (e) {
        e.preventDefault();
        var data = Object.fromEntries(new FormData(form));
        var payload = {
          customerName: data.customerName,
          phone: data.phone,
          email: data.email || null,
          address: data.address,
          pestType: parseInt(data.pestType),
          scheduledFor: data.scheduledFor ? new Date(data.scheduledFor).toISOString() : null,
          price: data.price ? parseFloat(data.price) : null,
          description: data.description || null
        };

        var submitBtn = form.querySelector('[type="submit"]');
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Creating…'; }

        apiPost('/Admin/api/quickcreate', payload).then(function (result) {
          closeModal();
          form.reset();
          showToast('Booking created for ' + result.customerName, 'success');
          prependTableRow(result.id, payload, result.customerName);
        }).catch(function () {
          showToast('Failed to create booking', 'error');
        }).finally(function () {
          if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = 'Create Booking'; }
        });
      });
    }
  }

  function prependTableRow(id, payload, customerName) {
    var tbody = document.getElementById('requests-tbody');
    if (!tbody) return;
    var pestNames = ['Ants','Bed Bugs','Cockroaches','Fleas','Mosquitoes','Rodents','Spiders','Termites','Wasps','Other'];
    var pestLabel = pestNames[payload.pestType] || 'Other';
    var schedLabel = payload.scheduledFor ? formatDateTime(payload.scheduledFor) : '—';
    var today = new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

    var tr = document.createElement('tr');
    tr.className = 'admin-table-row';
    tr.dataset.id = id;
    tr.dataset.status = 'New';
    tr.setAttribute('role', 'button');
    tr.setAttribute('tabindex', '0');
    tr.innerHTML =
      '<td class="admin-td-name">' + escHtml(customerName) + '</td>'
      + '<td class="admin-td-pest">' + escHtml(pestLabel) + '</td>'
      + '<td><span class="admin-status-badge admin-status-new">New</span></td>'
      + '<td class="admin-td-sched">' + escHtml(schedLabel) + '</td>'
      + '<td class="admin-td-date admin-th-date">' + escHtml(today) + '</td>';

    tr.addEventListener('click', function () { openPanel(id); });
    tr.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openPanel(id); }
    });

    tbody.insertBefore(tr, tbody.firstChild);
  }

  // ── 8. Keyboard nav ───────────────────────────────────────────────────────

  function initKeyboard() {
    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') {
        var panel = document.getElementById('appt-panel');
        var modal = document.getElementById('quick-add-modal');
        if (modal && modal.classList.contains('is-open')) { closeModal(); return; }
        if (panel && panel.classList.contains('is-open')) { closePanel(); }
      }
    });
  }

  // ── Boot ──────────────────────────────────────────────────────────────────

  function init() {
    initTheme();
    initSidebar();
    initCalendar();
    initTableFilter();
    initTableRowClicks();
    initTimelineClicks();
    initPanel();
    initModal();
    initKeyboard();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

  return { openPanel: openPanel, closePanel: closePanel, showToast: showToast };

})();
