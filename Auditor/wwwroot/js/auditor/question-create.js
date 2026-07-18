let checklistIndex = 0;


$(document).ready(function () {


    $('#QuestionTypeId').change(function () {


        const selectedType = $(this).val();


        if (selectedType == "7") {
            $('#checklist-section').show();
        }
        else {
            $('#checklist-section').hide();
        }


    });



    $('#add-checklist-item').click(function () {


        let html = `

        <div class="row mb-2 checklist-item">


            <div class="col-md-10">

                <input 
                    class="form-control"
                    name="ChecklistItems[${checklistIndex}].Text"
                    placeholder="Checklist item"/>

            </div>


            <div class="col-md-2">

                <button 
                    type="button"
                    class="btn btn-danger remove-item">

                    Remove

                </button>

            </div>


            <input 
                type="hidden"
                name="ChecklistItems[${checklistIndex}].Sequence"
                value="${checklistIndex + 1}" />


        </div>


        `;


        $('#checklist-items').append(html);


        checklistIndex++;


    });



    $(document).on(
        'click',
        '.remove-item',
        function () {

            $(this)
                .closest('.checklist-item')
                .remove();

        }
    );


});