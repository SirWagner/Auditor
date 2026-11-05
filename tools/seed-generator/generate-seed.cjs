// Builds Auditor/SeedData/acsa-seed.json from the client's audit spreadsheets in /docs.
//
//   cd tools/seed-generator && npm install && node generate-seed.cjs
//
// Personal data in the source files (contact names, e-mails, phone numbers) is deliberately not
// carried over: the seed ships inside the public Docker image.

const XLSX = require('xlsx');
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..', '..');
const DOCS = path.join(ROOT, 'docs');
const OUT = path.join(ROOT, 'Auditor', 'SeedData', 'acsa-seed.json');

// ---------------------------------------------------------------- helpers

const clean = v => String(v ?? '').replace(/[ ​]/g, ' ').replace(/\s+/g, ' ').trim();
const sheet = (file, name) =>
    XLSX.utils.sheet_to_json(XLSX.readFile(path.join(DOCS, file)).Sheets[name], { header: 1, defval: '' });
const unique = arr => [...new Set(arr.filter(Boolean))];
// Capitalises each word but keeps acronyms such as FIDS or ATM intact.
const titleCase = s => s.replace(/\w\S*/g, w => w[0].toUpperCase() + w.slice(1));

function splitReasons(raw) {
    return unique(String(raw ?? '')
        .split(/[•\r\n]+/)
        .map(r => clean(r).replace(/^[-–.]\s*/, '').replace(/\s*\.$/, ''))
        .filter(r => r.length > 1 && !/^(yes|no|other)$/i.test(r)))
        .map(r => r.charAt(0).toUpperCase() + r.slice(1));
}

function sectionPath(...parts) {
    const out = [];
    for (const p of parts.map(clean)) {
        if (!p || /^general$/i.test(p) || /field not in use/i.test(p)) continue;
        if (/^entry question/i.test(p)) continue;
        if (out.length && out[out.length - 1].toLowerCase() === p.toLowerCase()) continue;
        out.push(p);
    }
    return out.join(' › ').slice(0, 500);
}

// ---------------------------------------------------------------- reference data

// Verified against Wikipedia and airports.co.za contact pages (October 2026).
const AIRPORTS = [
    { code: 'JNB', icao: 'FAOR', name: 'O. R. Tambo International Airport', type: 'International', region: 'Gauteng', tel: '+27 11 921 6262', terminals: ['Terminal A', 'Terminal B'] },
    { code: 'CPT', icao: 'FACT', name: 'Cape Town International Airport', type: 'International', region: 'Western Cape', tel: '+27 21 937 1200', terminals: ['International Terminal', 'Domestic Terminal'] },
    { code: 'DUR', icao: 'FALE', name: 'King Shaka International Airport', type: 'International', region: 'KwaZulu-Natal', tel: '+27 32 436 6585', terminals: ['International Terminal', 'Domestic Terminal'] },
    { code: 'BFN', icao: 'FABL', name: 'Bram Fischer International Airport', type: 'International', region: 'Free State', tel: '+27 51 407 2200', terminals: ['Main Terminal'] },
    { code: 'PLZ', icao: 'FAPE', name: 'Chief Dawid Stuurman International Airport', type: 'International', region: 'Eastern Cape', tel: '+27 41 507 7348', terminals: ['Main Terminal'] },
    { code: 'UTN', icao: 'FAUP', name: 'Upington International Airport', type: 'International', region: 'Northern Cape', tel: '+27 54 337 7900', terminals: ['Main Terminal'] },
    { code: 'ELS', icao: 'FAEL', name: 'King Phalo Airport', type: 'Domestic', region: 'Eastern Cape', tel: '+27 43 706 0306', terminals: ['Main Terminal'] },
    { code: 'GRJ', icao: 'FAGG', name: 'George Airport', type: 'Domestic', region: 'Western Cape', tel: '+27 44 876 9310', terminals: ['Main Terminal'] },
    { code: 'KIM', icao: 'FAKM', name: 'Kimberley Airport', type: 'Domestic', region: 'Northern Cape', tel: '+27 53 830 7110', terminals: ['Main Terminal'] },
];
const REGIONAL = ['PLZ', 'ELS', 'GRJ', 'BFN', 'KIM', 'UTN'];

// From the "Service Standards Measurement Matrix Categories ACSA" sheet.
const CATEGORIES = {
    'Environment': 'Cleaning, compliance, hygiene, landscaping and surfaces: the measurement of all cleaning aspects visible to the eye and the general state of the area.',
    'Maintenance': 'The condition, functionality and repair of infrastructure, finishes and fixtures.',
    'Facilities': 'Availability of resources and equipment, and signage that says the right message at the right place.',
    'Productivity & People': 'Manpower deployment, utilisation and presence; queuing; retail service; correct uniform display and identification.',
    'Technology & Equipment': 'Inventory and stock levels, lighting, and the functionality of equipment with emphasis on downtime.',
    'Safety': 'All aspects pertaining to safety as it applies to the area and in relation to resource management.',
    'Waiting Time': 'The processing time, expressed as a time value, for a passenger or consumer within a process.',
    'Flight Information': 'Flight details recorded at the start of an observation, needed if an investigation takes place.',
    'Entry Questions': 'Gate questions that confirm whether an area or asset is compliant before the detailed checks.',
};
const MEASURE_TO_CATEGORY = {
    'environment': 'Environment',
    'maintenance': 'Maintenance',
    'facilities': 'Facilities',
    'people & productivity': 'Productivity & People',
    'productivity': 'Productivity & People',
    'technology & equipment': 'Technology & Equipment',
    'safety': 'Safety',
    'waiting time': 'Waiting Time',
    'entry question': 'Entry Questions',
};
const categoryFor = measure => {
    const key = clean(measure).toLowerCase().replace(/ and /g, ' & ').replace(/^maintainance$/, 'maintenance').replace(/^entry questions?$/, 'entry question');
    return MEASURE_TO_CATEGORY[key] ?? 'Environment';
};

