using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateEditViewModel : AuditTemplateFormViewModel
    {
        public override string FormAction => "Edit";

        public override string SubmitText =>
            "Save Changes";

    }


}