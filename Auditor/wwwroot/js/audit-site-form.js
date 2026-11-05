// Audit Site Create/Edit: Category → Sub-Category → Site Type, and the airport-dependent Terminal list.
document.addEventListener('DOMContentLoaded', function () {
    const config = window.auditSiteForm || {};
    const airportSelect = document.getElementById('airportSelect');
    const categorySelect = document.getElementById('categorySelect');
    const subCategorySelect = document.getElementById('subCategorySelect');
    const siteTypeSelect = document.getElementById('siteTypeSelect');
    const siteTypeHint = document.getElementById('siteTypeHint');
    const terminalSelect = document.getElementById('terminalSelect');
    const terminalRequired = document.getElementById('terminalRequired');
    const terminalHint = document.getElementById('terminalHint');

    // Site types and terminals for the currently selected airport.
    let airportData = { terminals: [], siteTypes: {} };

    function fillSelect(select, values, placeholder, selected) {
        select.innerHTML = '';
        select.add(new Option(placeholder, ''));
        values.forEach(function (item) {
            const value = typeof item === 'object' ? String(item.id) : item;
            const text = typeof item === 'object' ? item.name : item;
            select.add(new Option(text, value, false, value === String(selected ?? '')));
        });
        select.disabled = values.length === 0;
    }

    function selectedOr(select) {
        // Keep the server-provided value until the user changes something upstream.
        const value = select.dataset.selected;
        delete select.dataset.selected;
        return value !== undefined ? value : select.value;
    }

    function refreshSubCategories() {
        const subs = config.subCategoriesByCategory[categorySelect.value] || [];
        fillSelect(subCategorySelect, subs,
            subs.length ? '-- Select Sub-Category --' : '-- Select Category first --',
            selectedOr(subCategorySelect));
        refreshSiteTypes();
    }

    function refreshSiteTypes() {
        const sub = subCategorySelect.value;
        const types = (airportData.siteTypes && airportData.siteTypes[sub]) || [];
        let placeholder = '-- Select Site Type --';
        let hint = '';

        if (!sub) {
            placeholder = '-- Select Sub-Category first --';
        } else if (!airportSelect.value && !(sub in (airportData.siteTypes || {}))) {
            placeholder = '-- Select an Airport first --';
        } else if (types.length === 0) {
            placeholder = '-- No ' + sub + ' locations --';
            hint = 'This airport has no ' + sub + ' locations yet. Add them on the airport\'s Edit page.';
        }

        fillSelect(siteTypeSelect, types, placeholder, selectedOr(siteTypeSelect));
        setHint(siteTypeHint, hint);
        refreshTerminalRequirement();
    }

    function refreshTerminals() {
        const terminals = airportData.terminals || [];
        const placeholder = !airportSelect.value
            ? '-- Select an Airport first --'
            : terminals.length ? '-- Select Terminal --' : '-- No terminals at this airport --';
        fillSelect(terminalSelect, terminals, placeholder, selectedOr(terminalSelect));
        refreshTerminalRequirement();
    }

    function refreshTerminalRequirement() {
        const required = categorySelect.value === config.terminalCategory
            && subCategorySelect.value === config.terminalSubCategory;
        terminalRequired.hidden = !required;
        terminalSelect.required = required;

        const noTerminals = airportSelect.value && (airportData.terminals || []).length === 0;
        setHint(terminalHint, required && noTerminals
            ? 'Required here, but this airport has no terminals yet. Add them on the airport\'s Edit page.'
            : required ? 'Required for Operations sites in a terminal.' : 'Optional for this kind of site.');
    }

    function setHint(el, text) {
        el.textContent = text;
        el.hidden = !text;
    }

    async function loadAirport() {
        airportData = { terminals: [], siteTypes: {} };

        if (airportSelect.value) {
            try {
                const response = await fetch(config.locationsUrl + '?airportId=' + encodeURIComponent(airportSelect.value));
                if (response.ok) airportData = await response.json();
            } catch (err) {
                setHint(siteTypeHint, 'Could not load this airport\'s areas. Check your connection and re-select the airport.');
            }
        }

        refreshTerminals();
        refreshSiteTypes();
    }

    airportSelect.addEventListener('change', loadAirport);
    categorySelect.addEventListener('change', refreshSubCategories);
    subCategorySelect.addEventListener('change', refreshSiteTypes);

    // Initial render: sub-categories first (static), then the airport's data.
    const initialSub = subCategorySelect.dataset.selected;
    const subs = config.subCategoriesByCategory[categorySelect.value] || [];
    fillSelect(subCategorySelect, subs, subs.length ? '-- Select Sub-Category --' : '-- Select Category first --', initialSub);
    delete subCategorySelect.dataset.selected;
    loadAirport();

    // Flag fields that came back with server-side errors, and clear the flag once edited.
    document.querySelectorAll('.form-group').forEach(function (group) {
        const errorSpan = group.querySelector('.form-group-error');
        if (errorSpan && errorSpan.textContent.trim()) group.classList.add('has-error');

        group.querySelectorAll('input, select, textarea').forEach(function (field) {
            ['input', 'change'].forEach(function (evt) {
                field.addEventListener(evt, function () { group.classList.remove('has-error'); });
            });
        });
    });
});
