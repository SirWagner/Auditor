namespace Auditor.ViewModels.QuestionBank
{
    public class QuestionBankCreateViewModel
    {
        public string QuestionText { get; set; }

        public int CategoryId { get; set; }

        public int QuestionTypeId { get; set; }

        public string ServiceStandardRecommendation { get; set; }

        public string ResponsibleContractor { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }


        public List<QuestionOptionViewModel> Options { get; set; }
            = new();
    }
}
