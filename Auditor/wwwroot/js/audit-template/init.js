import QuestionManager from "./question-manager.js";
import Stepper from "./stepper.js";

document.addEventListener("DOMContentLoaded", () => {

    const auditTemplateData = window.__AUDIT_TEMPLATE_DATA__;

    const questionManager = new QuestionManager({
        questionBank: auditTemplateData.questionBank,
        existingQuestions: auditTemplateData.existingQuestions || [],
        responsibilities: auditTemplateData.responsibilities || [],
        serviceProviders: auditTemplateData.serviceProviders || []
    });

    new Stepper(questionManager);
});
