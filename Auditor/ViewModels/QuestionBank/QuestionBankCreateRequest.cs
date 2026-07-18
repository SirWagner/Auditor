namespace Auditor.ViewModels.QuestionBank
{
    public class QuestionBankCreateRequest
    {
        public string QuestionText { get; set; }

        public int QuestionTypeId { get; set; }


        public List<QuestionOptionViewModel> Options { get; set; }
            = new();
    }
}
