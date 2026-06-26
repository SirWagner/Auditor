class QuestionManager {
    constructor({ questionBank }) {
        this.state = {
            questionIndex: 0,
            addedQuestions: new Set(),
            questions: questionBank || []
        };

        this.cacheElements();
        this.bindEvents();
        this.renderQuestionBankList();
        this.updateUIState();
    }

    cacheElements() {
        this.els = {
            container: document.getElementById("questionContainer"),
            modal: document.getElementById("questionBankModal"),
            searchInput: document.getElementById("questionSearch"),
            questionBankList: document.getElementById("questionBankList"),
            form: document.getElementById("auditTemplateForm")
        };
    }

    bindEvents() {
        if (this.els.modal) {
            this.els.modal.addEventListener("shown.bs.modal", () => {
                if (this.els.searchInput) this.els.searchInput.value = "";
                this.renderQuestionBankList();
            });
        }

        this.els.searchInput?.addEventListener("input", (e) => {
            this.filterQuestions(e.target.value);
        });

        this.els.questionBankList?.addEventListener("click", (e) => {
            const button = e.target.closest("[data-question-id]");
            if (!button) return;

            this.addQuestion(
                button.dataset.questionId,
                button.dataset.questionText,
                button.dataset.questionType
            );
        });

        this.els.container?.addEventListener("click", (e) => {
            if (e.target.closest("[data-action='deleteQuestion']")) {
                this.deleteQuestion(e.target.closest(".card"));
            }
        });

        this.els.form?.addEventListener("submit", (e) => {
            if (this.state.questionIndex === 0) {
                e.preventDefault();
                alert("Please add at least one question");
            }
        });
    }

    filterQuestions(term) {
        const filtered = this.state.questions.filter(q =>
            q.text.toLowerCase().includes(term.toLowerCase())
        );
        this.renderQuestionBankList(filtered);
    }

    renderQuestionBankList(list = this.state.questions) {
        if (!this.els.questionBankList) return;

        this.els.questionBankList.innerHTML = list.map(q => {
            const isAdded = this.state.addedQuestions.has(q.id);

            return `
            <div class="question-bank-item ${isAdded ? "disabled" : ""}"
                 data-question-id="${q.id}"
                 data-question-text="${this.escape(q.text)}"
                 data-question-type="${this.escape(q.questionType)}">

                <div class="fw-semibold">${this.escape(q.text)}</div>
                <div class="text-muted small">${this.escape(q.questionType)}</div>
            </div>
        `;
        }).join("");
    }

    addQuestion(id, text, type) {
        if (this.state.addedQuestions.has(id)) {
            this.notify("Question already added", "warning");
            return;
        }

        this.state.addedQuestions.add(id);
        this.renderCard(id, text, type);
        this.state.questionIndex++;
        this.updateUIState();
        this.closeModal();
    }

    renderCard(id, text, type) {
        const index = this.state.questionIndex;

        const card = document.createElement("div");
        card.className = "card mb-3 shadow-sm";
        card.dataset.questionId = id;

        card.innerHTML = `
            <div class="card-body">
                <div class="d-flex justify-content-between">
                    <div>
                        <div class="fw-semibold">${this.escape(text)}</div>
                        <span class="badge bg-light text-primary">${this.escape(type)}</span>
                    </div>
                    <button class="btn btn-sm btn-outline-danger" data-action="deleteQuestion">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>

                <input type="hidden" name="Items[${index}].QuestionBankId" value="${id}" />
                <input type="hidden" name="Items[${index}].QuestionType" value="${type}" />

                <div class="row mt-3">
                    <div class="col-md-6">
                        <input type="checkbox" name="Items[${index}].Mandatory" />
                        <label>Mandatory</label>
                    </div>

                    <div class="col-md-6">
                        <input type="number" name="Items[${index}].Sequence" value="${index + 1}" />
                    </div>
                </div>
            </div>
        `;

        this.els.container?.appendChild(card);
    }

    deleteQuestion(card) {
        if (!confirm("Remove this question?")) return;

        const id = card.dataset.questionId;
        this.state.addedQuestions.delete(id);
        card.remove();
        this.reindex();
        this.updateUIState();
    }

    reindex() {
        const cards = this.els.container?.querySelectorAll(".card") || [];

        cards.forEach((card, index) => {
            card.querySelectorAll("input").forEach(input => {
                if (input.name.includes("QuestionBankId"))
                    input.name = `Items[${index}].QuestionBankId`;

                if (input.name.includes("Mandatory"))
                    input.name = `Items[${index}].Mandatory`;

                if (input.name.includes("Sequence")) {
                    input.name = `Items[${index}].Sequence`;
                    input.value = index + 1;
                }
            });
        });

        this.state.questionIndex = cards.length;
    }

    closeModal() {
        try {
            bootstrap.Modal.getOrCreateInstance(this.els.modal)?.hide();
        } catch { }
    }

    notify(msg, type = "info") {
        console.log(`[${type}] ${msg}`);
    }

    escape(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
    updateUIState() {
        const count = this.state.addedQuestions.size;

        const container = this.els.container;
        const counter = document.getElementById("questionCount");

        if (counter) counter.textContent = count;

        if (!container) return;

        if (count > 0) {
            container.classList.add("has-items");
        } else {
            container.classList.remove("has-items");
        }
    }
}

window.QuestionManager = QuestionManager;