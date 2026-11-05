// Answer controls on the audit screen. Each one keeps its value in an .answer-value input and fires "change" on it,
// which is all audit-execute.js needs to know about.
//  - Time: "Now" stamps the current time to the second; typing fills in the colons; the clock opens hour/minute/second wheels.
//  - List: a dropdown with "Other" for anything not on the list yet. Picking an aircraft fills in its body type.
(function () {
    const pad = n => String(n).padStart(2, '0');
    const nowText = () => { const d = new Date(); return `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`; };

    // ---------------------------------------------------------------- time

    // "1430" -> "14:30:00", "14305" -> "14:30:05", "9:5" -> "09:05:00". Returns null when it isn't a time.
    function parseTime(text) {
        const t = text.trim();
        if (!t) return '';
        let parts;
        if (t.includes(':')) parts = t.split(':');
        else if (/^\d+$/.test(t) && t.length <= 6) {
            const digits = t.length % 2 ? '0' + t : t;
            parts = digits.match(/\d{2}/g);
        } else return null;
        if (parts.length > 3 || parts.some(p => !/^\d{1,2}$/.test(p))) return null;
        const [h, m = 0, s = 0] = parts.map(Number);
        if (h > 23 || m > 59 || s > 59) return null;
        return `${pad(h)}:${pad(m)}:${pad(s)}`;
    }

    function setTime(field, value) {
        const input = field.querySelector('.time-input');
        input.value = value;
        input.classList.remove('is-invalid');
        field.querySelector('.time-hint').hidden = true;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    // One set of wheels, moved to whichever field is open.
    const wheelMax = { h: 24, m: 60, s: 60 };
    let pop = null;
    let openField = null;
    let ticker = null;

    function column(unit, label) {
        const cells = Array.from({ length: wheelMax[unit] }, (_, i) =>
            `<button type="button" class="time-cell" data-unit="${unit}" data-value="${i}">${pad(i)}</button>`).join('');
        return `<div class="time-col"><div class="time-col-label">${label}</div><div class="time-wheel" data-wheel="${unit}">${cells}</div></div>`;
    }

    function buildPop() {
        pop = document.createElement('div');
        pop.className = 'time-pop';
        pop.setAttribute('role', 'dialog');
        pop.setAttribute('aria-label', 'Pick a time');
        pop.innerHTML = `
            <button type="button" class="time-pop-now" data-now><i class="far fa-clock"></i> Now <span class="time-pop-clock"></span></button>
            <div class="time-cols">${column('h', 'Hour')}${column('m', 'Min')}${column('s', 'Sec')}</div>
            <div class="time-pop-foot">
                <button type="button" class="time-pop-clear" data-clear>Clear</button>
                <button type="button" class="time-pop-done" data-done>Done</button>
            </div>`;
        pop.addEventListener('click', e => {
            const cell = e.target.closest('.time-cell');
            if (cell) {
                const current = openField.querySelector('.time-input').value.split(':').map(Number);
                const parts = { h: current[0] || 0, m: current[1] || 0, s: current[2] || 0 };
                parts[cell.dataset.unit] = Number(cell.dataset.value);
                setTime(openField, `${pad(parts.h)}:${pad(parts.m)}:${pad(parts.s)}`);
                syncWheels(false);
            } else if (e.target.closest('[data-now]')) {
                setTime(openField, nowText());
                syncWheels(true);
            } else if (e.target.closest('[data-clear]')) {
                setTime(openField, '');
                syncWheels(false);
            } else if (e.target.closest('[data-done]')) {
                closePop(true);
            }
        });
    }

    function syncWheels(scroll) {
        const value = openField.querySelector('.time-input').value;
        const parts = value ? value.split(':').map(Number) : [];
        ['h', 'm', 's'].forEach((unit, i) => {
            const wheel = pop.querySelector(`[data-wheel="${unit}"]`);
            wheel.querySelectorAll('.time-cell').forEach(c => c.classList.toggle('is-on', Number(c.dataset.value) === parts[i]));
            if (scroll) {
                const on = wheel.querySelector('.is-on') || wheel.firstElementChild;
                wheel.scrollTop = on.offsetTop - wheel.clientHeight / 2 + on.offsetHeight / 2;
            }
        });
    }

    function openPop(field) {
        if (!pop) buildPop();
        if (openField === field) { closePop(true); return; }
        closePop(false);
        openField = field;
        field.appendChild(pop);
        field.querySelector('.time-open').setAttribute('aria-expanded', 'true');
        pop.hidden = false;
        syncWheels(true);
        const clock = pop.querySelector('.time-pop-clock');
        clock.textContent = nowText();
        ticker = setInterval(() => { clock.textContent = nowText(); }, 1000);
        // On wide screens it drops down from the field, so bring it clear of the Save / Submit bar. On phones it's a bottom sheet.
        if (getComputedStyle(pop).position !== 'fixed') {
            const bar = document.querySelector('.exec-actions')?.offsetHeight || 0;
            const hidden = pop.getBoundingClientRect().bottom - (window.innerHeight - bar - 8);
            if (hidden > 0) window.scrollBy({ top: hidden, behavior: 'instant' });
        }
    }

    function closePop(focusButton) {
        if (!openField) return;
        clearInterval(ticker);
        pop.hidden = true;
        const button = openField.querySelector('.time-open');
        button.setAttribute('aria-expanded', 'false');
        if (focusButton) button.focus();
        openField = null;
    }

    document.addEventListener('click', e => {
        if (openField && !e.target.closest('.time-field')) closePop(false);
    });
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape') closePop(true);
    });

    document.querySelectorAll('[data-time-field]').forEach(field => {
        const input = field.querySelector('.time-input');
        const hint = field.querySelector('.time-hint');

        field.querySelector('.time-now').addEventListener('click', () => setTime(field, nowText()));
        field.querySelector('.time-open').addEventListener('click', () => openPop(field));

        // Tidy what was typed before the audit screen reads it.
        input.addEventListener('change', e => {
            if (e.isTrusted === false) return;
            const parsed = parseTime(input.value);
            const ok = parsed !== null;
            input.classList.toggle('is-invalid', !ok);
            hint.hidden = ok;
            if (ok) input.value = parsed;
            if (openField === field) syncWheels(false);
        }, true);
    });

    // ---------------------------------------------------------------- list

    const listField = code => document.querySelector(`[data-list-field][data-list-code="${code}"]`);

    function setList(field, value) {
        const select = field.querySelector('.list-select');
        const other = field.querySelector('.list-other');
        const known = Array.from(select.options).some(o => o.value === value && o.value !== '__other');
        select.value = value === '' ? '' : known ? value : '__other';
        other.hidden = select.value !== '__other';
        other.value = known ? '' : value;
        const answer = field.querySelector('.answer-value');
        answer.value = value;
        answer.dispatchEvent(new Event('change', { bubbles: true }));
    }

    document.querySelectorAll('[data-list-field]').forEach(field => {
        const select = field.querySelector('.list-select');
        const other = field.querySelector('.list-other');
        const answer = field.querySelector('.answer-value');

        select.addEventListener('change', () => {
            other.hidden = select.value !== '__other';
            if (select.value === '__other') {
                answer.value = other.value.trim();
                other.focus();
            } else {
                answer.value = select.value;
                other.value = '';
            }
            answer.dispatchEvent(new Event('change', { bubbles: true }));

            // An aircraft type knows its body type; don't overwrite one the auditor already chose.
            const body = select.selectedOptions[0]?.dataset.group;
            const bodyField = field.dataset.listCode === 'AIRCRAFT_TYPES' ? listField('BODY_TYPES') : null;
            if (body && bodyField && !bodyField.querySelector('.answer-value').value) setList(bodyField, body);
        });

        other.addEventListener('input', () => {
            answer.value = other.value.trim();
            answer.dispatchEvent(new Event('change', { bubbles: true }));
        });
    });
})();
