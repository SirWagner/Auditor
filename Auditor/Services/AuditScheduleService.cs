using Auditor.DTO.AuditSchedule;
using Auditor.Models;
using Auditor.Services.Interfaces;

namespace Auditor.Services
{
    public class AuditScheduleService : IAuditScheduleService
    {
        private readonly AuditorContext _context;
        private readonly ILogger<AuditScheduleService> _logger;
        public AuditScheduleService(AuditorContext context, ILogger<AuditScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<AuditSchedule> Create(AuditScheduleCreateDTO auditScheduleCreateDTO)
        {
            ArgumentNullException.ThrowIfNull(auditScheduleCreateDTO);

            _logger.LogInformation("Creating a new audit schedule.");

            try
            {
                var auditSchedule = new AuditSchedule
                {
                    SchedulerId = auditScheduleCreateDTO.SchedulerId,
                    SiteId = auditScheduleCreateDTO.SiteId,
                    TemplateId = auditScheduleCreateDTO.TemplateId,
                    ScheduledDate = auditScheduleCreateDTO.ScheduledDate,
                    DueDate = auditScheduleCreateDTO.DueDate,
                    Status = "SCHEDULED"
                };

                var result = await _context.AuditSchedules.AddAsync(auditSchedule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Audit schedule created successfully.");

                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an audit schedule.");
                throw;
            }
        }
    }
}
