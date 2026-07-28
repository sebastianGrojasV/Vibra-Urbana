
window.addEventListener("DOMContentLoaded", () => {
    enhanceSidebar();
    enhanceModulePlaceholders();
    setupInlineConfirmations();
    setupSaleCancellation();
    setupSaleStatusChange();
    setupAutoDismissAlerts();
    setupCatalogFilters();
    setupCatalogCartButtons();

    if (!window.jQuery || !jQuery.fn.DataTable) {
        return;
    }

    const hasDataTableButtons = Boolean(jQuery.fn.dataTable?.Buttons);
    const dataTableOptions = {
        dom: hasDataTableButtons ? 'Bfrtip' : 'frtip',
        pageLength: 8,
        lengthMenu: [5, 8, 10, 25],
        order: [],
        language: {
            search: "Buscar:",
            lengthMenu: "Mostrar _MENU_ registros",
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            infoEmpty: "Sin registros",
            infoFiltered: "(filtrado de _MAX_ registros)",
            zeroRecords: "No se encontraron resultados",
            emptyTable: "No hay datos disponibles",
            paginate: {
                first: "Primero",
                last: "Último",
                next: "Siguiente",
                previous: "Anterior"
            }
        },
        columnDefs: [
            {
                orderable: false,
                targets: "vu-actions-column"
            }
        ]
    };

    if (hasDataTableButtons) {
        dataTableOptions.buttons = [
            {
                extend: 'excelHtml5',
                text: `${iconSvg("file")}<span>Excel</span>`,
                title: document.title.replace(" - Vibra Urbana", ""),
                filename: exportFileName("excel"),
                className: "vu-export-btn vu-export-excel",
                exportOptions: {
                    columns: ":visible:not(.vu-actions-column)"
                }
            },
            {
                extend: 'pdfHtml5',
                text: `${iconSvg("file")}<span>PDF</span>`,
                title: document.title.replace(" - Vibra Urbana", ""),
                filename: exportFileName("pdf"),
                className: "vu-export-btn vu-export-pdf",
                orientation: 'landscape',
                pageSize: 'A4',
                exportOptions: {
                    columns: ":visible:not(.vu-actions-column)"
                },
                customize: (doc) => {
                    doc.defaultStyle.fontSize = 9;
                    doc.styles.tableHeader.fillColor = "#145a5a";
                    doc.styles.tableHeader.color = "#ffffff";
                    doc.styles.title.color = "#181311";
                    doc.pageMargins = [24, 32, 24, 32];
                }
            }
        ];
    }

    jQuery(".vu-data-table").each(function () {
        if (jQuery.fn.DataTable.isDataTable(this)) {
            return;
        }

        jQuery(this).DataTable(dataTableOptions);
    });
});

function exportFileName(type) {
    const title = document.title
        .replace(" - Vibra Urbana", "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/[^a-zA-Z0-9]+/g, "-")
        .replace(/^-|-$/g, "")
        .toLowerCase();
    const today = new Date().toISOString().slice(0, 10);

    return `vibra-urbana-${title || "reporte"}-${today}-${type}`;
}

function enhanceSidebar() {
    const sidebar = document.querySelector(".vu-admin-sidebar");

    if (!sidebar) {
        return;
    }

    const storageKey = "vibra.sidebarCollapsed";
    const icons = {
        "panel inicial": "home",
        "usuarios": "users",
        "roles": "shield",
        "clientes": "users",
        "productos": "package",
        "inventario": "archive",
        "ventas": "cart",
        "facturación": "file",
        "facturacion": "file",
        "cierre de caja": "file",
        "reportes": "chart",
        "pedidos": "bag"
    };

    const toggle = document.createElement("button");
    toggle.type = "button";
    toggle.className = "vu-sidebar-toggle";
    toggle.setAttribute("aria-label", "Contraer menú lateral");
    toggle.innerHTML = iconSvg("menu");
    sidebar.prepend(toggle);

    sidebar.querySelectorAll(".vu-sidebar-nav a").forEach((link) => {
        if (link.querySelector(".vu-sidebar-link-text")) {
            return;
        }

        const text = link.textContent.trim();
        const key = text.toLocaleLowerCase("es");
        link.innerHTML = `${iconSvg(icons[key] || "dot")}<span class="vu-sidebar-link-text">${text}</span>`;
        link.setAttribute("title", text);
    });

    const setCollapsed = (collapsed) => {
        document.body.classList.toggle("vu-sidebar-collapsed", collapsed);
        toggle.setAttribute("aria-label", collapsed ? "Expandir menú lateral" : "Contraer menú lateral");
        localStorage.setItem(storageKey, collapsed ? "true" : "false");
    };

    setCollapsed(localStorage.getItem(storageKey) === "true");

    toggle.addEventListener("click", () => {
        setCollapsed(!document.body.classList.contains("vu-sidebar-collapsed"));
    });
}

function setupInlineConfirmations() {
    const frame = document.querySelector("[data-vu-confirm-frame]");

    if (!frame) {
        return;
    }

    const title = frame.querySelector("[data-vu-confirm-title]");
    const message = frame.querySelector("[data-vu-confirm-message]");
    const submit = frame.querySelector("[data-vu-confirm-submit]");
    const cancel = frame.querySelector("[data-vu-confirm-cancel]");
    let pendingForm = null;

    document.querySelectorAll("form[data-vu-confirm]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (form.dataset.vuConfirmed === "true") {
                return;
            }

            event.preventDefault();
            pendingForm = form;
            title.textContent = form.dataset.vuConfirmTitle || "Confirmar acción";
            message.textContent = form.dataset.vuConfirmMessage || "Confirma para continuar.";
            submit.textContent = form.dataset.vuConfirmAction || "Confirmar";
            frame.hidden = false;
            frame.classList.add("is-visible");
            submit.focus();
        });
    });

    submit.addEventListener("click", () => {
        if (!pendingForm) {
            return;
        }

        pendingForm.dataset.vuConfirmed = "true";
        pendingForm.submit();
    });

    cancel.addEventListener("click", () => {
        pendingForm = null;
        frame.classList.remove("is-visible");
        frame.hidden = true;
    });
}

