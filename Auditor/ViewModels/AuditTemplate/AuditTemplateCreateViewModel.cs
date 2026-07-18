using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Auditor.ViewModels.AuditTemplate
{
    public class AuditTemplateCreateViewModel : AuditTemplateFormViewModel
    {
        public override string FormAction => "Create";

        public override string SubmitText =>
            "Confirm & Save Template";
    }

}