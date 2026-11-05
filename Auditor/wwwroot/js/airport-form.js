// Airport Create/Edit: dependent Region dropdown and multi-value area pickers.
document.addEventListener('DOMContentLoaded', function () {
    const regionsByCountry = window.airportRegionsByCountry || {};
    const countrySelect = document.getElementById('countrySelect');
    const regionSelect = document.getElementById('regionSelect');

    function fillRegions(keepSelection) {
        if (!countrySelect || !regionSelect) return;

        const selected = keepSelection ? regionSelect.dataset.selected : '';
        const regions = regionsByCountry[countrySelect.value] || [];

        regionSelect.innerHTML = '';
        const placeholder = new Option(regions.length ? '-- Select Region --' : '-- Select Country first --', '');
        regionSelect.add(placeholder);

        regions.forEach(function (region) {
            regionSelect.add(new Option(region, region, false, region === selected));
        });
    }

    countrySelect?.addEventListener('change', function () { fillRegions(false); });
    fillRegions(true);

    document.querySelectorAll('.area-picker').forEach(function (picker) {
        const field = picker.dataset.field;
        const chips = picker.querySelector('.area-chips');
        const input = picker.querySelector('.area-add input');
        const addButton = picker.querySelector('.area-add-btn');

        function existingNames() {
            return Array.from(chips.querySelectorAll('input[type=hidden]'))
                .map(function (hidden) { return hidden.value.toLowerCase(); });
        }

        function addChip(rawName) {
            const name = rawName.replace(/\s+/g, ' ').trim();
            if (!name) return;
            if (existingNames().includes(name.toLowerCase())) {
                input.value = '';
                return;
            }

            const chip = document.createElement('span');
            chip.className = 'area-chip';

            const label = document.createElement('span');
            label.textContent = name;

            const hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = field;
            hidden.value = name;

            const remove = document.createElement('button');
            remove.type = 'button';
            remove.className = 'area-chip-remove';
            remove.setAttribute('aria-label', 'Remove ' + name);
            remove.innerHTML = '&times;';

            chip.append(label, hidden, remove);
            chips.appendChild(chip);
            input.value = '';
        }

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ',') {
                e.preventDefault();
                addChip(input.value);
            }
        });

        // Picking a datalist suggestion fires "input" with the full value; add it straight away.
        input.addEventListener('input', function (e) {
            if (e.inputType === 'insertReplacementText' || e.inputType === undefined) {
                const options = Array.from(document.getElementById(input.getAttribute('list'))?.options || []);
                if (options.some(function (o) { return o.value === input.value; })) addChip(input.value);
            }
        });

        addButton.addEventListener('click', function () { addChip(input.value); input.focus(); });

        chips.addEventListener('click', function (e) {
            const remove = e.target.closest('.area-chip-remove');
            if (remove) remove.closest('.area-chip').remove();
        });
    });

    // Don't lose a name that was typed but not yet added.
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            document.querySelectorAll('.area-picker').forEach(function (picker) {
                const input = picker.querySelector('.area-add input');
                if (input.value.trim()) picker.querySelector('.area-add-btn').click();
            });
        });
    });

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