const CONTRACTOR_ALIASES = { 'cc': 'Cleaning Company', 'm&e': 'M&E', 'm&e/it': 'M&E / IT', 'acs': 'ACS' };
function contractorFor(raw) {
    const c = clean(raw).replace(/^A1-A6\s+/i, '');
    if (!c) return null;
    return CONTRACTOR_ALIASES[c.toLowerCase()] ?? c;
}

// Who answers for a question. The SSMM sheets use the key "0 = No Responsibility, 1 = ACSA, 2 = Ramp Handling
// Company"; the other sheets give a contractor type or name a company. People's names, e-mail addresses and phone
// numbers in those sheets are deliberately not imported.
const CLEANING = 'Cleaning Company';
const RAMP = 'Ramp Handling Company';
const SSMM_RESPONSIBILITY = { '0': 'No Responsibility', '1': 'ACSA', '2': RAMP };
const NAMED_PROVIDERS = [
    [/hetlisa/i, 'Hetlisa', CLEANING],
    [/thuma\s*thina/i, 'Thuma Thina', CLEANING],
    [/lesedi mzansi/i, 'Lesedi Mzansi', CLEANING],
    [/sesfikile/i, 'Sesfikile', CLEANING],
    [/\btov\b/i, 'TOV', CLEANING],
    [/supercare/i, 'SuperCare Cleaning', CLEANING],
    [/bidvest prestige/i, 'Bidvest Prestige', CLEANING],
    [/\braps\b/i, 'RAPS', 'Security'],
    [/gauflora/i, 'GauFlora', 'Landscaping'],
];
const RESPONSIBILITY_ALIASES = {
    'cc': CLEANING, 'cleaning company': CLEANING,
    'm&e': 'M&E', 'm&e/it': 'M&E / IT', 'm&e / security': 'M&E / Security',
    'it': 'IT', 'i.t': 'IT',
    'projects signage': 'Projects Signage', 'projects - signage': 'Projects Signage',
    'landside': 'Landside Management', 'landside management': 'Landside Management',
};
function partyFor(raw) {
    const c = clean(raw);
    if (!c || /to be determined/i.test(c)) return { responsibility: null, provider: null };
    const named = NAMED_PROVIDERS.find(([re]) => re.test(c));
    if (named) return { responsibility: named[2], provider: named[1] };
    return { responsibility: RESPONSIBILITY_ALIASES[c.toLowerCase()] ?? c, provider: null };
}

// Questions answered from a list instead of typed.
const LIST_QUESTIONS = [
    [/^record name of the ground handling company/i, 'GROUND_HANDLERS'],
    [/^record the airline/i, 'AIRLINES'],
    [/^record aircraft type/i, 'AIRCRAFT_TYPES'],
    [/^wide\s*,\s*narrow/i, 'BODY_TYPES'],
    [/^parking bay/i, 'PARKING_BAYS'],
];

