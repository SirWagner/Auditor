console.log("🚀 Audit Render Script Loaded");

let isValid = true;

document.querySelectorAll("input[data-requires-reason]")
    .forEach(input => {

        input.addEventListener("change", function () {

            const container =
                this.closest(".question-container")
                    .querySelector(".reason-block");

            if (!container) return;

            if (this.dataset.requiresReason === "true")
                container.style.display = "block";
            else
                container.style.display = "none";

        });

    });
document.querySelectorAll(".reason-block").forEach(block => {

    const questionId = block.dataset.question;

    const reasonCheckboxes =
        block.querySelectorAll("input[type='checkbox']");

    const customText =
        block.querySelector("textarea");

    let anyChecked = false;

    reasonCheckboxes.forEach(c => {
        if (c.checked) anyChecked = true;
    });

    if (!anyChecked && customText.value.trim() === "") {

        isValid = false;

        block.classList.add("field-error");

        console.log("Reason required for question:", questionId);
    }

});


/* =====================================================
   MAIN SUBMIT ENTRY
===================================================== */

function submitAudit(executionId) {

    console.log("====================================");
    console.log("START AUDIT SUBMISSION");
    console.log("Execution ID:", executionId);
    console.log("====================================");

    isValid = true;

    clearErrors();

    validateAllQuestions();

    if (!isValid) {

        console.log("❌ VALIDATION FAILED");

        Swal.fire({
            icon: 'error',
            title: 'Validation Error',
            html: 'Please fix the highlighted fields and try again.'
        });

        return;
    }

    console.log("✅ VALIDATION PASSED");
    console.log("Preparing payload...");

    const payload = buildPayload(executionId);

    console.log("📦 Payload:", payload);

    const endpoint = window.__AUDIT_EXECUTION_STATUS__ === "REJECTED"
        ? "/AuditExecutions/Resubmit"
        : "/AuditExecutions/Submit";

    sendToBackend(payload, endpoint, "Audit submitted successfully.");
}

/* =====================================================
   SAVE DRAFT (no validation - partial answers are fine)
===================================================== */

function saveDraft(executionId) {

    console.log("Saving draft for execution:", executionId);

    const payload = buildPayload(executionId);

    sendToBackend(payload, "/AuditExecutions/SaveDraft", "Draft saved.");
}

/* =====================================================
   CLEAR ERRORS
===================================================== */

function clearErrors() {

    console.log("Clearing previous validation errors...");

    document.querySelectorAll(".field-error")
        .forEach(el => el.classList.remove("field-error"));

    document.querySelectorAll(".question-error")
        .forEach(el => el.classList.remove("question-error"));
}

/* =====================================================
   VALIDATION ENGINE
===================================================== */

function validateAllQuestions() {

    console.log("Running full validation...");

    const cards = document.querySelectorAll(".card");

    cards.forEach((card, index) => {

        console.log("Checking question:", index + 1);

        let questionValid = true;

        const inputs = card.querySelectorAll("input, textarea, select");

        inputs.forEach(input => {

            if (input.type === "radio") {

                const group = card.querySelectorAll(
                    `input[name='${input.name}']`
                );

                let checked = false;

                group.forEach(r => {
                    if (r.checked) checked = true;
                });

                if (!checked) {
                    questionValid = false;
                    console.log("❌ Radio not selected");
                }
            }

            if (input.type === "checkbox") {
                // Handled in reason logic
            }

            if (input.tagName === "TEXTAREA") {
                if (input.value.trim() === "") {
                    questionValid = false;
                    input.classList.add("field-error");
                    console.log("❌ Empty textarea");
                }
            }

            if (input.type === "number") {
                if (input.value === "" || isNaN(input.value)) {
                    questionValid = false;
                    input.classList.add("field-error");
                    console.log("❌ Invalid number");
                }
            }

            if (input.type === "date") {
                if (input.value === "") {
                    questionValid = false;
                    input.classList.add("field-error");
                    console.log("❌ Date missing");
                }
            }

            if (input.type === "time") {
                if (input.value === "") {
                    questionValid = false;
                    input.classList.add("field-error");
                    console.log("❌ Time missing");
                }
            }
        });

        if (!questionValid) {
            card.classList.add("question-error");
            isValid = false;
        }
    });

    validateReasonRules();
}

/* =====================================================
   REASON VALIDATION (FIXED LOGIC)
===================================================== */

function validateReasonRules() {

    console.log("Validating requires_reason logic...");

    document.querySelectorAll(".question-container")
        .forEach(container => {

            const selectedOption =
                container.querySelector("input[type='radio']:checked");

            if (!selectedOption) return;

            if (selectedOption.dataset.requiresReason !== "true")
                return;

            console.log("Reason required for this selection");

            const reasonBlock =
                container.querySelector(".reason-block");

            if (!reasonBlock) return;

            const checkboxes =
                reasonBlock.querySelectorAll("input[type='checkbox']:checked");

            const customText =
                reasonBlock.querySelector("textarea");

            if (checkboxes.length === 0 &&
                customText.value.trim() === "") {

                console.log("❌ Reason missing");

                reasonBlock.classList.add("field-error");

                isValid = false;
            }
        });
}

/* =====================================================
   BUILD PAYLOAD
===================================================== */

function buildPayload(executionId) {
    const formData = new FormData();
    formData.append("ExecutionId", executionId);

    document.querySelectorAll(".question-container").forEach((container, index) => {
        const templateItemId = container.dataset.question;

        const selectedOption = container.querySelector("input[type='radio']:checked");
        const selectedOptionId = selectedOption ? selectedOption.value : null;

        const selectedReasonIds = [];
        container.querySelectorAll(".reason-block input[type='checkbox']:checked")
            .forEach(c => selectedReasonIds.push(c.value));

        const customReason = container.querySelector(".reason-block textarea")?.value || "";

        // Append normal fields
        formData.append(`Responses[${index}].TemplateItemId`, templateItemId);
        formData.append(`Responses[${index}].SelectedOptionId`, selectedOptionId);
        formData.append(`Responses[${index}].CustomReason`, customReason);

        selectedReasonIds.forEach((reasonId, rIndex) => {
            formData.append(`Responses[${index}].SelectedReasonIds[${rIndex}]`, reasonId);
        });

        // Append attachments
        const files = container.querySelectorAll("input[type='file']");
        files.forEach(input => {
            for (let i = 0; i < input.files.length; i++) {
                formData.append(`Responses[${index}].Attachments`, input.files[i]);
            }
        });
    });

    return formData;
}

/* =====================================================
   SEND TO BACKEND
===================================================== */

function sendToBackend(payload, endpoint, successMessage) {

    endpoint = endpoint || '/AuditExecutions/Submit';
    successMessage = successMessage || 'Audit submitted successfully.';

    console.log("Sending to backend:", endpoint);

    fetch(endpoint, {
        method: 'POST',
        body: (payload)
    })
        .then(async response => {

            const data = await response.json();

            console.log("Backend Response:", data);

            if (!response.ok || !data.success) {

                Swal.fire({
                    icon: 'error',
                    title: 'Server Error',
                    html: data.message || 'Unknown backend error'
                });

                return;
            }

            Swal.fire({
                icon: 'success',
                title: 'Success',
                text: successMessage
            }).then(() => {
                if (endpoint !== '/AuditExecutions/SaveDraft') {
                    window.location.href = "/AuditExecutions";
                }
            });

        })
        .catch(error => {

            console.log("Network/Error:", error);

            Swal.fire({
                icon: 'error',
                title: 'Unexpected Error',
                text: error.toString()
            });
        });
} 