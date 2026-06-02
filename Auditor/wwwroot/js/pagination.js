window.Paginator = (function () {

    function init(options) {
        const {
            tableBodyId,
            paginationId,
            rowsPerPage = 5
        } = options;

        const tableBody = document.getElementById(tableBodyId);
        const pagination = document.getElementById(paginationId);

        if (!tableBody || !pagination) {
            console.warn("Paginator: Missing elements");
            return;
        }

        const rows = Array.from(tableBody.querySelectorAll("tr"));
        let currentPage = 1;
        const totalPages = Math.ceil(rows.length / rowsPerPage);

        function displayPage(page) {
            currentPage = page;

            rows.forEach((row, index) => {
                row.style.display =
                    index >= (page - 1) * rowsPerPage &&
                        index < page * rowsPerPage
                        ? ""
                        : "none";
            });

            renderPagination();
        }

        function renderPagination() {
            pagination.innerHTML = "";

            // Previous
            const prev = document.createElement("li");
            prev.className = `page-item ${currentPage === 1 ? "disabled" : ""}`;
            prev.innerHTML = `<a class="page-link" href="#">Previous</a>`;
            prev.onclick = (e) => {
                e.preventDefault();
                if (currentPage > 1) displayPage(currentPage - 1);
            };
            pagination.appendChild(prev);

            // Numbers
            for (let i = 1; i <= totalPages; i++) {
                const li = document.createElement("li");
                li.className = `page-item ${i === currentPage ? "active" : ""}`;

                const a = document.createElement("a");
                a.className = "page-link";
                a.href = "#";
                a.innerText = i;

                a.onclick = (e) => {
                    e.preventDefault();
                    displayPage(i);
                };

                li.appendChild(a);
                pagination.appendChild(li);
            }

            // Next
            const next = document.createElement("li");
            next.className = `page-item ${currentPage === totalPages ? "disabled" : ""}`;
            next.innerHTML = `<a class="page-link" href="#">Next</a>`;
            next.onclick = (e) => {
                e.preventDefault();
                if (currentPage < totalPages) displayPage(currentPage + 1);
            };
            pagination.appendChild(next);
        }

        displayPage(1);
    }

    return {
        init
    };
})();