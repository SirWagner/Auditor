using Auditor.DTO.Questionbanks;

namespace Auditor.Services.Interfaces
{
    public interface IQuestionBankService
    {
        public Task<List<QuestionBankInfoDTO>> GetAll();
        Task Create(CreateQuestionBankDTO dto);
        Task<bool> ToggleActiveAsync(long id, long modifiedByUserId);


    }
}