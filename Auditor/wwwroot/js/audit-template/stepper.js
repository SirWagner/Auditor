class Stepper {
    constructor(questionManager) {
        this.questionManager = questionManager;

        this.state = {
            currentStep: 1
        };

        this.dom = this.initializeDomReferences();

        this.initializeEventListeners();
    }

    /* -----------------------------
     * DOM REFERENCES
     * ----------------------------- */

    initializeDomReferences() {
        return {
            nextButtons: document.querySelectorAll(".next-step"),
            previousButtons: document.querySelectorAll(".prev-step"),

            templateNameInput: document.querySelector('[name="Name"]'),
            templateVersionInput: document.querySelector('[name="Version"]'),

            summaryContainer: document.getElementById("review-summary")
        };
    }

    /* -----------------------------
     * EVENTS
     * ----------------------------- */

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

    /* -----------------------------
     * NAVIGATION
     * ----------------------------- */

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

    /* -----------------------------
     * STEP VISIBILITY
     * ----------------------------- */

    showStep(stepNumber) {
        document
            .getElementById(`step-${stepNumber}`)
            ?.classList.remove("d-none");
    }

    hideStep(stepNumber) {
        document
            .getElementById(`step-${stepNumber}`)
            ?.classList.add("d-none");
    }

    activateStepIndicator(stepNumber) {
        document
            .getElementById(`step-${stepNumber}-indicator`)
            ?.classList.add("active");
    }

    deactivateStepIndicator(stepNumber) {
        document
            .getElementById(`step-${stepNumber}-indicator`)
            ?.classList.remove("active");
    }

    /* -----------------------------
     * VALIDATION (SRP: separate logic)
     * ----------------------------- */

    validateCurrentStep() {
        const step = this.state.currentStep;

        if (step === 1) {
            return this.validateTemplateDetails();
        }

        if (step === 2) {
            return this.validateQuestions();
        }

        return true;
    }

    validateTemplateDetails() {
        const templateName = this.dom.templateNameInput?.value;

        if (!templateName) {
            alert("Template name is required");
            return false;
        }

        return true;
    }

    validateQuestions() {
        const hasQuestions =
            this.questionManager?.state?.addedQuestionIds?.size > 0;

        if (!hasQuestions) {
            alert("Please add at least one question");
            return false;
        }

        return true;
    }

    /* -----------------------------
     * SUMMARY (PURE UI RENDERING)
     * ----------------------------- */

    buildSummary() {
        if (!this.dom.summaryContainer) return;

        const templateName = this.dom.templateNameInput?.value;
        const templateVersion = this.dom.templateVersionInput?.value;
        const questionCount = this.getQuestionCount();

        this.dom.summaryContainer.innerHTML = `
            <div class="col-md-6">
                <strong>Name:</strong>
                <p>${templateName}</p>
            </div>

            <div class="col-md-6">
                <strong>Version:</strong>
                <p>v${templateVersion}</p>
            </div>

            <div class="col-md-12">
                <strong>${questionCount} Questions</strong>
            </div>
        `;
    }

    /* -----------------------------
     * HELPERS
     * ----------------------------- */

    isQuestionStep() {
        return this.state.currentStep === 2;
    }

    getQuestionCount() {
        return this.questionManager?.state?.addedQuestionIds?.size || 0;
    }
}

window.Stepper = Stepper;