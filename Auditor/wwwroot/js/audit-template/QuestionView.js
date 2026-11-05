export default class QuestionView {
    constructor({ responsibilities = [], serviceProviders = [] } = {}) {
        this.responsibilities = responsibilities;
        this.dom = this.initializeDomReferences();
        this.createProviderList(serviceProviders);
    }

    // Suggestions for the Service provider fields.
    createProviderList(serviceProviders) {
        const list = document.createElement("datalist");
        list.id = "templateProviderList";
        list.innerHTML = serviceProviders.map(p => `<option value="${this.escapeHtml(p)}"></option>`).join("");
        document.body.appendChild(list);
    }

    responsibilityOptions(selected) {
        const names = selected && !this.responsibilities.includes(selected)
            ? [...this.responsibilities, selected]
            : this.responsibilities;
        return `<option value="">Not assigned</option>` + names
            .map(n => `<option value="${this.escapeHtml(n)}" ${n === selected ? "selected" : ""}>${this.escapeHtml(n)}</option>`)
            .join("");
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
            <div class="question-bank-item ${isAlreadyAdded ? "is-added" : ""}"
                 data-question-id="${question.id}"
                 data-question-text="${this.escapeHtml(question.text)}"
                 data-question-type="${this.escapeHtml(question.questionType)}"
                 data-question-contractor="${this.escapeHtml(question.contractor ?? "")}">

                <div class="fw-semibold">
                    ${this.escapeHtml(question.text)}
                </div>

                <div class="text-muted small">
                    ${this.escapeHtml(question.questionType)}
                    ${isAlreadyAdded ? '<span class="badge bg-light text-success ms-2" title="Already on this template. Add it again to ask it in another section.">Added</span>' : ""}
                </div>
            </div>
        `;
    }

    /* -----------------------------
     * QUESTION CARD RENDERING
     * ----------------------------- */

    renderQuestionCard(
        questionId,
        text,
        type,
        index,
        mandatory = false,
        sequence = null,
        section = "",
        itemId = null,
        responsibility = "",
        serviceProvider = ""
    ) {
        const container = this.dom.questionContainer;
        if (!container) return;

        const card = document.createElement("div");

        card.className = "card mb-3 shadow-sm";
        card.dataset.questionId = questionId;

        card.innerHTML = this.buildQuestionCardHtml({
            questionId,
            text,
            type,
            index,
            mandatory,
            sequence,
            section,
            itemId,
            responsibility,
            serviceProvider
        });

        container.appendChild(card);
        this.updateSectionList();
    }

    buildQuestionCardHtml({
        questionId,
        text,
        type,
        index,
        mandatory,
        sequence,
        section,
        itemId,
        responsibility,
        serviceProvider
    }) {

        // Ids use the card index: the same question can be on the template more than once.
        const key = `item-${index}-${Date.now().toString(36)}`;
        const mandatoryId = `${key}-mandatory`;
        const sequenceId = `${key}-sequence`;
        const sectionId = `${key}-section`;
        const responsibilityId = `${key}-responsibility`;
        const providerId = `${key}-provider`;

        return `
        <div class="card-body">

            <div class="d-flex justify-content-between align-items-start">

                <div>
                    <div class="fw-semibold">
                        ${this.escapeHtml(text)}
                    </div>

                    <span class="badge bg-light text-primary mt-1">
                        ${this.escapeHtml(type)}
                    </span>
                </div>


                <button type="button"
                        class="btn btn-sm btn-outline-danger"
                        data-action="deleteQuestion"
                        aria-label="Remove question">
                    <i class="fas fa-trash"></i>
                </button>

            </div>


            <!-- Hidden fields -->

            <input type="hidden"
                   name="Items[${index}].QuestionBankId"
                   value="${questionId}" />

            <input type="hidden"
                   name="Items[${index}].QuestionType"
                   value="${this.escapeHtml(type)}" />

            ${itemId ? `<input type="hidden" name="Items[${index}].Id" value="${itemId}" />` : ""}

            <div class="mt-3">
                <label class="form-label" for="${sectionId}">Section</label>
                <input class="form-control"
                       type="text"
                       id="${sectionId}"
                       data-field="section"
                       name="Items[${index}].Section"
                       value="${this.escapeHtml(section ?? "")}"
                       list="templateSectionList"
                       maxlength="500"
                       placeholder="e.g. Arrival › Environment, or Female" />
                <div class="form-text">Optional. Questions with the same section are shown together; the part before the first › becomes a tab when auditing.</div>
            </div>

            <div class="row mt-3">
                <div class="col-md-6">
                    <label class="form-label" for="${responsibilityId}">Responsibility</label>
                    <select class="form-select" id="${responsibilityId}" name="Items[${index}].Responsibility">
                        ${this.responsibilityOptions(responsibility)}
                    </select>
                    <div class="form-text">Who answers for this question in reports.</div>
                </div>
                <div class="col-md-6">
                    <label class="form-label" for="${providerId}">Service provider</label>
                    <input class="form-control"
                           type="text"
                           id="${providerId}"
                           name="Items[${index}].ServiceProvider"
                           value="${this.escapeHtml(serviceProvider ?? "")}"
                           list="templateProviderList"
                           maxlength="200" />
                    <div class="form-text">Only if one company always answers for it. Otherwise it comes from the audit or site.</div>
                </div>
            </div>


            <div class="row mt-3">

                <div class="col-md-6">

                    <div class="form-check">

                       <input class="form-check-input"
                               type="checkbox"
                               id="${mandatoryId}"
                               name="Items[${index}].Mandatory"
                               value="true"
                               ${mandatory ? "checked" : ""} />

                        <input type="hidden"
                               name="Items[${index}].Mandatory"
                               value="false" />

                        <label class="form-check-label"
                               for="${mandatoryId}">
                            Mandatory
                        </label>

                    </div>

                </div>


                <div class="col-md-6">

                    <label class="form-label"
                           for="${sequenceId}">
                        Sequence
                    </label>

                    <input class="form-control"
                           type="number"
                           id="${sequenceId}"
                           name="Items[${index}].Sequence"
                           value="${sequence ?? index + 1}"
                           min="1" />

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
        this.updateSectionList();
    }

    // Suggestions for the Section fields: every section already used on this template.
    updateSectionList() {
        let list = document.getElementById("templateSectionList");
        if (!list) {
            list = document.createElement("datalist");
            list.id = "templateSectionList";
            document.body.appendChild(list);
        }
        const sections = new Set();
        this.dom.questionContainer?.querySelectorAll("[data-field='section']").forEach(input => {
            const value = input.value.trim();
            if (value) sections.add(value);
        });
        list.innerHTML = [...sections].sort().map(v => `<option value="${this.escapeHtml(v)}"></option>`).join("");
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
        div.textContent = text ?? "";
        return div.innerHTML.replace(/"/g, "&quot;");
    }
}