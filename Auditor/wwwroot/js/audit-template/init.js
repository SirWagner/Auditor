document.addEventListener("DOMContentLoaded", () => {
    const data = window.__AUDIT_TEMPLATE_DATA__ || {};

    const qm = new QuestionManager({
        questionBank: data.questionBank
    });

    new Stepper(qm);
});