namespace Auditor.DTO.Questionbanks
{
    public class CreateQuestionBankDTO
    {
        public string QuestionText { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int QuestionTypeId { get; set; }

        public string? ServiceStandardRecommendation { get; set; }

        public string? ResponsibleContractor { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }


        public List<CreateQuestionOptionDTO> Options { get; set; }
            = new();
    }
    public class CreateQuestionOptionDTO
    {
        public string Text { get; set; } = string.Empty;

        public int Sequence { get; set; }
    }
}
