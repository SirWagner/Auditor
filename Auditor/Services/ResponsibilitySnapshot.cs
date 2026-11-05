using Auditor.Models;
using Auditor.Models.Lookups;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    /// <summary>
    /// Records on each answer who was responsible when the audit was submitted, so reports don't change when a
    /// template or a site's contractor changes later. The service provider comes from, in order: the company the
    /// checklist names on the question; the ground handler or airline recorded on the flight; the outlet for
    /// concession checks; the site's contractor for cleaning.
    /// </summary>
    public class ResponsibilitySnapshot
    {
        private readonly AuditorContext _context;

        public ResponsibilitySnapshot(AuditorContext context)
        {
            _context = context;
        }

        /// <summary>Sets the responsibility and provider on the execution's answers. The caller saves.</summary>
        public async Task ApplyAsync(long executionId)
        {
            var site = await _context.AuditExecutions
                .Where(e => e.Id == executionId)
                .Select(e => new { e.Schedule.Site.Name, e.Schedule.Site.ServiceProvider })
                .FirstAsync();

            var responses = await _context.AuditResponses
                .Include(r => r.TemplateItem).ThenInclude(t => t.QuestionBank).ThenInclude(q => q.ReferenceList)
                .Where(r => r.ExecutionId == executionId)
                .ToListAsync();

            string? AnswerFromList(string code) => responses
                .FirstOrDefault(r => r.TemplateItem.QuestionBank.ReferenceList?.Code == code && !string.IsNullOrWhiteSpace(r.AnswerValue))
                ?.AnswerValue;

            var handler = AnswerFromList(ReferenceLists.GroundHandlers);
            var airline = AnswerFromList(ReferenceLists.Airlines);

            foreach (var response in responses)
            {
                var item = response.TemplateItem;
                var responsibility = string.IsNullOrWhiteSpace(item.Responsibility) ? Responsibilities.NotAssigned : item.Responsibility;

                response.Responsibility = responsibility;
                response.ServiceProvider = !string.IsNullOrWhiteSpace(item.ServiceProvider)
                    ? item.ServiceProvider
                    : responsibility switch
                    {
                        Responsibilities.RampHandling => handler,
                        Responsibilities.Airline => airline,
                        Responsibilities.Concessionaire => site.Name,
                        Responsibilities.Cleaning => site.ServiceProvider,
                        _ => null,
                    };
            }
        }
    }
}