function setupSaleCancellation() {
    const frame = document.querySelector("[data-vu-cancel-sale-frame]");

    if (!frame) {
        return;
    }

    const idInput = frame.querySelector("[data-vu-cancel-sale-id]");
    const reasonInput = frame.querySelector("[data-vu-cancel-sale-reason]");
    const title = frame.querySelector("[data-vu-cancel-sale-title]");
    const message = frame.querySelector("[data-vu-cancel-sale-message]");
    const close = frame.querySelector("[data-vu-cancel-sale-close]");

    document.querySelectorAll("[data-vu-cancel-sale]").forEach((button) => {
        button.addEventListener("click", () => {
            const saleId = button.dataset.vuSaleId;
            const saleCode = button.dataset.vuSaleCode || `#${saleId}`;
            const saleClient = button.dataset.vuSaleClient || "cliente seleccionado";

            idInput.value = saleId;
            reasonInput.value = "";
            title.textContent = `Anular venta ${saleCode}`;
            message.textContent = `Confirma la anulación de la venta de ${saleClient}. El inventario será restaurado.`;
            frame.hidden = false;
            frame.classList.add("is-visible");
            reasonInput.focus();
        });
    });

    close.addEventListener("click", () => {
        idInput.value = "";
        reasonInput.value = "";
        frame.classList.remove("is-visible");
        frame.hidden = true;
    });
}

function setupSaleStatusChange() {
    const frame = document.querySelector("[data-vu-state-sale-frame]");

    if (!frame) {
        return;
    }

    const idInput = frame.querySelector("[data-vu-state-sale-id]");
    const statusInput = frame.querySelector("[data-vu-state-sale-status]");
    const reasonInput = frame.querySelector("[data-vu-state-sale-reason]");
    const title = frame.querySelector("[data-vu-state-sale-title]");
    const message = frame.querySelector("[data-vu-state-sale-message]");
    const submit = frame.querySelector("[data-vu-state-sale-submit]");
    const close = frame.querySelector("[data-vu-state-sale-close]");

    document.querySelectorAll("[data-vu-state-sale]").forEach((button) => {
        button.addEventListener("click", () => {
            const saleId = button.dataset.vuSaleId;
            const saleCode = button.dataset.vuSaleCode || `#${saleId}`;
            const saleClient = button.dataset.vuSaleClient || "cliente seleccionado";
            const targetStatus = button.dataset.vuTargetStatus;

            idInput.value = saleId;
            statusInput.value = targetStatus;
            reasonInput.value = "";
            title.textContent = `Cambiar venta ${saleCode} a ${targetStatus}`;
            message.textContent = `Registra el motivo del cambio para la venta de ${saleClient}.`;
            submit.textContent = `Marcar como ${targetStatus}`;
            frame.hidden = false;
            frame.classList.add("is-visible");
            reasonInput.focus();
        });
    });

    close.addEventListener("click", () => {
        idInput.value = "";
        statusInput.value = "";
        reasonInput.value = "";
        frame.classList.remove("is-visible");
        frame.hidden = true;
    });
}

function setupAutoDismissAlerts() {
    document.querySelectorAll(".alert-success").forEach((alert) => {
        window.setTimeout(() => {
            alert.classList.add("vu-alert-dismissing");
            window.setTimeout(() => alert.remove(), 260);
        }, 3000);
    });
}

