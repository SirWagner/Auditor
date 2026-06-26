import QuestionStore from "./QuestionStore.js";
import QuestionView from "./QuestionView.js";
import QuestionService from "./QuestionService.js";

export default class QuestionManager {
    constructor({ questionBank }) {
        this.store = new QuestionStore(questionBank);
        this.view = new QuestionView();
        this.service = new QuestionService(this.store);

        this.initialize();
    }

    /* -----------------------------
     * INITIALIZATION
     * ----------------------------- */

    initialize() {
        this.bindEvents();
        this.renderInitialState();
    }

    renderInitialState() {
        this.view.renderQuestionBankList(
            this.store.getAvailableQuestions(),
            (id) => this.store.hasQuestion(id)
        );

        this.view.updateCounter(this.store.getQuestionCount());
        this.view.toggleContainerState(this.store.getQuestionCount() > 0);
    }

    /* -----------------------------
     * EVENTS
     * ----------------------------- */

    bindEvents() {
        this.bindSearchEvent();
        this.bindAddQuestionEvent();
        this.bindDeleteQuestionEvent();
        this.bindModalEvent();
        this.bindFormValidation();
    }

    /* -----------------------------
     * SEARCH
     * ----------------------------- */

    bindSearchEvent() {
        this.view.dom.questionSearchInput?.addEventListener("input", (event) => {
            const filtered = this.service.filterQuestions(event.target.value);

            this.view.renderQuestionBankList(
                filtered,
                (id) => this.store.hasQuestion(id)
            );
        });
    }

    /* -----------------------------
     * ADD QUESTION FLOW
     * ----------------------------- */

    bindAddQuestionEvent() {
        this.view.dom.questionBankListContainer?.addEventListener("click", (event) => {
            const element = event.target.closest("[data-question-id]");
            if (!element) return;

            const { questionId, questionText, questionType } = element.dataset;

            if (!this.service.canAddQuestion(questionId)) return;

            this.handleAddQuestion(questionId, questionText, questionType);
        });
    }

    handleAddQuestion(questionId, questionText, questionType) {
        const index = this.store.getNextQuestionIndex();

        this.store.addQuestion(questionId);

        this.view.renderQuestionCard(
            questionId,
            questionText,
            questionType,
            index
        );

        this.syncUi();
        this.view.closeModal();
    }

    /* -----------------------------
     * DELETE QUESTION FLOW
     * ----------------------------- */

    bindDeleteQuestionEvent() {
        this.view.dom.questionContainer?.addEventListener("click", (event) => {
            const deleteButton = event.target.closest("[data-action='deleteQuestion']");
            if (!deleteButton) return;

            const card = deleteButton.closest(".card");
            this.handleRemoveQuestion(card);
        });
    }

    handleRemoveQuestion(cardElement) {
        if (!cardElement || !confirm("Remove this question?")) return;

        const questionId = cardElement.dataset.questionId;

        this.store.removeQuestion(questionId);

        this.view.removeQuestionCard(cardElement);

        this.rebuildIndexes();

        this.syncUi();
    }

    /* -----------------------------
     * INDEX REBUILD (SAFE SYNC)
     * ----------------------------- */

    rebuildIndexes() {
        const cards = this.view.dom.questionContainer?.querySelectorAll(".card") || [];

        this.store.resetIndex(cards.length);

        cards.forEach((card, index) => {
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
    }

    /* -----------------------------
     * MODAL
     * ----------------------------- */

    bindModalEvent() {
        this.view.dom.questionBankModal?.addEventListener("shown.bs.modal", () => {
            this.view.clearSearch();
        });
    }

    /* -----------------------------
     * FORM VALIDATION
     * ----------------------------- */

    bindFormValidation() {
        const form = this.view.dom.auditTemplateForm;

        form?.addEventListener("submit", (event) => {
            if (this.store.getQuestionCount() === 0) {
                event.preventDefault();
                alert("Please add at least one question");
            }
        });
    }

    /* -----------------------------
     * UI SYNC (ONLY ONE SOURCE OF TRUTH)
     * ----------------------------- */

    syncUi() {
        const count = this.store.getQuestionCount();

        this.view.updateCounter(count);
        this.view.toggleContainerState(count > 0);
    }

    getQuestionCount() {
        return this.store.getQuestionCount();
    }
}

window.QuestionManager = QuestionManager;