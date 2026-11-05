using Auditor.DTO.AppUsers;
using Auditor.DTO.Questionbanks;
using Auditor.Models;
using Auditor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Services
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly AuditorContext _context;
        private readonly ILogger<QuestionBankService> _logger;

        public QuestionBankService(AuditorContext context,
                            ILogger<QuestionBankService> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<List<QuestionBankInfoDTO>> GetAll()
        {
            return _context.QuestionBanks
                .Include(item=>item.Category)
                .Include(item=>item.QuestionType)
                .Select(question => new QuestionBankInfoDTO(question.Id, question.QuestionText, question.Category.Name, question.QuestionType.Name, question.ResponsibleContractor)).ToList();
        }
        public async Task Create(CreateQuestionBankDTO dto)
        {
            var questionType = await _context.QuestionTypes
                .FirstOrDefaultAsync(x => x.Id == dto.QuestionTypeId);


            if (questionType == null)
            {
                throw new Exception(
                    "Invalid question type."
                );
            }


            var questionBank = new QuestionBank
            {
                QuestionText = dto.QuestionText,

                CategoryId = dto.CategoryId,

                QuestionTypeId = dto.QuestionTypeId,

                ServiceStandardRecommendation =
                    dto.ServiceStandardRecommendation,

                ResponsibleContractor =
                    dto.ResponsibleContractor,

                ReferenceListId = questionType.Name == "LIST" ? dto.ReferenceListId : null,

                IsActive = dto.IsActive,

                CreatedBy = dto.CreatedBy,

                CreatedDate = DateTime.UtcNow
            };


            if (questionType.Name == "CHECKLIST" || questionType.Name == "MULTIPLE_CHOICE")
            {
                questionBank.QuestionBankOptions =
                    dto.Options
                    .Where(item => !string.IsNullOrWhiteSpace(item.Text))
                    .Select(item => new QuestionBankOption
                    {
                        OptionText = item.Text.Trim(),
                        OptionValue = item.Text.Trim(),
                        DisplayOrder = item.Sequence
                    })
                    .ToList();
            }
            else if (questionType.Name == "YES_NO")
            {
                // Compliance questions: "Not compliant" asks the auditor for a reason.
                questionBank.QuestionBankOptions = Auditor.Data.DatabaseSeeder.ComplianceOptions(allowNotApplicable: true).ToList();
            }
            else if (questionType.Name == "BOOLEAN")
            {
                questionBank.QuestionBankOptions = new List<QuestionBankOption>
                {
                    new() { OptionText = "Yes", OptionValue = "true", DisplayOrder = 1 },
                    new() { OptionText = "No", OptionValue = "false", DisplayOrder = 2 },
                };
            }


            _context.QuestionBanks.Add(questionBank);

            await _context.SaveChangesAsync();
        }
        public async Task<bool> ToggleActiveAsync(long id, long modifiedByUserId)
        {
            var template = await _context.QuestionBanks.FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return false;

            template.IsActive = !template.IsActive;
            //template.Update = modifiedByUserId;
            //template.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


    }
}
