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
                .Select(question => new QuestionBankInfoDTO(question.Id, question.QuestionText, question.Category.Name, question.QuestionType.Name)).ToList();
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

                IsActive = dto.IsActive,

                CreatedBy = dto.CreatedBy,

                CreatedDate = DateTime.UtcNow
            };


            if (questionType.Name == "CHECKLIST")
            {
                questionBank.QuestionBankOptions =
                    dto.ChecklistItems
                    .Select(item => new QuestionBankOption
                    {
                        OptionText = item.Text,

                        DisplayOrder = item.Sequence

                    })
                    .ToList();
            }


            _context.QuestionBanks.Add(questionBank);

            await _context.SaveChangesAsync();
        }


    }
}
