// Section tabs for audit questions (execution and results screens):
//  - one tab per main section, remembered in the URL (#s2) so a reload returns to the same place
//  - inside a tab, a navigation tree (desktop) or "Jump to" list (phone) of sections and sub-locations,
//    highlighting the one being read
//  - live counts: answered/total while auditing, non-compliant answers on the results page
// Exposes window.auditSections = { show(questionEl), refresh(), filter(fn|null) }.
(function () {
    const root = document.querySelector('[data-sections]');
    if (!root) return;

    const editing = !!document.getElementById('auditForm');
    const tabs = Array.from(root.querySelectorAll('.sec-tab'));
    const panels = Array.from(root.querySelectorAll('.sec-panel'));
    const sticky = root.querySelector('.sec-sticky');
    const storeKey = 'auditSections:' + root.dataset.key;
    let current = panels[0]?.dataset.panel;

    // ---------------------------------------------------------------- sticky offsets

    function setOffsets() {
        const h = sticky ? sticky.offsetHeight : 0;
        document.documentElement.style.setProperty('--sec-sticky-h', h + 'px');
    }
    setOffsets();
    window.addEventListener('resize', setOffsets);

    // Where the pinned bars end once stuck (nav bar, progress + tabs, and on phones the "Jump to" bar),
    // measured from their heights so it's right even before the page has scrolled them into place.
    function stickyBottom() {
        let bottom = (document.querySelector('.auditor-header-wrapper')?.offsetHeight || 0) + (sticky ? sticky.offsetHeight : 0);
        const nav = panel(current)?.querySelector('.sec-nav');
        if (nav && getComputedStyle(nav).position === 'sticky' && window.matchMedia('(max-width: 991.98px)').matches)
            bottom += nav.offsetHeight;
        return bottom;
    }

    // ---------------------------------------------------------------- tabs

    function panel(id) { return panels.find(p => p.dataset.panel === id); }

    function activate(id, options = {}) {
        if (!panel(id)) return;
        current = id;
        panels.forEach(p => { p.hidden = p.dataset.panel !== id; });
        tabs.forEach(t => {
            const on = t.dataset.tab === id;
            t.setAttribute('aria-selected', on ? 'true' : 'false');
            t.tabIndex = on ? 0 : -1;
            if (on) t.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        });
        if (options.focus) tabs.find(t => t.dataset.tab === id)?.focus();
        if (options.toTop) {
            const top = root.getBoundingClientRect().top + window.scrollY - (document.querySelector('.auditor-header-wrapper')?.offsetHeight || 0) - 8;
            if (window.scrollY > top) window.scrollTo({ top, behavior: 'smooth' });
        }
        if (tabs.length) {
            history.replaceState(null, '', '#' + id);
            try { sessionStorage.setItem(storeKey, id); } catch { /* storage unavailable: the URL still remembers */ }
        }
        setOffsets();
        spy();
    }

    tabs.forEach((t, i) => {
        t.addEventListener('click', () => activate(t.dataset.tab));
        t.addEventListener('keydown', e => {
            const visible = tabs.filter(x => !x.hidden);
            const at = visible.indexOf(t);
            let next = null;
            if (e.key === 'ArrowRight') next = visible[(at + 1) % visible.length];
            else if (e.key === 'ArrowLeft') next = visible[(at - 1 + visible.length) % visible.length];
            else if (e.key === 'Home') next = visible[0];
            else if (e.key === 'End') next = visible[visible.length - 1];
            if (next) { e.preventDefault(); activate(next.dataset.tab, { focus: true }); }
        });
    });

    root.querySelectorAll('[data-go]').forEach(b => b.addEventListener('click', () => activate(b.dataset.go, { toTop: true })));

    // ---------------------------------------------------------------- in-tab navigation

    function scrollToGroup(groupId) {
        const g = document.getElementById('grp-' + groupId);
        if (!g) return;
        const top = g.getBoundingClientRect().top + window.scrollY - stickyBottom() - 12;
        window.scrollTo({ top, behavior: 'smooth' });
    }

    root.querySelectorAll('.sec-tree a[data-group]').forEach(a => a.addEventListener('click', e => {
        e.preventDefault();
        scrollToGroup(a.dataset.group);
    }));
    root.querySelectorAll('.sec-jump').forEach(s => s.addEventListener('change', () => { if (s.value) scrollToGroup(s.value); }));

    // Highlights the group being read in the tree and the "Jump to" list.
    let spyQueued = false;
    function spy() {
        const p = panel(current);
        if (!p) return;
        const groups = Array.from(p.querySelectorAll('.sec-group')).filter(g => !g.hidden);
        const line = stickyBottom() + 60;
        let active = groups[0];
        for (const g of groups) { if (g.getBoundingClientRect().top <= line) active = g; else break; }
        const id = active?.dataset.group;
        p.querySelectorAll('.sec-tree a[data-group]').forEach(a => a.classList.toggle('is-current', a.dataset.group === id));
        const jump = p.querySelector('.sec-jump');
        if (jump && id && jump.value !== id) jump.value = id;
    }
    window.addEventListener('scroll', () => {
        if (spyQueued) return;
        spyQueued = true;
        requestAnimationFrame(() => { spyQueued = false; spy(); });
    }, { passive: true });

    // ---------------------------------------------------------------- counts

    const isAnswered = q => q.classList.contains('answered') || q.classList.contains('non-compliant');
    const isIssue = q => q.classList.contains('non-compliant');

    function state(questions) {
        const shown = questions.filter(q => !q.hidden);
        return {
            total: shown.length,
            answered: shown.filter(isAnswered).length,
            issues: shown.filter(isIssue).length,
            errors: shown.filter(q => q.classList.contains('has-error')).length,
        };
    }

    function paint(el, countEl, s) {
        if (!el) return;
        el.classList.toggle('has-error', s.errors > 0);
        if (editing) {
            el.classList.toggle('is-done', s.total > 0 && s.answered === s.total && s.errors === 0);
            el.classList.toggle('is-partial', s.answered > 0 && s.answered < s.total);
            if (countEl) countEl.textContent = `${s.answered}/${s.total}`;
        } else {
            el.classList.toggle('has-issues', s.issues > 0);
            if (countEl) countEl.textContent = s.issues > 0 ? `${s.issues} ✗` : String(s.total);
            if (countEl) countEl.title = s.issues > 0 ? `${s.issues} non-compliant answer(s)` : `${s.total} question(s)`;
        }
    }

    function refresh() {
        panels.forEach(p => {
            const tab = tabs.find(t => t.dataset.tab === p.dataset.panel);
            paint(tab, tab?.querySelector('[data-count]'), state(Array.from(p.querySelectorAll('.exec-question'))));
        });
        root.querySelectorAll('.sec-group').forEach(g => {
            const s = state(Array.from(g.querySelectorAll('.exec-question')));
            paint(g.querySelector('.exec-section'), g.querySelector('.sec-group-count'), s);
            const link = root.querySelector(`.sec-tree a[data-group="${g.dataset.group}"]`);
            paint(link, link?.querySelector('[data-count]'), s);
        });
    }

    root.addEventListener('change', () => queueMicrotask(refresh));
    root.addEventListener('input', () => queueMicrotask(refresh));

    // ---------------------------------------------------------------- public API

    function show(question) {
        const p = question?.closest('.sec-panel');
        if (!p) return;
        if (p.dataset.panel !== current) activate(p.dataset.panel);
        const top = question.getBoundingClientRect().top + window.scrollY - stickyBottom() - 16;
        window.scrollTo({ top, behavior: 'smooth' });
    }

    // Shows only questions matching fn (null shows all); groups, tree entries and tabs left empty are hidden.
    function filter(fn) {
        root.querySelectorAll('.exec-question').forEach(q => { q.hidden = !!fn && !fn(q); });
        root.querySelectorAll('.sec-group').forEach(g => {
            g.hidden = !g.querySelector('.exec-question:not([hidden])');
            const link = root.querySelector(`.sec-tree a[data-group="${g.dataset.group}"]`);
            if (link) link.parentElement.hidden = g.hidden;
        });
        tabs.forEach(t => { t.hidden = !panel(t.dataset.tab).querySelector('.exec-question:not([hidden])'); });
        if (tabs.length && tabs.find(t => t.dataset.tab === current)?.hidden) {
            const first = tabs.find(t => !t.hidden);
            if (first) activate(first.dataset.tab);
        }
        refresh();
    }

    window.auditSections = { show, refresh, filter };

    // ---------------------------------------------------------------- start

    let start = location.hash.slice(1);
    if (!panel(start)) {
        try { start = sessionStorage.getItem(storeKey) || ''; } catch { start = ''; }
    }
    if (panel(start) && start !== current) activate(start);
    else if (tabs.length && location.hash) activate(current);
    refresh();
    spy();
})();
