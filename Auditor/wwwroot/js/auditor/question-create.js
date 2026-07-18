let checklistIndex = 0;
let optionIndex = 0;

$(document).ready(function () {
    $('#QuestionTypeId').change(function () {
        const selectedType = $(this).find(":selected").text();
        renderQuestionTypeSettings(selectedType);
    });

    initializeDynamicButtons();
});

function renderQuestionTypeSettings(type) {
    const container = $('#question-type-settings');
    container.empty();

    switch (type) {
        case "CHECKLIST":
            renderChecklist(container);
            break;
        case "MULTIPLE_CHOICE":
            renderMultipleChoice(container);
            break;
        case "RATING":
            renderRating(container);
            break;
        default:
            break;
    }
}

function renderChecklist(container) {
    container.html(`
        <div class="card mt-4">
            <div class="card-header">
                <i class="fas fa-list-check"></i>
                Checklist Items
            </div>
            <div class="card-body">
                <div id="checklist-items"></div>
                <button type="button" id="add-checklist-item" class="btn btn-outline-primary">
                    <i class="fas fa-plus"></i>
                    Add Checklist Item
                </button>
            </div>
        </div>
    `);
}

function renderMultipleChoice(container) {
    container.html(`
        <div class="card mt-4">
            <div class="card-header">
                <i class="fas fa-list"></i>
                Answer Options
            </div>
            <div class="card-body">
                <div id="multiple-choice-options"></div>
                <button type="button" id="add-option" class="btn btn-outline-primary">
                    <i class="fas fa-plus"></i>
                    Add Option
                </button>
            </div>
        </div>
    `);
}

function renderRating(container) {
    container.html(`
        <div class="card mt-4">
            <div class="card-header">
                <i class="fas fa-star"></i>
                Rating Configuration
            </div>
            <div class="card-body">
                <label class="form-label">Maximum Rating</label>
                <select class="form-select" name="RatingMaxValue">
                    <option value="5">5</option>
                    <option value="10">10</option>
                </select>
            </div>
        </div>
    `);
}

function initializeDynamicButtons() {
    $(document).on('click', '#add-checklist-item', function () {
        addChecklistItem();
    });

    $(document).on('click', '#add-option', function () {
        addMultipleChoiceOption();
    });

    $(document).on('click', '.remove-dynamic-item', function () {
        $(this).closest('.dynamic-item').remove();
    });
}

function addChecklistItem() {
    let html = `
    <div class="row mb-2 dynamic-item">
        <div class="col-md-10">
            <input class="form-control" name="Options[${checklistIndex}].Text" placeholder="Checklist item" required />
        </div>
        <div class="col-md-2">
            <button type="button" class="btn btn-danger remove-dynamic-item">Remove</button>
        </div>
        <input type="hidden" name="Options[${checklistIndex}].Sequence" value="${checklistIndex + 1}" />
    </div>
    `;

    $('#checklist-items').append(html);
    checklistIndex++;
}

function addMultipleChoiceOption() {
    let html = `
    <div class="row mb-2 dynamic-item">
        <div class="col-md-10">
            <input class="form-control" name="Options[${optionIndex}].Text" placeholder="Answer option" required />
        </div>
        <div class="col-md-2">
            <button type="button" class="btn btn-danger remove-dynamic-item">Remove</button>
        </div>
        <input type="hidden" name="Options[${optionIndex}].Sequence" value="${optionIndex + 1}" />
    </div>
    `;

    $('#multiple-choice-options').append(html);
    optionIndex++;
}