function setupCatalogFilters() {
    const form = document.querySelector("[data-vu-catalog-form]");
    const results = document.querySelector("[data-vu-catalog-results]");

    if (!form || !results) {
        return;
    }

    const clear = document.querySelector("[data-vu-catalog-clear]");
    let controller = null;
    let timer = null;

    const buildUrl = () => {
        const url = new URL(form.action || window.location.href, window.location.origin);
        const params = new URLSearchParams();

        new FormData(form).forEach((value, key) => {
            const text = value.toString().trim();

            if (text) {
                params.set(key, text);
            }
        });

        url.search = params.toString();
        return url;
    };

    const loadCatalog = async (url) => {
        if (controller) {
            controller.abort();
        }

        controller = new AbortController();
        results.classList.add("is-loading");
        form.classList.add("is-loading");

        try {
            const response = await fetch(url, {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                signal: controller.signal
            });

            if (!response.ok) {
                throw new Error("No se pudo actualizar el catálogo.");
            }

            results.innerHTML = await response.text();
            window.history.replaceState({}, "", url);
        } catch (error) {
            if (error.name !== "AbortError") {
                form.submit();
            }
        } finally {
            results.classList.remove("is-loading");
            form.classList.remove("is-loading");
        }
    };

    const queueCatalogLoad = () => {
        window.clearTimeout(timer);
        timer = window.setTimeout(() => loadCatalog(buildUrl()), 320);
    };

    form.addEventListener("submit", (event) => {
        event.preventDefault();
        window.clearTimeout(timer);
        loadCatalog(buildUrl());
    });

    form.querySelectorAll("select").forEach((select) => {
        select.addEventListener("change", queueCatalogLoad);
    });

    form.querySelectorAll("input").forEach((input) => {
        input.addEventListener("input", queueCatalogLoad);
    });

    clear?.addEventListener("click", (event) => {
        event.preventDefault();
        form.reset();
        form.querySelectorAll("input").forEach((input) => {
            input.value = "";
        });
        form.querySelectorAll("select").forEach((select) => {
            select.selectedIndex = 0;
        });
        loadCatalog(new URL(clear.href, window.location.origin));
    });
}

function setupCatalogCartButtons() {
    document.querySelectorAll("[data-vu-add-cart]").forEach((button) => {
        button.addEventListener("click", () => {
            const stock = Number(button.dataset.vuProductStock || "0");
            const productId = button.dataset.vuProductId;
            const feedback = document.querySelector("[data-vu-cart-feedback]");

            if (!productId || stock <= 0) {
                return;
            }

            const storageKey = "vibra.catalogCart";
            const current = JSON.parse(localStorage.getItem(storageKey) || "[]");
            const item = current.find((product) => product.id === productId);

            if (item) {
                item.quantity = Math.min(item.quantity + 1, stock);
            } else {
                current.push({
                    id: productId,
                    name: button.dataset.vuProductName || "Producto",
                    price: Number(button.dataset.vuProductPrice || "0"),
                    quantity: 1
                });
            }

            localStorage.setItem(storageKey, JSON.stringify(current));

            if (feedback) {
                feedback.hidden = false;
                feedback.textContent = "Producto agregado al carrito.";
                window.setTimeout(() => {
                    feedback.hidden = true;
                }, 3000);
            }
        });
    });
}

function enhanceModulePlaceholders() {
    document.querySelectorAll("[data-vu-module-icon]").forEach((element) => {
        element.innerHTML = iconSvg(element.dataset.vuModuleIcon || "dot");
    });
}

function iconSvg(name) {
    const icons = {
        menu: '<path d="M4 7h16"></path><path d="M4 12h16"></path><path d="M4 17h16"></path>',
        home: '<path d="m3 11 9-8 9 8"></path><path d="M5 10v10h14V10"></path><path d="M9 20v-6h6v6"></path>',
        users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path><circle cx="9" cy="7" r="4"></circle><path d="M22 21v-2a4 4 0 0 0-3-3.87"></path><path d="M16 3.13a4 4 0 0 1 0 7.75"></path>',
        shield: '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z"></path><path d="m9 12 2 2 4-4"></path>',
        package: '<path d="m21 8-9-5-9 5 9 5 9-5Z"></path><path d="M3 8v8l9 5 9-5V8"></path><path d="M12 13v8"></path>',
        archive: '<path d="M21 8v13H3V8"></path><path d="M1 3h22v5H1Z"></path><path d="M10 12h4"></path>',
        cart: '<circle cx="9" cy="20" r="1"></circle><circle cx="17" cy="20" r="1"></circle><path d="M2 3h3l3 12h10l3-8H6"></path>',
        file: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"></path><path d="M14 2v6h6"></path><path d="M8 13h8"></path><path d="M8 17h6"></path>',
        chart: '<path d="M3 3v18h18"></path><path d="M8 17V9"></path><path d="M13 17V5"></path><path d="M18 17v-6"></path>',
        bag: '<path d="M6 8h12l-1 13H7L6 8Z"></path><path d="M9 8a3 3 0 0 1 6 0"></path>',
        dot: '<circle cx="12" cy="12" r="4"></circle>'
    };

    return `<svg class="vu-menu-icon" viewBox="0 0 24 24" aria-hidden="true">${icons[name] || icons.dot}</svg>`;
}
