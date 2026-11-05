using System.Text.RegularExpressions;

namespace Auditor.ViewModels.Shared
{
    /// <summary>
    /// Turns the "Main › Section › Sub-location" paths stored on template questions into what the screens show:
    /// one tab per main section, and inside each tab the sections and sub-locations as groups with a navigation tree.
    /// Questions keep their template order inside a group; groups follow the order they first appear in.
    /// </summary>
    public static class SectionTree
    {
        public const string Separator = " › ";
        public const int MaxLength = 500;

        /// <summary>Cleans a section typed by a user or imported: accepts "›", ">" or "/" between levels.</summary>
        public static string? Normalize(string? section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return null;

            var parts = Regex.Split(section, @"\s*(?:›|>|\s/\s)\s*")
                .Select(p => Regex.Replace(p, @"\s+", " ").Trim())
                .Where(p => p.Length > 0)
                .ToList();
            return parts.Count == 0 ? null : string.Join(Separator, parts);
        }

        /// <summary>The levels of a stored path. " — note" suffixes from older imports are dropped from the level name.</summary>
        public static List<string> Segments(string? section) =>
            string.IsNullOrWhiteSpace(section)
                ? new List<string>()
                : section.Split('›')
                    .Select(s => Regex.Replace(s.Split(" — ")[0], @"\s+", " ").Trim())
                    .Where(s => s.Length > 0)
                    .ToList();

        public static SectionLayout<T> Build<T>(IReadOnlyList<T> items, Func<T, string?> sectionOf, string generalTitle = "General")
        {
            var paths = items.Select(i => Segments(sectionOf(i))).ToList();

            // A level every question shares ("Arrival" on a one-stage checklist) says nothing: tab on the level below it.
            while (paths.Count > 0 && paths.All(p => p.Count >= 2) && paths.Select(p => p[0]).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1)
                paths.ForEach(p => p.RemoveAt(0));

            var root = new Node<T>("", 0);
            for (var i = 0; i < items.Count; i++)
            {
                var node = root;
                foreach (var segment in paths[i])
                {
                    var child = node.Children.FirstOrDefault(c => c.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
                    if (child == null)
                        node.Children.Add(child = new Node<T>(segment, i));
                    node = child;
                }
                node.Items.Add(items[i]);
            }

            var tops = root.Children.ToList();
            if (root.Items.Count > 0)
            {
                var general = new Node<T>(generalTitle, paths.FindIndex(p => p.Count == 0));
                general.Items.AddRange(root.Items);
                tops.Add(general);
            }
            tops = tops.OrderBy(t => t.FirstIndex).ToList();

            var layout = new SectionLayout<T>();
            var groupNo = 0;
            for (var t = 0; t < tops.Count; t++)
            {
                var tab = new SectionTab<T> { Id = $"s{t + 1}", Title = tops[t].Name };
                Walk(tops[t], new List<string>(), tab, ref groupNo);
                layout.Tabs.Add(tab);
            }
            return layout;
        }

        private static void Walk<T>(Node<T> node, List<string> path, SectionTab<T> tab, ref int groupNo)
        {
            if (node.Items.Count > 0)
            {
                var (label, units) = Label(path.Count == 0 ? "" : path[^1]);
                var group = new SectionGroup<T>
                {
                    Id = $"g{++groupNo}",
                    Path = path.Take(path.Count - 1).Select(p => Label(p).Label).Append(label).Where(p => p.Length > 0).ToList(),
                    Units = units,
                };
                group.Items.AddRange(node.Items);
                tab.Groups.Add(group);
                if (path.Count > 0 || node.Children.Count > 0)
                    tab.Nav.Add(new SectionNavEntry(path.Count == 0 ? "General" : label, Math.Max(0, path.Count - 1), group.Id, group.Items.Count));
            }
            else if (path.Count > 0)
            {
                tab.Nav.Add(new SectionNavEntry(Label(path[^1]).Label, path.Count - 1, null, 0));
            }

            foreach (var child in node.Children)
                Walk(child, path.Append(child.Name).ToList(), tab, ref groupNo);
        }

        /// <summary>"• E38 lift • E39 lift" names list the units to check: show a short label and the units separately.</summary>
        private static (string Label, List<string> Units) Label(string segment)
        {
            if (!segment.Contains('•'))
                return (segment, new List<string>());
            var units = segment.Split('•').Select(u => u.Trim()).Where(u => u.Length > 0).ToList();
            return units.Count switch
            {
                0 => ("Units", units),
                1 => (units[0], new List<string>()),
                _ => ($"{units.Count} units", units),
            };
        }

        private sealed class Node<T>
        {
            public Node(string name, int firstIndex) { Name = name; FirstIndex = firstIndex; }
            public string Name { get; }
            public int FirstIndex { get; }
            public List<Node<T>> Children { get; } = new();
            public List<T> Items { get; } = new();
        }
    }

    public sealed class SectionLayout<T>
    {
        public List<SectionTab<T>> Tabs { get; } = new();

        /// <summary>Tabs are only worth showing when there's more than one.</summary>
        public bool HasTabs => Tabs.Count > 1;

        /// <summary>Every item in display order: tab by tab, group by group.</summary>
        public IEnumerable<T> Ordered => Tabs.SelectMany(t => t.Groups).SelectMany(g => g.Items);
    }

    public sealed class SectionTab<T>
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public List<SectionGroup<T>> Groups { get; } = new();
        public List<SectionNavEntry> Nav { get; } = new();
        public int Count => Groups.Sum(g => g.Items.Count);
    }

    public sealed class SectionGroup<T>
    {
        public string Id { get; init; } = "";

        /// <summary>The levels below the tab, e.g. ["Arrival Bus Gates", "Lifts"]. Empty for questions directly on the tab.</summary>
        public List<string> Path { get; init; } = new();

        /// <summary>Units listed in the sub-location name (lift numbers, door codes) to show as chips.</summary>
        public List<string> Units { get; init; } = new();

        public List<T> Items { get; } = new();
    }

    /// <summary>Model for the shared _SectionNav partial.</summary>
    public sealed record SectionNavPanel(string TabId, string Title, List<SectionNavEntry> Nav);

    /// <summary>One line of the in-tab navigation tree. GroupId is null for a heading that only holds deeper levels.</summary>
    public sealed record SectionNavEntry(string Label, int Depth, string? GroupId, int Count);
}