const NARROW = 'Narrow body', WIDE = 'Wide body', CODE_F = 'Code F';
const REFERENCE_LISTS = [
    {
        code: 'AIRLINES', name: 'Airlines', description: 'Carriers auditors can pick on flight observations. A starting list: add, rename or deactivate to match each airport.',
        items: [
            ...[['South African Airways', 'SA'], ['Airlink', '4Z'], ['FlySafair', 'FA'], ['LIFT', ''], ['CemAir', '5Z']].map(a => [...a, 'South African']),
            ...[['Air Botswana', 'BP'], ['Air Mauritius', 'MK'], ['Air Seychelles', 'HM'], ['Air Tanzania', 'TC'], ['Air Zimbabwe', 'UM'], ['ASKY Airlines', 'KP'], ['EgyptAir', 'MS'], ['Ethiopian Airlines', 'ET'], ['fastjet', 'FN'], ['FlyNamibia', 'OV'], ['Kenya Airways', 'KQ'], ['LAM Mozambique', 'TM'], ['Malawian Airlines', '3W'], ['Proflight Zambia', 'P0'], ['RwandAir', 'WB'], ['TAAG Angola Airlines', 'DT'], ['Uganda Airlines', 'UR']].map(a => [...a, 'Africa']),
            ...[['Air China', 'CA'], ['Air France', 'AF'], ['British Airways', 'BA'], ['Cathay Pacific', 'CX'], ['Condor', 'DE'], ['Delta Air Lines', 'DL'], ['Edelweiss Air', 'WK'], ['Emirates', 'EK'], ['Etihad Airways', 'EY'], ['KLM', 'KL'], ['Lufthansa', 'LH'], ['Qantas', 'QF'], ['Qatar Airways', 'QR'], ['Saudia', 'SV'], ['Singapore Airlines', 'SQ'], ['Swiss', 'LX'], ['Turkish Airlines', 'TK'], ['United Airlines', 'UA'], ['Virgin Atlantic', 'VS']].map(a => [...a, 'International']),
        ],
    },
    {
        code: 'AIRCRAFT_TYPES', name: 'Aircraft Types', description: 'Aircraft types for flight observations. The group is the body type, filled in automatically when an auditor picks the aircraft.',
        items: [
            ...['Airbus A319', 'Airbus A320', 'Airbus A320neo', 'Airbus A321', 'Airbus A321neo', 'Airbus A220-300', 'Boeing 737-400', 'Boeing 737-500', 'Boeing 737-700', 'Boeing 737-800', 'Boeing 737 MAX 8', 'Embraer E170', 'Embraer E175', 'Embraer E190', 'Embraer E195', 'Embraer ERJ 135', 'Embraer ERJ 145', 'ATR 42', 'ATR 72', 'De Havilland Dash 8-300', 'De Havilland Dash 8-400', 'Bombardier CRJ200', 'Bombardier CRJ700', 'Bombardier CRJ900', 'Avro RJ85', 'BAe Jetstream 41', 'Cessna 208 Caravan'].map(n => [n, '', NARROW]),
            ...['Airbus A330-200', 'Airbus A330-300', 'Airbus A330-900neo', 'Airbus A340-300', 'Airbus A340-600', 'Airbus A350-900', 'Airbus A350-1000', 'Boeing 747-400', 'Boeing 767-300', 'Boeing 777-200', 'Boeing 777-300ER', 'Boeing 787-8', 'Boeing 787-9', 'Boeing 787-10'].map(n => [n, '', WIDE]),
            ...['Airbus A380-800', 'Boeing 747-8'].map(n => [n, '', CODE_F]),
        ],
    },
    {
        code: 'BODY_TYPES', name: 'Aircraft Body Types', description: 'Wide, narrow and Code F (A380, 748), as asked on the SSMM flight information.',
        items: [[NARROW, '', ''], [WIDE, '', ''], [CODE_F, '', '']],
    },
    {
        code: 'GROUND_HANDLERS', name: 'Ground Handling Companies', description: 'Ramp handling companies. The one recorded on a flight audit is the service provider its ramp-handling questions are reported against.',
        items: ['BidAir Services', 'Menzies Aviation', 'Swissport'].map(n => [n, '', RAMP]),
    },
    {
        code: 'PARKING_BAYS', name: 'Parking Bays', description: 'Parking bays and stands. Add them per airport; until an airport has bays listed, auditors type the bay number.',
        items: [],
    },
    {
        code: 'SERVICE_PROVIDERS', name: 'Service Providers', description: 'Named contractors, from the ACSA checklists. Used on template questions and sites so reports can show who was responsible.',
        items: NAMED_PROVIDERS.map(([, name, group]) => [name, '', group]),
    },
    {
        code: 'RESPONSIBILITIES', name: 'Responsibilities', description: 'Who answers for a question. The first three are the SSMM key (0, 1, 2); the others come from the Responsible Contractor columns.',
        items: [
            ['No Responsibility', '0', ''], ['ACSA', '1', ''], [RAMP, '2', ''],
            ...['ACS', 'Airline', 'Cargo Agent', CLEANING, 'Concessionaire', 'Customer Care', 'Government', 'IT', 'Landscaping', 'Landside Management', 'M&E', 'M&E / IT', 'M&E / Security', 'Maintenance', 'Metro Police', 'Projects Signage', 'Security', 'Surface Maintenance'].map(n => [n, '', '']),
        ],
    },
];

// ---------------------------------------------------------------- accumulators

const questions = [];               // { text, type, category, recommendation, contractor, reasons, allowNotApplicable }
const questionIndex = new Map();    // normalized text|type -> index
const templates = new Map();        // name -> { name, description, items: [] }
const locations = {};               // code -> { Terminal:Set, TerminalLocation:Set, Airside:Set, Landside:Set }
const sites = new Map();            // code|name -> site

AIRPORTS.forEach(a => {
    locations[a.code] = { Terminal: new Set(a.terminals), TerminalLocation: new Set(), Airside: new Set(), Landside: new Set() };
});

function addQuestion({ text, type = 'YES_NO', category, recommendation, contractor, reasons = [], optional = false }) {
    text = clean(text).replace(/\s+([?.])$/, '$1');
    if (!text) return null;
    const list = LIST_QUESTIONS.find(([re]) => re.test(text))?.[1] ?? null;
    if (list) type = 'LIST';
    const key = `${text.toLowerCase().replace(/[^a-z0-9]+/g, ' ').trim()}|${type}`;
    let idx = questionIndex.get(key);
    if (idx === undefined) {
        idx = questions.length;
        questions.push({ text, type, list, category, recommendation: clean(recommendation) || null, contractor: contractor || null, reasons: [], allowNotApplicable: false });
        questionIndex.set(key, idx);
    }
    const q = questions[idx];
    q.recommendation ??= clean(recommendation) || null;
    q.contractor ??= contractor || null;
    for (const r of reasons) if (q.reasons.length < 25 && !q.reasons.some(x => x.toLowerCase() === r.toLowerCase())) q.reasons.push(r);
    if (optional) q.allowNotApplicable = true;
    return idx;
}

