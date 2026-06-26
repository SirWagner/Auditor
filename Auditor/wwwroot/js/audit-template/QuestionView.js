export default class QuestionView {
    constructor() {
        this.dom = this.initializeDomReferences();
    }

    /* -----------------------------
     * DOM REFERENCES
     * ----------------------------- */

    initializeDomReferences() {
        return {
            questionContainer: document.getElementById("questionContainer"),
            questionBankListContainer: document.getElementById("questionBankList"),
            questionSearchInput: document.getElementById("questionSearch"),
            questionCounterLabel: document.getElementById("questionCount"),
            questionBankModal: document.getElementById("questionBankModal")
        };
    }

    /* -----------------------------
     * QUESTION BANK LIST RENDERING
     * ----------------------------- */

    renderQuestionBankList(questions, isAddedFn) {
        const container = this.dom.questionBankListContainer;
        if (!container) return;

        container.innerHTML = questions
            .map(question => this.renderQuestionBankItem(question, isAddedFn))
            .join("");
    }

    renderQuestionBankItem(question, isAddedFn) {
        const isAlreadyAdded = isAddedFn(question.id);

        return `
            <div class="question-bank-item ${isAlreadyAdded ? "disabled" : ""}"
                 data-question-id="${question.id}"
                 data-question-text="${this.escapeHtml(question.text)}"
                 data-question-type="${this.escapeHtml(question.questionType)}">

                <div class="fw-semibold">
                    ${this.escapeHtml(question.text)}
                </div>

                <div class="text-muted small">
                    ${this.escapeHtml(question.questionType)}
                </div>
            </div>
        `;
    }

    /* -----------------------------
     * QUESTION CARD RENDERING
     * ----------------------------- */

    renderQuestionCard(questionId, text, type, index) {
        const container = this.dom.questionContainer;
        if (!container) return;

        const card = document.createElement("div");

        card.className = "card mb-3 shadow-sm";
        card.dataset.questionId = questionId;

        card.innerHTML = this.buildQuestionCardHtml({
            questionId,
            text,
            type,
            index
        });

        container.appendChild(card);
    }

    buildQuestionCardHtml({ questionId, text, type, index }) {
        return `
            <div class="card-body">

                <div class="d-flex justify-content-between">
                    <div>
                        <div class="fw-semibold">
                            ${this.escapeHtml(text)}
                        </div>

                        <span class="badge bg-light text-primary">
                            ${this.escapeHtml(type)}
                        </span>
                    </div>

                    <button class="btn btn-sm btn-outline-danger"
                            data-action="deleteQuestion">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>

                <input type="hidden"
                       name="Items[${index}].QuestionBankId"
                       value="${questionId}" />

                <input type="hidden"
                       name="Items[${index}].QuestionType"
                       value="${type}" />

                <div class="row mt-3">

                    <div class="col-md-6">
                        <input type="checkbox"
                               name="Items[${index}].Mandatory" />
                        <label>Mandatory</label>
                    </div>

                    <div class="col-md-6">
                        <input type="number"
                               name="Items[${index}].Sequence"
                               value="${index + 1}" />
                    </div>

                </div>

            </div>
        `;
    }

    /* -----------------------------
     * UI STATE UPDATES
     * ----------------------------- */

    updateCounter(count) {
        if (this.dom.questionCounterLabel) {
            this.dom.questionCounterLabel.textContent = count;
        }
    }

    toggleContainerState(hasItems) {
        this.dom.questionContainer?.classList.toggle("has-items", hasItems);
    }

    clearSearch() {
        if (this.dom.questionSearchInput) {
            this.dom.questionSearchInput.value = "";
        }
    }

    removeQuestionCard(cardElement) {
        cardElement?.remove();
    }

    clearQuestionContainer() {
        if (this.dom.questionContainer) {
            this.dom.questionContainer.innerHTML = "";
        }
    }

    /* -----------------------------
     * MODAL CONTROL (UI ONLY)
     * ----------------------------- */

    closeModal() {
        try {
            const modalElement = this.dom.questionBankModal;

            if (!modalElement || !window.bootstrap?.Modal) return;

            const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
            modal.hide();
        } catch (error) {
            console.warn("Failed to close modal:", error);
        }
    }

    /* -----------------------------
     * HELPERS
     * ----------------------------- */

    escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
}