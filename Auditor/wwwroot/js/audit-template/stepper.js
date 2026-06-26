export default class Stepper {
    constructor(questionManager) {
        this.questionManager = questionManager;

        this.state = {
            currentStep: 1
        };

        this.dom = this.initializeDomReferences();

        this.initializeEventListeners();
    }

    initializeDomReferences() {
        return {
            nextButtons: document.querySelectorAll(".next-step"),
            previousButtons: document.querySelectorAll(".prev-step"),

            templateNameInput: document.querySelector('[name="Name"]'),
            templateVersionInput: document.querySelector('[name="Version"]'),

            summaryContainer: document.getElementById("review-summary")
        };
    }

    initializeEventListeners() {
        this.bindNextStepEvents();
        this.bindPreviousStepEvents();
    }

    bindNextStepEvents() {
        this.dom.nextButtons.forEach(button => {
            button.addEventListener("click", () => this.goToNextStep());
        });
    }

    bindPreviousStepEvents() {
        this.dom.previousButtons.forEach(button => {
            button.addEventListener("click", () => this.goToPreviousStep());
        });
    }

    goToNextStep() {
        if (!this.validateCurrentStep()) return;

        if (this.isQuestionStep()) {
            this.buildSummary();
        }

        this.goToStep(this.state.currentStep + 1);
    }

    goToPreviousStep() {
        this.goToStep(this.state.currentStep - 1);
    }

    goToStep(targetStep) {
        this.hideStep(this.state.currentStep);
        this.deactivateStepIndicator(this.state.currentStep);

        this.state.currentStep = targetStep;

        this.showStep(this.state.currentStep);
        this.activateStepIndicator(this.state.currentStep);
    }

    showStep(stepNumber) {
        document.getElementById(`step-${stepNumber}`)?.classList.remove("d-none");
    }

    hideStep(stepNumber) {
        document.getElementById(`step-${stepNumber}`)?.classList.add("d-none");
    }

    activateStepIndicator(stepNumber) {
        document.getElementById(`step-${stepNumber}-indicator`)?.classList.add("active");
    }

    deactivateStepIndicator(stepNumber) {
        document.getElementById(`step-${stepNumber}-indicator`)?.classList.remove("active");
    }

    validateCurrentStep() {
        if (this.state.currentStep === 1) {
            return this.validateTemplateDetails();
        }

        if (this.state.currentStep === 2) {
            return this.validateQuestions();
        }

        return true;
    }

    validateTemplateDetails() {
        const name = this.dom.templateNameInput?.value;

        if (!name) {
            alert("Template name is required");
            return false;
        }

        return true;
    }

    validateQuestions() {
        const count = this.questionManager.getQuestionCount();

        if (count === 0) {
            alert("Please add at least one question");
            return false;
        }

        return true;
    }

    buildSummary() {
        if (!this.dom.summaryContainer) return;

        const name = this.dom.templateNameInput?.value;
        const version = this.dom.templateVersionInput?.value;
        const questionCount = this.questionManager.getQuestionCount();

        this.dom.summaryContainer.innerHTML = `
            <div class="col-md-6">
                <strong>Name:</strong>
                <p>${name}</p>
            </div>

            <div class="col-md-6">
                <strong>Version:</strong>
                <p>v${version}</p>
            </div>

            <div class="col-md-12">
                <strong>${questionCount} Questions</strong>
            </div>
        `;
    }

    isQuestionStep() {
        return this.state.currentStep === 2;
    }
}

window.Stepper = Stepper;