function templateFor(name, description) {
    if (!templates.has(name)) templates.set(name, { name, description, items: [] });
    return templates.get(name);
}

function addSite(site) {
    const key = `${site.airport}|${site.name.toLowerCase()}`;
    if (!sites.has(key)) sites.set(key, { ...site, templates: [] });
    const s = sites.get(key);
    s.serviceProvider ??= site.serviceProvider ?? null;
    for (const t of site.templates ?? []) if (!s.templates.includes(t)) s.templates.push(t);
    return s;
}

// "T ORTIA Terminal A Arrivals Performance Observation" -> "Terminal A Arrivals Performance Observation"
const auditTitle = raw => clean(raw).replace(/^[TL]\s+/, '').replace(/^(ORTIA|CTIA|KSIA)\s+/, '');

function terminalFromAudit(code, audit) {
    const a = audit.toLowerCase();
    if (/terminal a/.test(a)) return 'Terminal A';
    if (/terminal b/.test(a)) return 'Terminal B';
    if (/parkade|landside|roadside|bussing/.test(a)) return null;
    if (REGIONAL.includes(code)) return 'Main Terminal';
    if (/domestic/.test(a)) return 'Domestic Terminal';
    return 'International Terminal';
}

// ---------------------------------------------------------------- performance observations (terminal + landside)

function importObservation(file, sheetName, kind) {
    const rows = sheet(file, sheetName).slice(1).filter(r => clean(r[0]) && clean(r[7]));
    for (const r of rows) {
        const [airportRaw, auditRaw, main, section, sub, subSub, measure, text, reasonsRaw, recommendation, optional, contractor] = r;
        let audit = auditTitle(auditRaw);
        const codes = clean(airportRaw) === 'Regional' ? REGIONAL : [clean(airportRaw)];
        const isRegional = clean(airportRaw) === 'Regional';
        if (/bussing/i.test(audit)) audit = 'Landside Roadside Performance Observation';
        if (isRegional) audit = 'Landside Performance Observation';

        const templateName = isRegional ? `Regional ${audit}` : `${codes[0]} ${audit}`;
        const template = templateFor(templateName,
            isRegional
                ? `Landside performance observation for the regional airports (${REGIONAL.join(', ')}). Imported from the ACSA Landside spreadsheet.`
                : `${audit} at ${AIRPORTS.find(a => a.code === codes[0]).name}. Imported from the ACSA ${kind} spreadsheet.`);

        const isEntry = /entry question/i.test(clean(measure));
        const q = addQuestion({
            text,
            category: categoryFor(measure),
            recommendation,
            contractor: contractorFor(contractor),
            reasons: isEntry ? [] : splitReasons(reasonsRaw),
            optional: /optional/i.test(clean(optional)),
        });
        if (q === null) continue;

        template.items.push({
            ...partyFor(contractor),
            question: q,
            // An entry question opens its group ("Counters"); the detailed checks sit in the sub-locations below it.
            section: isEntry ? sectionPath(main, section, sub) : sectionPath(main, section, sub, subSub),
            mandatory: !/optional/i.test(clean(optional)),
        });

        for (const code of codes) {
            const loc = locations[code];
            const terminal = terminalFromAudit(code, audit);
            if (kind === 'Landside') {
                loc.Landside.add(sectionPath(main, section).replace(' › ', ' – '));
            } else if (clean(section)) {
                loc.TerminalLocation.add(clean(section));
            }

            const siteName = kind === 'Landside' ? 'Landside Roadside' : audit.replace(/ Performance Observation$/, '');
            addSite({
                airport: code,
                name: siteName,
                siteCategory: 'Operations',
                siteSubCategory: kind === 'Landside' ? 'Landside' : 'Terminal',
                siteType: kind === 'Landside' ? 'Roadside (all areas)' : (/arrival/i.test(audit) ? 'Arrivals (all areas)' : 'Departures (all areas)'),
                terminal,
                templates: [templateName],
            });
            if (kind === 'Landside') loc.Landside.add('Roadside (all areas)');
            else loc.TerminalLocation.add(/arrival/i.test(audit) ? 'Arrivals (all areas)' : 'Departures (all areas)');
        }
    }
}

importObservation('International Arrivals (1).xlsx', 'International Arrivals', 'Arrivals');
importObservation('International Departures (1).xlsx', 'International Departures', 'Departures');
importObservation('Landside (1).xlsx', 'Database', 'Landside');

// ---------------------------------------------------------------- washrooms

