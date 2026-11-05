// Audit Schedule Create/Edit:
//  - airport → its active sites
//  - site → only the templates allowed there (template airport/site restrictions), linked ones first
//  - airport → only auditors who cover it (their airport, or all airports)
document.addEventListener('DOMContentLoaded', function () {
    const airportSelect = document.getElementById('airportSelect');
    const siteSelect = document.getElementById('siteSelect');
    const templateSelect = document.getElementById('templateSelect');
    const templateHint = document.getElementById('templateHint');
    if (!airportSelect || !siteSelect || !templateSelect) return;

    const allTemplates = Array.from(templateSelect.options).filter(o => o.value);
    const initialTemplate = templateSelect.value;
    let sites = [];

    function renderTemplates() {
        const site = sites.find(s => String(s.id) === siteSelect.value);
        const current = templateSelect.value || initialTemplate;

        templateSelect.innerHTML = '';

        if (!site) {
            templateSelect.add(new Option('-- Select a site first --', ''));
            templateSelect.disabled = true;
            templateHint.textContent = 'Only templates available for the selected site are listed.';
            return;
        }

        const allowed = new Set(site.templateIds.map(String));
        const linked = new Set(site.linkedTemplateIds.map(String));
        const linkedOptions = allTemplates.filter(o => allowed.has(o.value) && linked.has(o.value));
        const otherOptions = allTemplates.filter(o => allowed.has(o.value) && !linked.has(o.value));

        templateSelect.disabled = false;
        templateSelect.add(new Option(linkedOptions.length + otherOptions.length ? '-- Select a Template --' : '-- No templates available for this site --', ''));

        const addGroup = (label, options) => {
            if (!options.length) return;
            const group = document.createElement('optgroup');
            group.label = label;
            options.forEach(o => group.appendChild(o.cloneNode(true)));
            templateSelect.appendChild(group);
        };
        addGroup('Made for this site', linkedOptions);
        addGroup(linkedOptions.length ? 'Also available' : 'Available templates', otherOptions);

        templateSelect.value = allowed.has(String(current)) ? current : '';
        if (!templateSelect.value && linkedOptions.length === 1) templateSelect.value = linkedOptions[0].value;

        const total = linkedOptions.length + otherOptions.length;
        templateHint.textContent = total === 0
            ? 'No active template can be used here. Create one for this site or airport under Audit Templates.'
            : `${total} template(s) available for this site.`;
    }

    function filterAuditors() {
        const airport = airportSelect.value;
        document.querySelectorAll('.check-list label[data-airport]').forEach(label => {
            const own = label.dataset.airport;
            const covers = !own || !airport || own === airport;
            const box = label.querySelector('input');
            label.hidden = !covers && !box.checked;
            label.classList.toggle('auditor-mismatch', !covers && box.checked);
        });
    }

    async function loadSites(keepSelection) {
        const selected = keepSelection ? siteSelect.dataset.selected : '';
        siteSelect.innerHTML = '';
        sites = [];
        filterAuditors();

        if (!airportSelect.value) {
            siteSelect.add(new Option('-- Select an Airport first --', ''));
            renderTemplates();
            return;
        }

        siteSelect.add(new Option('Loading sites…', ''));
        try {
            const response = await fetch(`${window.scheduleForm.siteOptionsUrl}?airportId=${encodeURIComponent(airportSelect.value)}`);
            sites = response.ok ? await response.json() : [];
        } catch {
            sites = [];
        }

        siteSelect.innerHTML = '';
        siteSelect.add(new Option(sites.length ? '-- Select a Site --' : '-- No active sites at this airport --', ''));
        sites.forEach(s => siteSelect.add(new Option(s.name, s.id, false, String(s.id) === String(selected))));
        renderTemplates();
    }

    airportSelect.addEventListener('change', () => loadSites(false));
    siteSelect.addEventListener('change', renderTemplates);
    loadSites(true);

    document.querySelectorAll('.form-group').forEach(group => {
        const error = group.querySelector('.form-group-error');
        if (error && error.textContent.trim()) group.classList.add('has-error');
    });
});
