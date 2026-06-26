class Stepper {
    constructor(questionManager) {
        this.currentStep = 1;
        this.qm = questionManager;

        this.bindEvents();
    }

    bindEvents() {
        document.querySelectorAll(".next-step").forEach(btn => {
            btn.addEventListener("click", () => this.next());
        });

        document.querySelectorAll(".prev-step").forEach(btn => {
            btn.addEventListener("click", () => this.prev());
        });
    }

    next() {
        if (!this.validate(this.currentStep)) return;

        if (this.currentStep === 2) this.buildSummary();

        this.change(this.currentStep + 1);
    }

    prev() {
        this.change(this.currentStep - 1);
    }

    change(step) {
        document.getElementById(`step-${this.currentStep}`)?.classList.add("d-none");
        document.getElementById(`step-${this.currentStep}-indicator`)?.classList.remove("active");

        this.currentStep = step;

        document.getElementById(`step-${this.currentStep}`)?.classList.remove("d-none");
        document.getElementById(`step-${this.currentStep}-indicator`)?.classList.add("active");
    }

    validate(step) {
        if (step === 1) {
            const name = document.querySelector('[name="Name"]')?.value;
            if (!name) {
                alert("Template name required");
                return false;
            }
        }

        if (step === 2 && this.qm.state.addedQuestions.size === 0) {
            alert("Add at least one question");
            return false;
        }

        return true;
    }

    buildSummary() {
        const name = document.querySelector('[name="Name"]')?.value;
        const version = document.querySelector('[name="Version"]')?.value;
        const count = this.qm.state.addedQuestions.size;

        document.getElementById("review-summary").innerHTML = `
            <div class="col-md-6"><strong>Name:</strong><p>${name}</p></div>
            <div class="col-md-6"><strong>Version:</strong><p>v${version}</p></div>
            <div class="col-md-12"><strong>${count} Questions</strong></div>
        `;
    }
}

window.Stepper = Stepper;