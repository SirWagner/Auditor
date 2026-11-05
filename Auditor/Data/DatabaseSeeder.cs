using System.Reflection;
using System.Text.Json;
using Auditor.Models;
using Auditor.Models.Lookups;
using Auditor.Security;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Data
{
    /// <summary>
    /// Seeds lookup data, the first administrator, and the ACSA reference data (airports, areas,
    /// question bank, audit templates and sites) on startup. Every step is idempotent: records are
    /// matched on their natural key and only missing ones are added, so user edits are never overwritten.
    /// </summary>
    public class DatabaseSeeder
    {
        public static readonly string[] QuestionTypes =
            { "YES_NO", "MULTIPLE_CHOICE", "TEXT", "NUMBER", "RATING", "BOOLEAN", "CHECKLIST", "DATE", "TIME", "PHOTO", "LIST" };

        public const string Compliant = "Compliant";
        public const string NotCompliant = "Not compliant";
        public const string NotApplicable = "Not applicable";

        private const string SeedResource = "Auditor.SeedData.acsa-seed.json";

        private readonly AuditorContext _context;
        private readonly PasswordService _passwords;
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AuditorContext context, PasswordService passwords, IConfiguration config, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _passwords = passwords;
            _config = config;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                if (!await _context.Database.CanConnectAsync())
                {
                    _logger.LogError("Seeding skipped: the database is not reachable.");
                    return;
                }

                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    _logger.LogError("Seeding skipped: the database has pending migrations. Run the auditor-migrate container first.");
                    return;
                }

                await SeedQuestionTypesAsync();
                await SeedDepartmentsAsync();
                var admin = await SeedAdministratorAsync();
                await BackfillEmployeeNumbersAsync();

                if (_config.GetValue("Seed:ReferenceData", true))
                    await SeedReferenceDataAsync(admin.Id);
            }
            catch (Exception ex)
            {
                // A seeding problem must not take the whole site down.
                _logger.LogError(ex, "Database seeding failed");
            }
        }

        private async Task SeedQuestionTypesAsync()
        {
            var existing = await _context.QuestionTypes.Select(t => t.Name).ToListAsync();
            foreach (var name in QuestionTypes.Except(existing, StringComparer.OrdinalIgnoreCase))
                _context.QuestionTypes.Add(new QuestionType { Name = name, Description = name.Replace('_', ' ').ToLowerInvariant() });
            await _context.SaveChangesAsync();
        }

        private async Task SeedDepartmentsAsync()
        {
            var defaults = new Dictionary<string, string>
            {
                ["Operations"] = "Airport operations",
                ["Commercial"] = "Food & beverage, retail and banking concessions",
                ["Quality Assurance"] = "Service standards and performance observations",
            };
            var existing = await _context.Departments.Select(d => d.Name).ToListAsync();
            foreach (var (name, description) in defaults.Where(d => !existing.Contains(d.Key, StringComparer.OrdinalIgnoreCase)))
                _context.Departments.Add(new Department { Name = name, Description = description });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates the super user from the Admin:* settings if that account doesn't exist yet.
        /// An existing account is left alone, so a changed password is never reset on restart.
        /// </summary>
        private async Task<AppUser> SeedAdministratorAsync()
        {
            var email = _config["Admin:Email"] ?? "admin@auditor.local";
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                if (user.Role != Roles.Admin || !user.IsActive)
                {
                    user.Role = Roles.Admin;
                    user.IsActive = true;
                    await _context.SaveChangesAsync();
                }
                return user;
            }

            var password = _config["Admin:Password"];
            if (string.IsNullOrEmpty(password))
            {
                password = "ChangeMe#2026";
                _logger.LogWarning("Admin:Password is not set; the super user {Email} was created with the default password. Change it after signing in.", email);
            }

            var employeeNumber = _config["Admin:EmployeeNumber"] ?? "ADM-0001";
            if (await _context.AppUsers.AnyAsync(u => u.EmployeeNumber == employeeNumber))
                employeeNumber = $"ADM-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            user = new AppUser
            {
                DisplayName = _config["Admin:Name"] ?? "System Administrator",
                Email = email,
                EmployeeNumber = employeeNumber,
                Role = Roles.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                AzureAdObjectId = Guid.NewGuid(),
            };
            user.PasswordHash = _passwords.Hash(user, password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created super user {Email} ({EmployeeNumber})", email, employeeNumber);
            return user;
        }

        // Employee number is mandatory for users; give accounts created before it existed a placeholder.
        private async Task BackfillEmployeeNumbersAsync()
        {
            var users = await _context.AppUsers.Where(u => u.EmployeeNumber == null || u.EmployeeNumber == "").ToListAsync();
            foreach (var user in users)
                user.EmployeeNumber = $"EMP-{user.Id:0000}";
            if (users.Count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Assigned placeholder employee numbers to {Count} existing user(s)", users.Count);
            }
        }

        // ------------------------------------------------------------------ reference data

        private async Task SeedReferenceDataAsync(long adminId)
        {
            var seed = LoadSeed();
            if (seed == null)
                return;

            var categories = await SeedCategoriesAsync(seed);
            var airports = await SeedAirportsAsync(seed);
            var lists = await SeedReferenceListsAsync(seed);
            var questions = await SeedQuestionsAsync(seed, categories, lists, adminId);
            var templates = await SeedTemplatesAsync(seed, questions, airports, adminId);
            await SeedSitesAsync(seed, airports, templates, adminId);
            await BackfillResponsibilityAsync();
        }

        /// <summary>
        /// Adds any missing list with its starting items. Items are only added when the list is created, so an item
        /// someone deleted doesn't come back on the next start.
        /// </summary>
        private async Task<Dictionary<string, ReferenceList>> SeedReferenceListsAsync(SeedFile seed)
        {
            var existing = await _context.ReferenceLists.ToListAsync();
            var added = 0;

            foreach (var l in seed.ReferenceLists)
            {
                if (existing.Any(e => e.Code == l.Code))
                    continue;

                var list = new ReferenceList { Code = l.Code, Name = l.Name, Description = l.Description, CreatedDate = DateTime.UtcNow };
                foreach (var i in l.Items)
                    list.Items.Add(new ReferenceListItem { Name = i.Name, Code = i.Code, GroupName = i.Group, DisplayOrder = i.Order, IsActive = true });

                _context.ReferenceLists.Add(list);
                existing.Add(list);
                added++;
            }

            await _context.SaveChangesAsync();
            if (added > 0)
                _logger.LogInformation("Seeded {Count} reference list(s)", added);

            return existing.Where(l => l.Code != null).ToDictionary(l => l.Code!);
        }

        /// <summary>Audits submitted before responsibilities existed get them from the current templates and sites.</summary>
        private async Task BackfillResponsibilityAsync()
        {
            var executionIds = await _context.AuditResponses
                .Where(r => r.Responsibility == null
                            && (r.Execution.Status == ExecutionStatuses.Completed || r.Execution.Status == ExecutionStatuses.Closed))
                .Select(r => r.ExecutionId)
                .Distinct()
                .ToListAsync();

            if (executionIds.Count == 0)
                return;

            var snapshot = new Auditor.Services.ResponsibilitySnapshot(_context);
            foreach (var id in executionIds)
                await snapshot.ApplyAsync(id);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Recorded responsibilities on {Count} submitted audit(s)", executionIds.Count);
        }

        private SeedFile? LoadSeed()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SeedResource);
            if (stream == null)
            {
                _logger.LogWarning("Reference data not found ({Resource}); skipping.", SeedResource);
                return null;
            }

            return JsonSerializer.Deserialize<SeedFile>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<Dictionary<string, long>> SeedCategoriesAsync(SeedFile seed)
        {
            var existing = await _context.QuestionCategories.ToListAsync();
            foreach (var c in seed.Categories.Where(c => !existing.Any(e => e.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase))))
            {
                var category = new QuestionCategory { Name = c.Name, Description = c.Description };
                _context.QuestionCategories.Add(category);
                existing.Add(category);
            }
            await _context.SaveChangesAsync();
            return existing.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, Airport>> SeedAirportsAsync(SeedFile seed)
        {
            var airports = await _context.Airports.Include(a => a.Locations).ToListAsync();
            var added = 0;

            foreach (var a in seed.Airports)
            {
                var airport = airports.FirstOrDefault(x => x.Code.Equals(a.Code, StringComparison.OrdinalIgnoreCase))
                    ?? airports.FirstOrDefault(x => x.Name.Equals(a.Name, StringComparison.OrdinalIgnoreCase));

                if (airport == null)
                {
                    airport = new Airport
                    {
                        Code = a.Code,
                        Name = a.Name,
                        AirportType = a.AirportType,
                        Region = a.Region,
                        Country = a.Country,
                        AirportManager = a.AirportManager,
                        EmailAddress = a.EmailAddress,
                        TelNumber = a.TelNumber,
                        IsActive = true,
                    };
                    _context.Airports.Add(airport);
                    airports.Add(airport);
                    added++;
                }

                foreach (var (areaType, names) in a.Locations)
                {
                    if (!AirportLookups.AreaTypes.ContainsKey(areaType))
                        continue;
                    foreach (var name in names.Where(n => !airport.Locations.Any(l => l.AreaType == areaType && l.Name.Equals(n, StringComparison.OrdinalIgnoreCase))))
                        airport.Locations.Add(new AirportLocation { AreaType = areaType, Name = name });
                }
            }

            await _context.SaveChangesAsync();
            if (added > 0)
                _logger.LogInformation("Seeded {Count} airport(s)", added);

            return seed.Airports.ToDictionary(
                a => a.Code,
                a => airports.First(x => x.Code.Equals(a.Code, StringComparison.OrdinalIgnoreCase) || x.Name.Equals(a.Name, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<Dictionary<int, long>> SeedQuestionsAsync(SeedFile seed, Dictionary<string, long> categories, Dictionary<string, ReferenceList> lists, long adminId)
        {
            var types = await _context.QuestionTypes.ToDictionaryAsync(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);
            var existing = await _context.QuestionBanks
                .Select(q => new { q.Id, q.QuestionText, q.QuestionTypeId })
                .ToListAsync();
            var textTypeId = types["TEXT"];
            var converted = 0;

            var map = new Dictionary<int, long>();
            var created = new List<(int Index, QuestionBank Entity)>();

            for (var i = 0; i < seed.Questions.Count; i++)
            {
                var q = seed.Questions[i];
                var typeId = types[q.Type];
                var listId = q.List != null && lists.TryGetValue(q.List, out var list) ? list.Id : (long?)null;
                var match = existing.FirstOrDefault(e => e.QuestionTypeId == typeId && e.QuestionText.Equals(q.Text, StringComparison.OrdinalIgnoreCase));

                // Questions that used to be typed become dropdowns. Answers already given stay as they are.
                if (match == null && listId != null)
                {
                    match = existing.FirstOrDefault(e => e.QuestionTypeId == textTypeId && e.QuestionText.Equals(q.Text, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        var question = await _context.QuestionBanks.FirstAsync(x => x.Id == match.Id);
                        question.QuestionTypeId = typeId;
                        question.ReferenceListId = listId;
                        converted++;
                    }
                }

                if (match != null)
                {
                    map[i] = match.Id;
                    continue;
                }

                var entity = new QuestionBank
                {
                    QuestionText = q.Text,
                    QuestionTypeId = typeId,
                    CategoryId = categories.TryGetValue(q.Category, out var cat) ? cat : categories.Values.First(),
                    ServiceStandardRecommendation = q.Recommendation,
                    ResponsibleContractor = q.Contractor,
                    ReferenceListId = listId,
                    IsActive = true,
                    CreatedBy = adminId,
                    CreatedDate = DateTime.UtcNow,
                };

                if (q.Type == "YES_NO")
                {
                    foreach (var option in ComplianceOptions(q.AllowNotApplicable))
                        entity.QuestionBankOptions.Add(option);
                    foreach (var reason in q.Reasons)
                        entity.QuestionBankReasons.Add(new QuestionBankReason { ReasonText = reason });
                }

                _context.QuestionBanks.Add(entity);
                created.Add((i, entity));
            }

            await _context.SaveChangesAsync();
            foreach (var (index, entity) in created)
                map[index] = entity.Id;

            if (created.Count > 0)
                _logger.LogInformation("Seeded {Count} question(s)", created.Count);
            if (converted > 0)
                _logger.LogInformation("Turned {Count} typed question(s) into dropdowns", converted);
            return map;
        }

        public static IEnumerable<QuestionBankOption> ComplianceOptions(bool allowNotApplicable)
        {
            yield return new QuestionBankOption { OptionText = Compliant, OptionValue = "Y", DisplayOrder = 1, RequiresReason = false };
            yield return new QuestionBankOption { OptionText = NotCompliant, OptionValue = "N", DisplayOrder = 2, RequiresReason = true };
            if (allowNotApplicable)
                yield return new QuestionBankOption { OptionText = NotApplicable, OptionValue = "NA", DisplayOrder = 3, RequiresReason = false };
        }

        private async Task<Dictionary<string, AuditTemplate>> SeedTemplatesAsync(SeedFile seed, Dictionary<int, long> questions, Dictionary<string, Airport> airports, long adminId)
        {
            var templates = await _context.AuditTemplates.Include(t => t.AuditTemplateItems).ToListAsync();
            var added = 0;
            var updated = 0;
            var questionsAdded = 0;

            foreach (var t in seed.Templates)
            {
                var existing = templates.FirstOrDefault(e => e.Name.Equals(t.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    var (changed, inserted) = SyncTemplate(existing, t, questions);
                    updated += changed;
                    questionsAdded += inserted;
                    continue;
                }

                var template = new AuditTemplate
                {
                    Name = t.Name,
                    Description = t.Description,
                    Version = "1.0",
                    IsActive = true,
                    CreatedBy = adminId,
                    CreatedDate = DateTime.UtcNow,
                    // "JNB Cargo …" style names belong to one airport; the rest are offered everywhere.
                    AirportId = airports.FirstOrDefault(a => t.Name.StartsWith(a.Key + " ", StringComparison.Ordinal)).Value?.Id,
                };

                foreach (var item in t.Items)
                {
                    template.AuditTemplateItems.Add(new AuditTemplateItem
                    {
                        QuestionBankId = questions[item.Question],
                        Sequence = item.Sequence,
                        Mandatory = item.Mandatory,
                        Section = string.IsNullOrWhiteSpace(item.Section) ? null : item.Section,
                        Responsibility = item.Responsibility,
                        ServiceProvider = item.Provider,
                    });
                }

                _context.AuditTemplates.Add(template);
                templates.Add(template);
                added++;
            }

            await _context.SaveChangesAsync();
            if (added > 0)
                _logger.LogInformation("Seeded {Count} audit template(s)", added);
            if (updated > 0)
                _logger.LogInformation("Updated {Count} template question(s) to match the checklists", updated);
            if (questionsAdded > 0)
                _logger.LogInformation("Added {Count} question(s) to existing templates", questionsAdded);

            return templates.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Brings an already-seeded template in line with the checklist files. Rows are paired by question in order
        /// (the same question can appear in several sections), so answers keep their question. A row someone edited
        /// keeps its values; a template someone edited keeps its question list and order.
        /// </summary>
        private static (int Changed, int Added) SyncTemplate(AuditTemplate template, SeedTemplate seed, Dictionary<int, long> questions)
        {
            var edited = template.ModifiedBy != null;
            var rows = template.AuditTemplateItems.OrderBy(i => i.Sequence).ThenBy(i => i.Id).ToList();
            var changed = 0;
            var added = 0;

            foreach (var item in seed.Items.OrderBy(i => i.Sequence))
            {
                if (!questions.TryGetValue(item.Question, out var questionId))
                    continue;

                var section = string.IsNullOrWhiteSpace(item.Section) ? null : item.Section;
                var row = rows.FirstOrDefault(i => i.QuestionBankId == questionId);
                if (row == null)
                {
                    if (edited)
                        continue;
                    template.AuditTemplateItems.Add(new AuditTemplateItem
                    {
                        QuestionBankId = questionId,
                        Sequence = item.Sequence,
                        Mandatory = item.Mandatory,
                        Section = section,
                        Responsibility = item.Responsibility,
                        ServiceProvider = item.Provider,
                    });
                    added++;
                    continue;
                }

                rows.Remove(row);
                if (row.ModifiedBy != null)
                    continue;

                var sequence = edited ? row.Sequence : item.Sequence;
                if (row.Section != section || row.Responsibility != item.Responsibility || row.ServiceProvider != item.Provider || row.Sequence != sequence)
                {
                    row.Section = section;
                    row.Responsibility = item.Responsibility;
                    row.ServiceProvider = item.Provider;
                    row.Sequence = sequence;
                    changed++;
                }
            }

            return (changed, added);
        }

        private async Task SeedSitesAsync(SeedFile seed, Dictionary<string, Airport> airports, Dictionary<string, AuditTemplate> templates, long adminId)
        {
            var departments = await _context.Departments.ToDictionaryAsync(d => d.Name, d => d.Id, StringComparer.OrdinalIgnoreCase);
            var existing = await _context.AuditSites.Include(s => s.Templates).ToListAsync();
            var added = 0;

            foreach (var s in seed.Sites)
            {
                if (!airports.TryGetValue(s.Airport, out var airport))
                    continue;

                var site = existing.FirstOrDefault(e => e.AirportId == airport.Id && e.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase));
                if (site == null)
                {
                    var terminal = s.Terminal == null
                        ? null
                        : airport.Locations.FirstOrDefault(l => l.AreaType == AirportLookups.Terminal && l.Name == s.Terminal);

                    site = new AuditSite
                    {
                        AirportId = airport.Id,
                        Name = s.Name,
                        SiteCategory = s.SiteCategory,
                        SiteSubCategory = s.SiteSubCategory,
                        SiteType = s.SiteType,
                        TerminalId = terminal?.Id,
                        DepartmentId = departments.TryGetValue(s.Department, out var dept) ? dept : departments.Values.First(),
                        Status = AuditSiteLookups.Active,
                        CreatedBy = adminId,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _context.AuditSites.Add(site);
                    existing.Add(site);
                    added++;

                    // Only link templates on sites we create; links on existing sites are the users' to manage.
                    foreach (var name in s.Templates)
                        if (templates.TryGetValue(name, out var template))
                            site.Templates.Add(template);
                }

                site.ServiceProvider ??= s.ServiceProvider;
            }

            await _context.SaveChangesAsync();
            if (added > 0)
                _logger.LogInformation("Seeded {Count} audit site(s)", added);
        }

        // ------------------------------------------------------------------ seed file shape

        private sealed class SeedFile
        {
            public List<SeedCategory> Categories { get; set; } = new();
            public List<SeedAirport> Airports { get; set; } = new();
            public List<SeedReferenceList> ReferenceLists { get; set; } = new();
            public List<SeedQuestion> Questions { get; set; } = new();
            public List<SeedTemplate> Templates { get; set; } = new();
            public List<SeedSite> Sites { get; set; } = new();
        }

        private sealed record SeedCategory(string Name, string? Description);

        private sealed class SeedReferenceList
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public List<SeedReferenceItem> Items { get; set; } = new();
        }

        private sealed record SeedReferenceItem(string Name, string? Code, string? Group, int Order);

        private sealed class SeedAirport
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? AirportType { get; set; }
            public string? Region { get; set; }
            public string? Country { get; set; }
            public string? AirportManager { get; set; }
            public string? EmailAddress { get; set; }
            public string? TelNumber { get; set; }
            public Dictionary<string, List<string>> Locations { get; set; } = new();
        }

        private sealed class SeedQuestion
        {
            public string Text { get; set; } = "";
            public string Type { get; set; } = "YES_NO";
            public string Category { get; set; } = "";
            public string? Recommendation { get; set; }
            public string? Contractor { get; set; }
            public string? List { get; set; }
            public List<string> Reasons { get; set; } = new();
            public bool AllowNotApplicable { get; set; }
        }

        private sealed class SeedTemplate
        {
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public List<SeedTemplateItem> Items { get; set; } = new();
        }

        private sealed record SeedTemplateItem(int Question, string? Section, bool Mandatory, int Sequence, string? Responsibility = null, string? Provider = null);

        private sealed class SeedSite
        {
            public string Airport { get; set; } = "";
            public string Name { get; set; } = "";
            public string? SiteCategory { get; set; }
            public string? SiteSubCategory { get; set; }
            public string? SiteType { get; set; }
            public string? Terminal { get; set; }
            public string? ServiceProvider { get; set; }
            public string Department { get; set; } = "Operations";
            public List<string> Templates { get; set; } = new();
        }
    }
}
