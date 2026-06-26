class QuestionManager {
    constructor({ questionBank }) {
        this.state = {
            nextQuestionIndex: 0,
            addedQuestionIds: new Set(),
            availableQuestions: questionBank || []
        };

        this.dom = this.initializeDomReferences();

        this.initializeEventListeners();
        this.renderQuestionBankList();
        this.updateUiState();
    }

    /* -----------------------------
     * DOM REFERENCES
     * ----------------------------- */

    initializeDomReferences() {
        return {
            questionContainer: document.getElementById("questionContainer"),
            questionBankModal: document.getElementById("questionBankModal"),
            questionSearchInput: document.getElementById("questionSearch"),
            questionBankListContainer: document.getElementById("questionBankList"),
            auditTemplateForm: document.getElementById("auditTemplateForm"),
            questionCounterLabel: document.getElementById("questionCount")
        };
    }

    /* -----------------------------
     * EVENTS
     * ----------------------------- */

    initializeEventListeners() {
        this.initializeModalEvents();
        this.initializeSearchEvents();
        this.initializeQuestionBankEvents();
        this.initializeContainerEvents();
        this.initializeFormEvents();
    }

    initializeModalEvents() {
        const modal = this.dom.questionBankModal;
        if (!modal) return;

        modal.addEventListener("shown.bs.modal", () => {
            this.clearSearchInput();
            this.renderQuestionBankList();
        });
    }

    initializeSearchEvents() {
        this.dom.questionSearchInput?.addEventListener("input", (event) => {
            this.filterQuestions(event.target.value);
        });
    }

    initializeQuestionBankEvents() {
        this.dom.questionBankListContainer?.addEventListener("click", (event) => {
            const questionElement = event.target.closest("[data-question-id]");
            if (!questionElement) return;

            this.addQuestionFromBank(questionElement);
        });
    }

    initializeContainerEvents() {
        this.dom.questionContainer?.addEventListener("click", (event) => {
            const deleteButton = event.target.closest("[data-action='deleteQuestion']");
            if (!deleteButton) return;

            const questionCard = deleteButton.closest(".card");
            this.removeQuestion(questionCard);
        });
    }

    initializeFormEvents() {
        this.dom.auditTemplateForm?.addEventListener("submit", (event) => {
            if (!this.hasAnyQuestions()) {
                event.preventDefault();
                alert("Please add at least one question");
            }
        });
    }

    /* -----------------------------
     * QUESTION BANK LOGIC
     * ----------------------------- */

    filterQuestions(searchTerm) {
        const normalizedTerm = searchTerm.toLowerCase();

        const filteredQuestions = this.state.availableQuestions.filter(question =>
            question.text.toLowerCase().includes(normalizedTerm)
        );

        this.renderQuestionBankList(filteredQuestions);
    }

    renderQuestionBankList(questions = this.state.availableQuestions) {
        const container = this.dom.questionBankListContainer;
        if (!container) return;

        container.innerHTML = questions
            .map(question => this.renderQuestionBankItem(question))
            .join("");
    }

    renderQuestionBankItem(question) {
        const isAlreadyAdded = this.state.addedQuestionIds.has(question.id);

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
     * ADD / REMOVE QUESTIONS
     * ----------------------------- */

    addQuestionFromBank(questionElement) {
        const { questionId, questionText, questionType } = questionElement.dataset;

        if (this.state.addedQuestionIds.has(questionId)) {
            this.showNotification("Question already added", "warning");
            return;
        }

        this.state.addedQuestionIds.add(questionId);

        this.renderQuestionCard(questionId, questionText, questionType);

        this.incrementQuestionIndex();
        this.updateUiState();
        this.closeModal();
    }

    removeQuestion(questionCard) {
        if (!questionCard || !confirm("Remove this question?")) return;

        const questionId = questionCard.dataset.questionId;
        this.state.addedQuestionIds.delete(questionId);

        questionCard.remove();

        this.recalculateQuestionIndexes();
        this.updateUiState();
    }

    /* -----------------------------
     * QUESTION CARD RENDERING
     * ----------------------------- */

    renderQuestionCard(questionId, questionText, questionType) {
        const index = this.state.nextQuestionIndex;

        const cardElement = document.createElement("div");
        cardElement.className = "card mb-3 shadow-sm";
        cardElement.dataset.questionId = questionId;

        cardElement.innerHTML = this.buildQuestionCardHtml({
            questionId,
            questionText,
            questionType,
            index
        });

        this.dom.questionContainer?.appendChild(cardElement);
    }

    buildQuestionCardHtml({ questionId, questionText, questionType, index }) {
        return `
            <div class="card-body">

                <div class="d-flex justify-content-between">
                    <div>
                        <div class="fw-semibold">
                            ${this.escapeHtml(questionText)}
                        </div>

                        <span class="badge bg-light text-primary">
                            ${this.escapeHtml(questionType)}
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
                       value="${questionType}" />

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
     * INDEX MANAGEMENT
     * ----------------------------- */

    incrementQuestionIndex() {
        this.state.nextQuestionIndex++;
    }

    recalculateQuestionIndexes() {
        const questionCards = this.dom.questionContainer?.querySelectorAll(".card") || [];

        questionCards.forEach((card, index) => {
            card.querySelectorAll("input").forEach(input => {
                if (input.name.includes("QuestionBankId")) {
                    input.name = `Items[${index}].QuestionBankId`;
                }

                if (input.name.includes("Mandatory")) {
                    input.name = `Items[${index}].Mandatory`;
                }

                if (input.name.includes("Sequence")) {
                    input.name = `Items[${index}].Sequence`;
                    input.value = index + 1;
                }
            });
        });

        this.state.nextQuestionIndex = questionCards.length;
    }

    /* -----------------------------
     * UI STATE
     * ----------------------------- */

    updateUiState() {
        const totalQuestions = this.state.addedQuestionIds.size;

        if (this.dom.questionCounterLabel) {
            this.dom.questionCounterLabel.textContent = totalQuestions;
        }

        this.dom.questionContainer?.classList.toggle(
            "has-items",
            totalQuestions > 0
        );
    }

    /* -----------------------------
     * HELPERS
     * ----------------------------- */

    hasAnyQuestions() {
        return this.state.nextQuestionIndex > 0;
    }

    clearSearchInput() {
        if (this.dom.questionSearchInput) {
            this.dom.questionSearchInput.value = "";
        }
    }

    closeModal() {
        try {
            bootstrap.Modal
                .getOrCreateInstance(this.dom.questionBankModal)
                ?.hide();
        } catch {
            // intentionally ignored
        }
    }

    showNotification(message, type = "info") {
        console.log(`[${type}] ${message}`);
    }

    escapeHtml(text) {
        const element = document.createElement("div");
        element.textContent = text;
        return element.innerHTML;
    }
}

window.QuestionManager = QuestionManager;