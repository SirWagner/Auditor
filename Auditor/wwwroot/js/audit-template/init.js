import QuestionManager from "./question-manager.js";
import Stepper from "./stepper.js";

document.addEventListener("DOMContentLoaded", () => {

    const auditTemplateData = window.__AUDIT_TEMPLATE_DATA__;
    console.log(auditTemplateData);

    const questionManager = new QuestionManager({
        questionBank: auditTemplateData.questionBank,
        addedQuestionsId: new Set(auditTemplateData.addedQuestionsIds || [])
    });

    new Stepper(questionManager);
});