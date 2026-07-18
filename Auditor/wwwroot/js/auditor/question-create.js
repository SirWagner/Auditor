document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("questionCreateForm");
    if (!form) return;

    const submitButton = form.querySelector("button[type='submit']");

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (typeof $ !== "undefined" && !$(form).valid()) {
            return;
        }

        submitButton.disabled = true;
        submitButton.innerHTML = `
            <i class="fas fa-spinner fa-spin"></i>
            Creating...
        `;

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const result = await response.json();

            if (result.success) {
                await Swal.fire({
                    title: "Question Created",
                    text: result.message,
                    icon: "success"
                });

                form.reset();
            } else {
                showValidationErrors(result);

                Swal.fire({
                    title: "Validation Error",
                    text: "Please review the fields.",
                    icon: "error"
                });
            }
        } catch (error) {
            console.error(error);
            Swal.fire("Error", "Unexpected error occurred.", "error");
        } finally {
            resetButton();
        }
    });

    function resetButton() {
        submitButton.disabled = false;
        submitButton.innerHTML = `
            <i class="fas fa-check-circle"></i>
            Create Question
        `;
    }

    function showValidationErrors(errors) {
        document.querySelectorAll("[data-valmsg-for]").forEach(x => x.textContent = "");

        Object.keys(errors).forEach(key => {
            const element = document.querySelector(`[data-valmsg-for="${key}"]`);

            if (element) {
                element.textContent = errors[key].errors[0].errorMessage;
            }
        });
    }
});