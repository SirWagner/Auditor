// Audit execution screen: answers, reasons, comments, photos, draft saving, submission.
(function () {
    const form = document.getElementById('auditForm');
    if (!form) return;

    const executionId = form.dataset.executionId;
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const questions = Array.from(form.querySelectorAll('.exec-question'));
    const progressBar = document.getElementById('progressBar');
    const progressText = document.getElementById('progressText');
    const saveState = document.getElementById('saveState');
    const saveBtn = document.getElementById('saveDraftBtn');
    const submitBtn = document.getElementById('submitBtn');

    // Files picked per question, uploaded with the next save.
    const pendingFiles = new Map();
    let dirty = new Set();

    // ---------------------------------------------------------------- reading answers

    function selectedOption(q) {
        return q.querySelector('input[type=radio][name^="q_"]:checked');
    }

    function answerOf(q) {
        const type = q.dataset.type;
        const result = { templateItemId: q.dataset.itemId, optionId: null, optionIds: [], value: null };

        if (type === 'CHECKLIST' || type === 'MULTIPLE_CHOICE') {
            result.optionIds = Array.from(q.querySelectorAll('input[name^="m_"]:checked')).map(i => i.value);
        } else {
            const opt = selectedOption(q);
            if (opt) result.optionId = opt.value;
            const valueInput = q.querySelector('.answer-value:not([type=radio]), .answer-value[type=radio]:checked');
            if (valueInput && valueInput.value.trim() !== '') result.value = valueInput.value.trim();
        }

        if (type === 'PHOTO' && (pendingFiles.get(q.dataset.itemId) || []).length > 0) result.value = 'attached';
        return result;
    }

    function isAnswered(q) {
        const a = answerOf(q);
        return a.optionId !== null || a.optionIds.length > 0 || (a.value !== null && a.value !== '');
    }

    function needsReason(q) {
        const opt = selectedOption(q);
        return !!opt && opt.dataset.requiresReason === 'true';
    }

    function hasReason(q) {
        return q.querySelectorAll('input[name^="r_"]:checked').length > 0
            || (q.querySelector('.custom-reason')?.value.trim() || '') !== '';
    }

    // ---------------------------------------------------------------- UI state

    function refresh(q) {
        const reasons = q.querySelector('.exec-reasons');
        if (reasons) reasons.hidden = !needsReason(q);

        q.classList.toggle('answered', isAnswered(q) && !needsReason(q));
        q.classList.toggle('non-compliant', needsReason(q));
        if (q.classList.contains('has-error') && isAnswered(q) && (!needsReason(q) || hasReason(q)))
            q.classList.remove('has-error');
    }

    function refreshProgress() {
        const answered = questions.filter(isAnswered).length;
        const pct = questions.length ? Math.round(answered * 100 / questions.length) : 0;
        progressBar.style.width = pct + '%';
        progressText.textContent = `${answered} of ${questions.length} answered`;
    }

    function markDirty(q) {
        dirty.add(q.dataset.itemId);
        saveState.textContent = 'Unsaved changes';
    }

    questions.forEach(q => {
        q.addEventListener('change', () => { refresh(q); refreshProgress(); markDirty(q); });
        q.addEventListener('input', () => { markDirty(q); });

        q.querySelector('[data-action="comment"]')?.addEventListener('click', () => {
            const box = q.querySelector('.exec-comment');
            box.hidden = !box.hidden;
            if (!box.hidden) box.querySelector('textarea').focus();
        });
        q.querySelector('[data-action="photo"]')?.addEventListener('click', () => q.querySelector('.file-camera').click());
        q.querySelector('[data-action="file"]')?.addEventListener('click', () => q.querySelector('.file-any').click());

        q.querySelectorAll('input[type=file]').forEach(input => input.addEventListener('change', () => {
            const id = q.dataset.itemId;
            const list = pendingFiles.get(id) || [];
            list.push(...input.files);
            pendingFiles.set(id, list);
            input.value = '';
            q.querySelector('.new-files').textContent = list.map(f => f.name).join(', ') + ' (not uploaded yet)';
            refresh(q); refreshProgress(); markDirty(q);
        }));

        refresh(q);
    });
    refreshProgress();

    // ---------------------------------------------------------------- saving

    function buildPayload(onlyDirty) {
        const data = new FormData();
        data.append('ExecutionId', executionId);
        let i = 0;
        questions.forEach(q => {
            const id = q.dataset.itemId;
            if (onlyDirty && !dirty.has(id)) return;
            const a = answerOf(q);
            const p = `Responses[${i}]`;
            data.append(`${p}.TemplateItemId`, id);
            if (a.optionId) data.append(`${p}.SelectedOptionId`, a.optionId);
            a.optionIds.forEach((v, j) => data.append(`${p}.SelectedOptionIds[${j}]`, v));
            if (a.value !== null && q.dataset.type !== 'PHOTO') data.append(`${p}.AnswerValue`, a.value);
            if (q.dataset.type === 'PHOTO' && q.querySelector('.answer-value')?.value) data.append(`${p}.AnswerValue`, 'attached');

            const comment = q.querySelector('textarea.comment')?.value.trim();
            if (comment) data.append(`${p}.Comment`, comment);

            Array.from(q.querySelectorAll('input[name^="r_"]:checked')).forEach((r, j) => data.append(`${p}.SelectedReasonIds[${j}]`, r.value));
            const custom = q.querySelector('.custom-reason')?.value.trim();
            if (custom) data.append(`${p}.CustomReason`, custom);

            (pendingFiles.get(id) || []).forEach(f => data.append(`${p}.Attachments`, f));
            i++;
        });
        return data;
    }

    async function post(url, data) {
        const response = await fetch(url, {
            method: 'POST',
            body: data,
            headers: { 'RequestVerificationToken': token },
            credentials: 'same-origin',
        });
        if (response.status === 401 || response.redirected) throw new Error('Your session has ended. Sign in again in another tab, then retry.');
        if (!response.ok) throw new Error(`The server responded with ${response.status}.`);
        return response.json();
    }

    function setBusy(busy) {
        saveBtn.disabled = busy;
        submitBtn.disabled = busy;
    }

    async function saveDraft(silent) {
        if (dirty.size === 0) {
            if (!silent) saveState.textContent = 'All changes saved';
            return true;
        }
        setBusy(true);
        saveState.textContent = 'Saving…';
        try {
            const result = await post('/AuditExecutions/SaveDraft', buildPayload(true));
            if (!result.success) throw new Error(result.message);
            if (pendingFiles.size > 0) { window.location.reload(); return true; }
            dirty = new Set();
            saveState.textContent = `Saved at ${result.savedAt}`;
            return true;
        } catch (err) {
            saveState.textContent = 'Not saved';
            Swal.fire({ icon: 'error', title: "Your answers weren't saved", text: err.message });
            return false;
        } finally {
            setBusy(false);
        }
    }

    function validateLocally() {
        const problems = questions.filter(q =>
            (q.dataset.mandatory === 'true' && !isAnswered(q)) || (needsReason(q) && !hasReason(q)) || q.querySelector('.time-input.is-invalid'));
        questions.forEach(q => q.classList.toggle('has-error', problems.includes(q)));
        window.auditSections?.refresh();
        return problems;
    }

    // Opens the question's section tab (when the audit has tabs) and scrolls to it.
    function showQuestion(q) {
        if (window.auditSections) window.auditSections.show(q);
        else q.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    // ' Sections: Arrival, Seating.' for the tabs holding the given questions (empty when there are no tabs).
    function sectionsOf(list) {
        const names = [...new Set(list.map(q => q.closest('.sec-panel')).filter(Boolean)
            .map(p => document.querySelector('#tab-' + p.dataset.panel + ' .sec-tab-title')?.textContent.trim()).filter(Boolean))];
        return names.length ? ' Sections: ' + names.join(', ') + '.' : '';
    }

    async function submit() {
        const problems = validateLocally();
        if (problems.length > 0) {
            showQuestion(problems[0]);
            Swal.fire({
                icon: 'warning',
                title: `${problems.length} question(s) need attention`,
                text: 'Answer every required question, give a reason for each "Not compliant" answer, and check any time shown in red.' + sectionsOf(problems),
            });
            return;
        }

        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Submit this audit?',
            text: "After submitting you can't change your answers unless the reviewer returns the audit.",
            showCancelButton: true,
            confirmButtonText: 'Submit',
        });
        if (!confirm.isConfirmed) return;

        setBusy(true);
        try {
            const result = await post('/AuditExecutions/Submit', buildPayload(false));
            if (!result.success) {
                const missing = (result.missing || []).map(id => document.getElementById('q-' + id)).filter(Boolean);
                missing.forEach(q => q.classList.add('has-error'));
                window.auditSections?.refresh();
                if (missing.length) showQuestion(missing[0]);
                throw new Error(result.message);
            }
            window.onbeforeunload = null;
            await Swal.fire({ icon: 'success', title: 'Audit submitted', text: 'Your manager will review it.' });
            window.location.href = result.redirect;
        } catch (err) {
            Swal.fire({ icon: 'error', title: "The audit wasn't submitted", text: err.message });
        } finally {
            setBusy(false);
        }
    }

    saveBtn.addEventListener('click', () => saveDraft(false));
    submitBtn.addEventListener('click', submit);

    // Autosave every 2 minutes, and warn before leaving with unsaved answers.
    setInterval(() => { if (dirty.size > 0 && pendingFiles.size === 0) saveDraft(true); }, 120000);
    window.onbeforeunload = e => {
        if (dirty.size > 0) { e.preventDefault(); return ''; }
    };
})();