const WASHROOM_TEMPLATE = 'Washroom Performance Observation';
{
    const template = templateFor(WASHROOM_TEMPLATE,
        'Washroom inspection covering female, male, disabled and baby-room facilities. Imported from the ACSA Washroom Audit Template (February 2017).');
    const typeOrder = ['Female', 'Male', 'Disabled', 'Baby Room'];
    const rows = sheet('Washroom Audit Template_Airports_February 2017 (1).xlsx', 'Database').slice(1).filter(r => clean(r[4]));
    rows.sort((a, b) => (typeOrder.indexOf(clean(a[1])) - typeOrder.indexOf(clean(b[1]))) || (Number(a[3]) - Number(b[3])));
    for (const r of rows) {
        const [, type, measure, , text, reasonsRaw, recommendation, optional, contractor] = r;
        const q = addQuestion({
            text,
            category: categoryFor(measure),
            recommendation,
            contractor: contractorFor(contractor),
            reasons: splitReasons(reasonsRaw),
            optional: /optional/i.test(clean(optional)),
        });
        // Tabs: Female, Male, Special (disabled and baby-room facilities).
        const section = /^(disabled|baby room)$/i.test(clean(type)) ? sectionPath('Special', type) : clean(type);
        template.items.push({ ...partyFor(contractor), question: q, section, mandatory: !/optional/i.test(clean(optional)) });
    }

    const locs = sheet('Washroom Performance Observation Locations All_February 2017 (1).xlsx', 'Database Washrooms')
        .slice(1).filter(r => clean(r[0]));
    for (const r of locs) {
        const code = clean(r[0]);
        if (!locations[code]) continue;
        const audit = clean(r[1]);
        const main = clean(r[2]);
        const section = clean(r[3]);
        const where = clean(r[5]) || section;
        const isLandside = /parkade/i.test(main) || /parkade/i.test(audit);
        const terminal = terminalFromAudit(code, audit);

        if (isLandside) locations[code].Landside.add(section);
        else locations[code].TerminalLocation.add(section);

        const baseName = `Washrooms – ${titleCase(where)}`;
        let name = baseName;
        const taken = n => {
            const s = sites.get(`${code}|${n.toLowerCase()}`);
            return s && !(s.siteType === section && s.terminal === terminal);
        };
        if (taken(name)) name = `${baseName} (${section})`;
        if (taken(name) && terminal) name = `${baseName} (${terminal}, ${section})`;

        addSite({
            airport: code,
            name: name.slice(0, 200),
            siteCategory: 'Operations',
            siteSubCategory: isLandside ? 'Landside' : 'Terminal',
            siteType: section,
            terminal: isLandside ? null : terminal,
            // Each washroom location names its cleaning contractor (the contact columns are not imported).
            serviceProvider: partyFor(r[6]).provider,
            templates: [WASHROOM_TEMPLATE],
        });
    }
}

// ---------------------------------------------------------------- departure baggage (SSMM)

{
    const name = 'Departure Baggage Performance Observation';
    const template = templateFor(name,
        'Ramp observation of a departing flight: flight details, facilities, turnaround times and safety. Imported from the ACSA Departures SSMM (Version 2).');
    const rows = sheet('Departures SSMM Version 2 (1).xls', 'Manual').slice(4);
    let heading = '';
    for (const r of rows) {
        const no = clean(r[0]);
        if (/^\d+\.\s*\D/.test(no) && !clean(r[1])) {
            heading = clean(no.replace(/^\d+\.\s*/, '').split(':')[0]).replace('Fligth', 'Flight');
            continue;
        }
        const text = clean(r[1]);
        if (!text) continue;
        const kind = clean(r[14]).toLowerCase();
        let type = 'YES_NO';
        if (kind.startsWith('date and time')) type = /time/i.test(text) ? 'TIME' : 'DATE';
        else if (kind === 'date') type = 'TIME';
        else if (kind === 'data' || kind === 'numeral' || !kind) type = 'TEXT';

        const responsibility = String(r[6]).trim();
        const category = clean(r[21]) ? categoryFor(r[21]) : (CATEGORIES[heading] ? heading : 'Facilities');
        const q = addQuestion({
            text,
            type,
            category,
            recommendation: clean(r[15]),
            contractor: responsibility === '2' ? 'Ramp Handling Company' : responsibility === '1' ? 'ACSA' : null,
            reasons: type === 'YES_NO' ? splitReasons(r[7]) : [],
            optional: /^yes$/i.test(clean(r[13])),
        });
        template.items.push({ responsibility: SSMM_RESPONSIBILITY[responsibility] ?? null, provider: null, question: q, section: heading, mandatory: !/^yes$/i.test(clean(r[13])) });
    }
}

// ---------------------------------------------------------------- ground handling PAU / wheelchair

