document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-vu-character-counts] input[maxlength], [data-vu-character-counts] textarea[maxlength]")
        .forEach((field) => {
            const counter = document.createElement("small");
            counter.id = `${field.id}-character-count`;
            counter.className = "d-block text-end text-muted mt-1 vu-character-count";
            field.insertAdjacentElement("afterend", counter);
            field.setAttribute("aria-describedby", [field.getAttribute("aria-describedby"), counter.id].filter(Boolean).join(" "));
            const update = () => {
                counter.textContent = `${field.value.length} / ${field.maxLength}`;
                counter.classList.toggle("text-danger", field.value.length > field.maxLength);
            };
            field.addEventListener("input", update);
            field.addEventListener("change", update);
            field.form?.addEventListener("reset", () => window.setTimeout(update, 0));
            update();
        });
});
