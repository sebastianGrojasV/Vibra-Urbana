window.addEventListener("DOMContentLoaded", () => {
    const mobile = window.matchMedia("(max-width: 991.98px)");
    const sidebar = document.querySelector(".vu-admin-sidebar");
    const main = document.querySelector("main");
    const header = document.querySelector("body > header");

    if (!sidebar && document.querySelector(".vu-catalog-section, .vu-product-detail-page, .vu-cart-page")) {
        const dock = document.createElement("nav");
        dock.className = "vu-mobile-store-nav";
        dock.setAttribute("aria-label", "Navegación de la tienda");
        const catalog = document.querySelector(".navbar-brand");
        const cart = document.querySelector(".vu-cart-nav-link");
        [[catalog, "Catálogo", "package"], [cart, "Carrito", "cart"]].forEach(([source, label, icon]) => {
            if (!source) return;
            const link = document.createElement("a");
            link.href = source.href;
            link.innerHTML = `${iconSvg(icon)}<span>${label}</span>`;
            const isCart = label === "Carrito";
            if (isCart) {
                const count = document.createElement("span");
                count.className = "vu-mobile-cart-count";
                count.dataset.vuCartCount = "";
                count.hidden = true;
                link.append(count);
            }
            if (isCart ? document.querySelector(".vu-cart-page") : document.querySelector(".vu-catalog-section, .vu-product-detail-page")) link.setAttribute("aria-current", "page");
            dock.append(link);
        });
        document.body.append(dock);
        document.body.classList.add("vu-has-store-nav");
        updateCartNavigationCount();
    }

    const enhanceCartQuantities = () => {
        document.querySelectorAll("[data-vu-cart-quantity]").forEach(input => {
            if (input.closest(".vu-quantity-stepper")) return;
            const group = document.createElement("span");
            group.className = "vu-quantity-stepper";
            input.before(group);
            const buttons = [-1, 1].map(direction => {
                const button = document.createElement("button");
                button.type = "button";
                button.textContent = direction === 1 ? "+" : "-";
                button.setAttribute("aria-label", direction === 1 ? "Aumentar cantidad" : "Disminuir cantidad");
                button.disabled = direction === 1 ? Number(input.value) >= Number(input.max) : Number(input.value) <= Number(input.min);
                button.addEventListener("click", () => {
                    if (direction === 1) input.stepUp(); else input.stepDown();
                    input.dispatchEvent(new Event("input", { bubbles: true }));
                    queueMicrotask(() => {
                        const current = [...document.querySelectorAll("[data-vu-cart-quantity]")].find(field => field.dataset.vuProductId === input.dataset.vuProductId);
                        const buttons = current?.closest(".vu-quantity-stepper")?.querySelectorAll("button");
                        const nextButton = buttons?.[direction === 1 ? 1 : 0];
                        if (nextButton && !nextButton.disabled) nextButton.focus({ preventScroll: true });
                    });
                });
                return button;
            });
            group.append(buttons[0], input, buttons[1]);
        });
    };
    enhanceCartQuantities();
    document.querySelectorAll("[data-vu-cart-items]").forEach(items => {
        new MutationObserver(() => { enhanceCartQuantities(); enhanceImages(); }).observe(items, { childList: true, subtree: true });
    });

    if (sidebar && window.bootstrap) {
        sidebar.id = "vu-mobile-navigation";
        const bar = document.createElement("div");
        bar.className = "vu-mobile-module-bar";
        const title = document.querySelector(".vu-admin-header h1, .vu-admin-content h1");
        const open = document.createElement("button");
        open.type = "button";
        open.className = "btn btn-outline-primary";
        open.innerHTML = `${iconSvg("menu")}<span>Módulos</span>`;
        open.setAttribute("aria-controls", sidebar.id);
        open.setAttribute("aria-expanded", "false");
        const label = document.createElement("strong");
        label.textContent = title?.textContent || "Administración";
        bar.append(open, label);
        main.prepend(bar);

        const close = document.createElement("button");
        close.type = "button";
        close.className = "btn-close btn-close-white vu-mobile-nav-close";
        close.setAttribute("aria-label", "Cerrar menú de módulos");
        sidebar.prepend(close);
        let drawer;
        const sync = () => {
            if (mobile.matches) {
                sidebar.classList.add("offcanvas", "offcanvas-start");
                sidebar.tabIndex = -1;
                sidebar.setAttribute("aria-label", "Módulos del sistema");
                drawer = bootstrap.Offcanvas.getOrCreateInstance(sidebar);
            } else {
                const instance = bootstrap.Offcanvas.getInstance(sidebar);
                if (instance && sidebar.classList.contains("show")) {
                    sidebar.addEventListener("hidden.bs.offcanvas", sync, { once: true });
                    instance.hide();
                    return;
                }
                instance?.dispose();
                drawer = null;
                sidebar.classList.remove("offcanvas", "offcanvas-start");
                sidebar.removeAttribute("tabindex");
            }
        };
        open.addEventListener("click", () => drawer?.show());
        close.addEventListener("click", () => drawer?.hide());
        sidebar.addEventListener("shown.bs.offcanvas", () => open.setAttribute("aria-expanded", "true"));
        sidebar.addEventListener("hidden.bs.offcanvas", () => {
            open.setAttribute("aria-expanded", "false");
            if (mobile.matches) open.focus();
        });
        mobile.addEventListener("change", sync);
        sync();
    }

    const annotate = (table) => {
        const headings = [...table.querySelectorAll("thead tr:first-child th")];
        table.classList.add("vu-mobile-table");
        table.setAttribute("role", "table");
        table.querySelectorAll("thead, tbody").forEach(group => group.setAttribute("role", "rowgroup"));
        table.querySelectorAll("tr").forEach(row => row.setAttribute("role", "row"));
        headings.forEach(heading => heading.setAttribute("role", "columnheader"));
        table.querySelectorAll("tbody tr").forEach(row => {
            [...row.cells].forEach((cell, index) => {
                cell.setAttribute("role", "cell");
                if (cell.colSpan === 1 && headings[index]) {
                    cell.dataset.label = headings[index].textContent.trim();
                    cell.classList.toggle("vu-mobile-actions", headings[index].classList.contains("vu-actions-column") || cell.dataset.label === "Acciones");
                }
            });
        });
    };
    document.querySelectorAll(".table-responsive table").forEach(table => {
        if (!table.querySelector("thead th")) return;
        annotate(table);
        if (window.jQuery) jQuery(table).on("draw.dt", () => annotate(table));
        if (window.jQuery?.fn?.DataTable?.isDataTable(table)) {
            const dataTable = jQuery(table).DataTable();
            const sort = document.createElement("div");
            sort.className = "vu-mobile-sort";
            const fieldLabel = document.createElement("label");
            fieldLabel.textContent = "Ordenar por";
            const field = document.createElement("select");
            field.className = "form-select";
            field.add(new Option("Orden original", ""));
            [...table.querySelectorAll("thead th")].forEach((heading, index) => {
                if (dataTable.settings()[0].aoColumns[index]?.bSortable) field.add(new Option(heading.textContent.trim(), String(index)));
            });
            const directionLabel = document.createElement("label");
            directionLabel.textContent = "Orden";
            const direction = document.createElement("select");
            direction.className = "form-select";
            direction.add(new Option("Ascendente", "asc"));
            direction.add(new Option("Descendente", "desc"));
            const update = () => dataTable.order(field.value === "" ? [] : [[Number(field.value), direction.value]]).draw();
            field.addEventListener("change", update);
            direction.addEventListener("change", update);
            fieldLabel.append(field);
            directionLabel.append(direction);
            sort.append(fieldLabel, directionLabel);
            table.before(sort);
        }
        new MutationObserver(() => annotate(table)).observe(table, { childList: true, subtree: true });
    });
    document.querySelectorAll(".table-responsive").forEach(region => {
        region.tabIndex = 0;
        region.setAttribute("role", "region");
        region.setAttribute("aria-label", "Listado de registros");
    });

    document.querySelectorAll(".vu-mobile-filters").forEach(details => {
        const fields = [...details.querySelectorAll("input, select")];
        details.open = !mobile.matches || fields.some(field => field.value !== "");
        mobile.addEventListener("change", () => { details.open = !mobile.matches; });
    });

    document.querySelectorAll('input[autocomplete="tel"]').forEach(input => { input.inputMode = "tel"; });
    document.querySelectorAll('input[name="Identificacion"], input[name="Cedula"]').forEach(input => { input.inputMode = "numeric"; });

    const confirmation = document.querySelector("[data-vu-confirm-frame]");
    if (confirmation) {
        confirmation.setAttribute("role", "dialog");
        confirmation.setAttribute("aria-label", "Confirmar acción");
        let previousFocus;
        document.addEventListener("submit", event => {
            if (event.target.matches("form[data-vu-confirm]")) previousFocus = event.submitter || document.activeElement;
        }, true);
        new MutationObserver(() => {
            if (!confirmation.hidden) {
                previousFocus = previousFocus || document.activeElement;
                [...confirmation.querySelectorAll("textarea, button")].find(control => control.getClientRects().length)?.focus();
            } else {
                previousFocus?.focus();
                previousFocus = null;
            }
        }).observe(confirmation, { attributes: true, attributeFilter: ["hidden"] });
        confirmation.addEventListener("keydown", event => {
            if (event.key === "Escape") confirmation.querySelector("[data-vu-confirm-cancel]")?.click();
            if (event.key !== "Tab") return;
            const controls = [...confirmation.querySelectorAll("button, textarea")].filter(control => control.getClientRects().length && !control.disabled);
            const first = controls[0], last = controls.at(-1);
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
            if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
        });
    }

    function enhanceImages() {
      document.querySelectorAll(".vu-catalog-media img, .vu-product-detail-media img, .vu-cart-item-media img, .vu-product-thumb").forEach(img => {
        if (img.dataset.vuFallbackReady) return;
        img.dataset.vuFallbackReady = "true";
        const fallback = () => {
            const placeholder = document.createElement("div");
            placeholder.className = "vu-catalog-image-fallback";
            if (img.classList.contains("vu-product-thumb")) placeholder.classList.add("vu-image-thumb-fallback");
            placeholder.setAttribute("role", "img");
            placeholder.setAttribute("aria-label", img.alt || "Imagen no disponible");
            const initials = document.createElement("span");
            initials.textContent = (img.alt || "VU").slice(0, 2).toUpperCase();
            const caption = document.createElement("small");
            caption.textContent = "Imagen no disponible";
            placeholder.append(initials, caption);
            img.replaceWith(placeholder);
        };
        if (img.complete && !img.naturalWidth) fallback();
        else img.addEventListener("error", fallback, { once: true });
      });
    }
    enhanceImages();
    const catalogResults = document.querySelector("[data-vu-catalog-results]");
    if (catalogResults) new MutationObserver(enhanceImages).observe(catalogResults, { childList: true, subtree: true });

    const updateHeader = () => {
        if (header) document.documentElement.style.setProperty("--vu-header-height", `${header.getBoundingClientRect().height}px`);
    };
    if (header && window.ResizeObserver) new ResizeObserver(updateHeader).observe(header);
    updateHeader();
});