{
    const name = 'Ground Handling PAU Performance Observation';
    const template = templateFor(name,
        'Passenger assistance unit (PAU) and wheelchair service observation for ground handlers. Imported from the ACSA Ground Handling PAU spreadsheet (Version 2).');
    const rows = sheet('Ground Handling PAU Performance Version 2 (1).xls', 'Pre Defined Comments').slice(1);
    let section = 'PAU Staff';
    let current = null;
    const flush = () => {
        if (!current) return;
        const t = current.text.toLowerCase();
        const category = /clean/.test(t) ? 'Environment'
            : /available|availability/.test(t) ? 'Facilities'
            : /safe|secured|illness|injury|shoes/.test(t) ? 'Safety'
            : 'Productivity & People';
        const q = addQuestion({ text: current.text, category, contractor: RAMP, reasons: current.reasons });
        template.items.push({ responsibility: RAMP, provider: null, question: q, section, mandatory: true });
        current = null;
    };
    for (const r of rows) {
        const text = clean(r[0]);
        if (/^wheelchair staff$/i.test(text)) { flush(); section = 'Wheelchair Staff'; continue; }
        if (text) {
            flush();
            current = { text: text.replace(/\s*\?*$/, '?').replace(/ - \(/, ' (').replace(/ – \(/, ' ('), reasons: [] };
        }
        const reason = clean(r[4]);
        if (current && reason) current.reasons.push(...splitReasons(reason));
    }
    flush();
}

// ---------------------------------------------------------------- arrival baggage (SSMM)
// Columns are found by header text because the two files lay them out differently.
// The Version 2 file also carries ground-handler staff e-mail addresses; those columns are ignored.

function importSsmm(file, sheetName, name, description) {
    const template = templateFor(name, description);
    const rows = sheet(file, sheetName);
    const headerIndex = rows.findIndex(r => /^question no/i.test(clean(r[0])));
    const header = rows[headerIndex].map(h => clean(h).toLowerCase());
    const col = re => header.findIndex(h => re.test(h));
    const cQuestion = col(/^question(?! no)/);
    const cResponsibility = col(/^responsibility/);
    const cReasons = col(/predefined/);
    const cOptional = col(/^optional/);
    const cType = col(/^question type/);
    const cRecommendation = col(/recommendation/);

    let heading = '';
    for (const r of rows.slice(headerIndex + 1)) {
        const no = clean(r[0]);
        const text = clean(r[cQuestion]);
        // Section headings ("2. Productivity") sit in the number column, or alone in the question column.
        const headingText = /^\d+\.\s*\D/.test(no) && !text ? no
            : !no && /^\d+\.\s*[A-Za-z]/.test(text) && r.slice(cQuestion + 1).every(c => !clean(c)) ? text
            : null;
        if (headingText) {
            heading = clean(headingText.replace(/^\d+\.\s*/, '').split(':')[0]).replace('Fligth', 'Flight');
            continue;
        }
        if (!text || /^acsa qa$/i.test(text)) continue;

        const kind = cType >= 0 ? clean(r[cType]).toLowerCase() : '';
        let type = 'YES_NO';
        if (kind.startsWith('date')) type = /^record date/i.test(text) ? 'DATE' : 'TIME';
        else if (kind === 'numeric') type = 'NUMBER';
        else if (kind === 'data') type = 'TEXT';
        else if (!kind && /^(record|time |estimated time|actual time)/i.test(text))
            type = /total number/i.test(text) ? 'NUMBER' : /^record date/i.test(text) ? 'DATE' : /^(time |estimated time|actual time|record time)/i.test(text) ? 'TIME' : 'TEXT';
        if (heading === 'Flight Information' && type === 'YES_NO')
            type = 'TEXT';

        const responsibility = cResponsibility >= 0 ? String(r[cResponsibility]).trim() : '';
        const optional = cOptional >= 0 && /^yes$/i.test(clean(r[cOptional]));
        const q = addQuestion({
            text: text.replace(/\s*\.\s*$/, '').replace(/\s+\?/, '?'),
            type,
            category: CATEGORIES[heading] ? heading : heading === 'Productivity' ? 'Productivity & People' : heading === 'Technology' ? 'Technology & Equipment' : 'Facilities',
            recommendation: cRecommendation >= 0 ? clean(r[cRecommendation]) : null,
            contractor: responsibility === '2' ? 'Ramp Handling Company' : responsibility === '1' ? 'ACSA' : null,
            reasons: type === 'YES_NO' && cReasons >= 0 ? splitReasons(r[cReasons]) : [],
            optional,
        });
        if (!template.items.some(i => i.question === q))
            template.items.push({ responsibility: SSMM_RESPONSIBILITY[responsibility] ?? null, provider: null, question: q, section: heading, mandatory: !optional });
    }
}

const ARRIVAL_BAGGAGE = 'Arrival Baggage Performance Observation';
const ARRIVAL_BAGGAGE_REGIONAL = 'Arrival Baggage Performance Observation (Regional)';
importSsmm('Arrivals SSMM Version 2 (1).xls', 'Feedback From SP ', ARRIVAL_BAGGAGE,
    'Ramp observation of an arriving flight: flight details, productivity, waiting times, safety and baggage technology. Imported from the ACSA Arrivals SSMM (Version 2).');
importSsmm('Arrivals SSMM Regional Version 1 (1).xls', 'Arrivals', ARRIVAL_BAGGAGE_REGIONAL,
    'Arrival ramp observation for the regional airports. Imported from the ACSA Arrivals SSMM Regional (Version 1).');

// ---------------------------------------------------------------- cargo QMS (JNB)

const CARGO_TEMPLATE = 'JNB Cargo Performance Observation';
{
    const template = templateFor(CARGO_TEMPLATE,
        'Cargo precinct inspection at O. R. Tambo: security points, general areas, washrooms, approach roads and parking. Imported from the ACSA Cargo QMS (Version 2).');
    const file = 'CARGO Version 2 (1).xlsx';
    const buildings = new Set();

    for (const sheetName of ['Security points and guard house', 'General Areas', 'Washrooms', 'Landside']) {
        const rows = sheet(file, sheetName);
        const header = rows[0].map(h => clean(h).toLowerCase());
        const col = re => header.findIndex(h => re.test(h));
        const cSection = col(/^section/), cSub = col(/^sub section/), cLoc = col(/^sub, sub location/), cType = col(/^type$/),
            cMeasure = col(/^measure/), cQuestion = col(/^questions/), cReasons = col(/pre-defined/), cRec = col(/recommendation/),
            cContractor = col(/contractor/), cOptional = col(/^optional/);

        for (const r of rows.slice(1)) {
            const text = clean(r[cQuestion]);
            if (!text) continue;
            String(r[cSection]).split('•').map(clean).filter(Boolean)
                .filter(b => !/swissport 7/i.test(b))   // typo in one row of the source
                .forEach(b => sheetName !== 'Landside' && buildings.add(b));

            const optional = /^yes$/i.test(clean(r[cOptional]));
            const isEntry = /entry question/i.test(clean(r[cMeasure]));
            const washroomType = cType >= 0 ? clean(r[cType]).replace(/•/g, '/').replace(/^\/\s*/, '').replace(/\s+\/\s+/g, ' / ') : '';
            const q = addQuestion({
                text,
                category: categoryFor(r[cMeasure]),
                recommendation: clean(r[cRec]),
                contractor: contractorFor(clean(r[cContractor]).replace(/^contracted security company - /i, '')),
                reasons: isEntry ? [] : splitReasons(r[cReasons]),
                optional,
            });
            const section = sheetName === 'Landside'
                ? sectionPath(r[cSection], r[cLoc])
                : sectionPath(r[cSub], r[cLoc]) + (washroomType ? ` (${washroomType})` : '');
            template.items.push({ ...partyFor(r[cContractor]), question: q, section, mandatory: !optional });
        }
    }

    const jnb = locations.JNB;
    jnb.Landside.add('Cargo Precinct');
    for (const b of [...buildings].sort())
        addSite({ airport: 'JNB', name: `Cargo – ${b}`, siteCategory: 'Operations', siteSubCategory: 'Landside', siteType: 'Cargo Precinct', terminal: null, templates: [CARGO_TEMPLATE] });
    addSite({ airport: 'JNB', name: 'Cargo – Approach Roads & Parking', siteCategory: 'Operations', siteSubCategory: 'Landside', siteType: 'Cargo Precinct', terminal: null, templates: [CARGO_TEMPLATE] });
}

// ---------------------------------------------------------------- commercial (concessions)
// The "SSMM" sheet is the master question bank; each outlet sheet marks which questions apply,
// in which stage of the visit (Arrival, Seating, Browsing, Return) and in what order. Y = required, O = optional.

const COMMERCIAL_TEMPLATES = {
    'F&B Sit Down': { name: 'Commercial – F&B Sit-Down', sub: 'Food & Beverage', type: 'Sit-Down' },
    'F&B Take Away with Sit Down': { name: 'Commercial – F&B Sit-Down with Take-Away', sub: 'Food & Beverage', type: 'Sit-Down with Take-Away' },
    'F&B Take Away': { name: 'Commercial – F&B Take-Away', sub: 'Food & Beverage', type: 'Take-Away' },
    'Retail': { name: 'Commercial – Retail', sub: 'Retail', type: 'Retail' },
    'Services': { name: 'Commercial – Services', sub: 'Services', type: 'Services' },
    'Car Rental': { name: 'Commercial – Car Rental', sub: 'Car Rental', type: 'Car Rental' },
};
{
    const file = 'Commercial Version 1 (002) (1).xlsx';

    // Master: question → category, reasons, recommendation and the detailed checks behind it.
    const master = new Map();
    let category = '', current = null;
    for (const r of sheet(file, 'SSMM').slice(1)) {
        if (clean(r[2])) category = categoryFor(clean(r[2]));
        const text = clean(r[4]);
        if (text) {
            current = { text, category, reasons: [], checks: [], recommendation: clean(r[10]) };
            master.set(text.toLowerCase(), current);
        }
        if (!current) continue;
        const check = clean(r[6]).replace(/^\d+(\.\d+)*\s*/, '');
        if (check) current.checks.push(check);
        current.reasons.push(...splitReasons(r[8]));
    }

    for (const [sheetName, t] of Object.entries(COMMERCIAL_TEMPLATES)) {
        const rows = sheet(file, sheetName);
        const stageRow = rows.find(r => /^environment$/i.test(clean(r[1])));
        const stages = [[7, 8], [9, 10]].map(([qn, flag]) => ({ qn, flag, name: clean(stageRow[flag]) })).filter(s => s.name);
        const picked = [];
        let block = 0, blockName = '';
        rows.forEach((r, rowIndex) => {
            if (/^(environment|maintainance|maintenance|people & productivity)$/i.test(clean(r[1])) && !clean(r[0])) {
                block++;
                blockName = /^mainta/i.test(clean(r[1])) ? 'Maintenance' : clean(r[1]);
                return;
            }
            const text = clean(r[1]);
            const m = master.get(text.toLowerCase());
            if (!m) return;
            stages.forEach((s, stageIndex) => {
                const flag = clean(r[s.flag]).toUpperCase();
                const qn = Number(r[s.qn]);
                if (!flag && !qn) return;
                picked.push({ m, stageIndex, stage: s.name, block, blockName, qn: qn || 99, rowIndex, optional: flag === 'O' });
            });
        });
        picked.sort((a, b) => a.stageIndex - b.stageIndex || a.block - b.block || a.qn - b.qn || a.rowIndex - b.rowIndex);

        const template = templateFor(t.name,
            `Concession inspection for ${t.type} outlets, by stage of the customer visit (${stages.map(s => s.name).join(', ')}). Imported from the ACSA Commercial checklist (Version 1).`);
        for (const p of picked) {
            const recommendation = [p.m.recommendation, p.m.checks.length ? `Check: ${p.m.checks.join('; ')}.` : ''].filter(Boolean).join(' ');
            const q = addQuestion({ text: p.m.text, category: p.m.category, recommendation, contractor: 'Concessionaire', reasons: p.m.reasons, optional: p.optional });
            // Stage of the visit, then the checklist block, in the sheet's QN order: "Arrival › Environment", "Seating › Maintenance".
            // On a one-stage checklist the stage level is shared by every question, so the screens tab on the blocks.
            template.items.push({ responsibility: 'Concessionaire', provider: null, question: q, section: sectionPath(p.stage, p.blockName), mandatory: !p.optional });
        }
    }
}

// ---------------------------------------------------------------- apron sites (ramp observations)

AIRPORTS.forEach(a => {
    locations[a.code].Airside.add('Apron (Parking Bays)');
    const regional = REGIONAL.includes(a.code);
    addSite({
        airport: a.code,
        name: 'Apron – Parking Bays',
        siteCategory: 'Operations',
        siteSubCategory: 'Airside',
        siteType: 'Apron (Parking Bays)',
        terminal: null,
        templates: ['Departure Baggage Performance Observation', regional ? ARRIVAL_BAGGAGE_REGIONAL : ARRIVAL_BAGGAGE, 'Ground Handling PAU Performance Observation'],
    });
});

// ---------------------------------------------------------------- flight details needed for reporting
// The SSMM sheets record a flight number but not the airline; the PAU sheet has one column per handler but no question
// saying which handler was observed. These questions make both reportable.

const NO_RESPONSIBILITY = { responsibility: SSMM_RESPONSIBILITY['0'], provider: null };
for (const name of ['Arrival Baggage Performance Observation', 'Arrival Baggage Performance Observation (Regional)', 'Departure Baggage Performance Observation']) {
    const template = templates.get(name);
    const q = addQuestion({ text: 'Record the airline', category: 'Flight Information' });
    const at = template.items.findIndex(i => /^record flight number/i.test(questions[i.question].text));
    if (at < 0) throw new Error(`No flight number question on ${name}`);
    template.items.splice(at, 0, { ...NO_RESPONSIBILITY, question: q, section: template.items[at].section, mandatory: true });
}
{
    const q = addQuestion({ text: 'Record name of the Ground handling company', category: 'Flight Information' });
    templates.get('Ground Handling PAU Performance Observation').items.unshift({ ...NO_RESPONSIBILITY, question: q, section: 'Flight Information', mandatory: true });
}

// ---------------------------------------------------------------- write

const seed = {
    source: 'ACSA audit spreadsheets in /docs; airport details from Wikipedia and airports.co.za',
    generated: new Date().toISOString().slice(0, 10),
    categories: Object.entries(CATEGORIES).map(([name, description]) => ({ name, description })),
    airports: AIRPORTS.map(a => ({
        code: a.code,
        name: a.name,
        airportType: a.type,
        region: a.region,
        country: 'South Africa',
        airportManager: 'Airport General Manager',
        emailAddress: 'customercare@airports.co.za',
        telNumber: a.tel,
        locations: Object.fromEntries(Object.entries(locations[a.code]).map(([k, v]) => [k, [...v].filter(Boolean).sort()])),
    })),
    referenceLists: REFERENCE_LISTS.map(l => ({
        ...l,
        items: l.items.map(([name, code, group], i) => ({ name, code: code || null, group: group || null, order: i + 1 })),
    })),
    questions,
    templates: [...templates.values()].map(t => ({
        ...t,
        items: t.items.map((it, i) => ({ ...it, sequence: i + 1 })),
    })),
    sites: [...sites.values()].map(s => ({ ...s, department: 'Operations' })),
};

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(seed, null, 1));

console.log(`Wrote ${path.relative(ROOT, OUT)}`);
console.log(`  categories ${seed.categories.length}, airports ${seed.airports.length}, questions ${questions.length}, templates ${seed.templates.length}, sites ${seed.sites.length}`);
for (const t of seed.templates) console.log(`  template ${t.name}: ${t.items.length} items`);
for (const a of seed.airports) console.log(`  ${a.code}: ` + Object.entries(a.locations).map(([k, v]) => `${k} ${v.length}`).join(', ') + `, sites ${seed.sites.filter(s => s.airport === a.code).length}`);
