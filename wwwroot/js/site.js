
window.addEventListener("DOMContentLoaded", () => {
    enhanceSidebar();
    enhanceModulePlaceholders();
    setupInlineConfirmations();
    setupSaleCancellation();
    setupSaleStatusChange();
    setupAutoDismissAlerts();
    setupCatalogFilters();
    setupCatalogCartButtons();
    setupCartPage();
    setupCheckoutButton();
    setupOrderConfirmationPage();
    updateCartNavigationCount();

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
    const observationWrap = frame.querySelector("[data-vu-confirm-observation-wrap]");
    const observationLabel = frame.querySelector("[data-vu-confirm-observation-label]");
    const observationInput = frame.querySelector("[data-vu-confirm-observation]");
    const observationError = frame.querySelector("[data-vu-confirm-observation-error]");
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
            const needsObservation = form.dataset.vuConfirmObservation === "true";

            if (observationWrap && observationInput && observationLabel && observationError) {
                observationWrap.hidden = !needsObservation;
                observationInput.value = "";
                observationLabel.textContent = form.dataset.vuConfirmObservationLabel || "Observación";
                observationInput.placeholder = form.dataset.vuConfirmObservationPlaceholder || "";
                observationInput.required = form.dataset.vuConfirmObservationRequired === "true";
                observationError.hidden = true;
            }

            frame.hidden = false;
            frame.classList.add("is-visible");
            if (needsObservation && observationInput) {
                observationInput.focus();
            } else {
                submit.focus();
            }
        });
    });

    submit.addEventListener("click", () => {
        if (!pendingForm) {
            return;
        }

        if (observationInput && observationInput.required && !observationInput.value.trim()) {
            if (observationError) {
                observationError.hidden = false;
            }

            observationInput.focus();
            return;
        }

        if (observationInput) {
            let formObservation = pendingForm.querySelector('input[name="Observacion"]');

            if (!formObservation) {
                formObservation = document.createElement("input");
                formObservation.type = "hidden";
                formObservation.name = "Observacion";
                pendingForm.appendChild(formObservation);
            }

            formObservation.value = observationInput.value.trim();
        }

        pendingForm.dataset.vuConfirmed = "true";
        pendingForm.submit();
    });

    cancel.addEventListener("click", () => {
        pendingForm = null;
        if (observationInput) {
            observationInput.value = "";
            observationInput.required = false;
        }

        if (observationError) {
            observationError.hidden = true;
        }

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
            const price = Number(button.dataset.vuProductPrice || "0");

            if (!productId || stock <= 0) {
                showCartFeedback(feedback, "Este producto no tiene disponibilidad por el momento.", "error");
                return;
            }

            const storageKey = "vibra.catalogCart";
            const current = readCatalogCart();
            const item = current.find((product) => product.id === productId);

            if (item) {
                if (Number(item.quantity || 0) >= stock) {
                    showCartFeedback(feedback, `No hay más disponibilidad. Stock actual: ${stock}.`, "error");
                    return;
                }

                item.quantity = Number(item.quantity || 0) + 1;
                item.stock = stock;
                item.price = price;
                item.subtotal = item.price * item.quantity;
            } else {
                current.push({
                    id: productId,
                    name: button.dataset.vuProductName || "Producto",
                    price,
                    quantity: 1,
                    subtotal: price,
                    stock,
                    image: button.dataset.vuProductImage || "",
                    category: button.dataset.vuProductCategory || "",
                    size: button.dataset.vuProductSize || "",
                    color: button.dataset.vuProductColor || ""
                });
            }

            localStorage.setItem(storageKey, JSON.stringify(current));
            updateCartNavigationCount();

            if (feedback) {
                showCartFeedback(feedback, 'Producto agregado al carrito. <a href="/Carrito">Ver carrito</a>', "success");
            }
        });
    });
}

function setupCartPage() {
    const itemsContainer = document.querySelector("[data-vu-cart-items]");
    const emptyState = document.querySelector("[data-vu-cart-empty]");
    const form = document.querySelector("[data-vu-order-form]");

    if (!itemsContainer || !emptyState) {
        return;
    }

    const renderCart = () => {
        const cart = readCatalogCart();
        const totals = calculateCartTotals(cart);
        const countLabel = document.querySelector("[data-vu-cart-count-label]");
        const subtotal = document.querySelector("[data-vu-cart-subtotal]");
        const tax = document.querySelector("[data-vu-cart-tax]");
        const total = document.querySelector("[data-vu-cart-total]");

        emptyState.hidden = cart.length > 0;
        itemsContainer.hidden = cart.length === 0;
        itemsContainer.innerHTML = cart.map(renderCartItem).join("");

        if (countLabel) {
            countLabel.textContent = `${totals.quantity} artículo${totals.quantity === 1 ? "" : "s"}`;
        }

        if (subtotal) {
            subtotal.textContent = formatCurrency(totals.subtotal);
        }

        if (tax) {
            tax.textContent = formatCurrency(totals.tax);
        }

        if (total) {
            total.textContent = formatCurrency(totals.total);
        }

        if (form) {
            form.hidden = cart.length === 0;
        }

        updateCartNavigationCount();
    };

    itemsContainer.addEventListener("input", (event) => {
        const input = event.target.closest("[data-vu-cart-quantity]");

        if (!input) {
            return;
        }

        const cart = readCatalogCart();
        const item = cart.find((product) => product.id === input.dataset.vuProductId);

        if (!item) {
            return;
        }

        const stock = Number(item.stock || input.max || "1");
        item.quantity = Math.max(1, Math.min(Number(input.value || "1"), stock));
        item.subtotal = Number(item.price || 0) * item.quantity;
        writeCatalogCart(cart);
        renderCart();
    });

    itemsContainer.addEventListener("click", (event) => {
        const remove = event.target.closest("[data-vu-remove-cart]");

        if (!remove) {
            return;
        }

        const cart = readCatalogCart().filter((product) => product.id !== remove.dataset.vuProductId);
        writeCatalogCart(cart);
        renderCart();
    });

    form?.addEventListener("submit", (event) => {
        event.preventDefault();
        const cart = readCatalogCart();
        const message = document.querySelector("[data-vu-order-message]");

        if (cart.length === 0) {
            showOrderMessage(message, "Agrega al menos un producto para confirmar el pedido.");
            return;
        }

        clearOrderFieldMessages(form);

        if (!form.checkValidity()) {
            showOrderValidationMessages(form, message);
            form.reportValidity();
            return;
        }

        const data = Object.fromEntries(new FormData(form).entries());
        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
        const endpoint = form.dataset.vuOrderEndpoint || "/Carrito/RegistrarPedido";

        submitOnlineOrder(endpoint, token, data, cart, message, form);
    });

    setupOrderClientLookup(form);
    setupOrderFieldValidation(form);
    renderCart();
}

function setupOrderFieldValidation(form) {
    if (!form) {
        return;
    }

    form.querySelectorAll("input, textarea, select").forEach((field) => {
        field.addEventListener("input", () => {
            if (field.checkValidity()) {
                clearOrderFieldMessage(field);
            }
        });

        field.addEventListener("blur", () => {
            if (!field.checkValidity()) {
                showOrderFieldMessage(field, getFriendlyFieldMessage(field));
            }
        });
    });
}

function setupOrderClientLookup(form) {
    if (!form) {
        return;
    }

    const endpoint = form.dataset.vuClientLookupEndpoint;
    const cedulaInput = form.querySelector('[name="cedulaCliente"]');
    const status = form.querySelector("[data-vu-client-lookup-status]");
    let lookupTimer = null;
    let lastLookupValue = "";

    if (!endpoint || !cedulaInput) {
        return;
    }

    cedulaInput.addEventListener("input", () => {
        const cedula = cedulaInput.value.replace(/\D/g, "").slice(0, 9);
        cedulaInput.value = cedula;

        window.clearTimeout(lookupTimer);

        if (cedula.length !== 9) {
            updateClientLookupStatus(status, "", "neutral");
            return;
        }

        lookupTimer = window.setTimeout(async () => {
            lastLookupValue = cedula;
            updateClientLookupStatus(status, "Buscando datos del cliente...", "neutral");

            try {
                const response = await fetch(`${endpoint}?cedula=${encodeURIComponent(cedula)}`, {
                    headers: {
                        "Accept": "application/json"
                    }
                });

                const result = await response.json();

                if (lastLookupValue !== cedulaInput.value) {
                    return;
                }

                if (!response.ok) {
                    updateClientLookupStatus(status, result.mensaje || "Revisa la cédula ingresada.", "warning");
                    return;
                }

                const encontrado = result.encontrado ?? result.Encontrado;
                const cliente = result.cliente ?? result.Cliente;

                if (!encontrado || !cliente) {
                    updateClientLookupStatus(status, "No encontramos datos previos. Puedes continuar con el registro.", "neutral");
                    return;
                }

                fillOrderClientData(form, cliente);
                updateClientLookupStatus(status, "Datos encontrados y completados automáticamente.", "success");
            } catch {
                updateClientLookupStatus(status, "No se pudo consultar la cédula en este momento.", "warning");
            }
        }, 350);
    });
}

function fillOrderClientData(form, cliente) {
    const values = {
        nombreCliente: cliente.nombreCompleto ?? cliente.NombreCompleto ?? "",
        telefonoCliente: cliente.telefono ?? cliente.Telefono ?? "",
        correoCliente: cliente.correo ?? cliente.Correo ?? "",
        direccionEntrega: cliente.direccion ?? cliente.Direccion ?? ""
    };

    Object.entries(values).forEach(([name, value]) => {
        const field = form.querySelector(`[name="${name}"]`);

        if (field && value) {
            field.value = value;
        }
    });
}

function updateClientLookupStatus(status, message, tone) {
    if (!status) {
        return;
    }

    if (!message) {
        status.hidden = true;
        status.textContent = "";
        status.dataset.tone = "neutral";
        return;
    }

    status.hidden = false;
    status.textContent = message;
    status.dataset.tone = tone || "neutral";
}

function setupCheckoutButton() {
    const button = document.querySelector("[data-vu-checkout]");

    if (!button) {
        return;
    }

    button.addEventListener("click", () => {
        const cart = readCatalogCart();
        const message = document.querySelector("[data-vu-checkout-message]");

        if (cart.length === 0) {
            showOrderMessage(message, "Debes agregar productos al carrito antes de finalizar la compra.");
            return;
        }

        window.location.href = "/Carrito/Pedido";
    });
}

async function submitOnlineOrder(endpoint, token, data, cart, message, form) {
    const submit = form.querySelector('button[type="submit"]');
    const originalText = submit?.textContent || "Confirmar pedido";
    const payload = {
        nombreCliente: data.nombreCliente,
        cedulaCliente: data.cedulaCliente,
        telefonoCliente: data.telefonoCliente,
        correoCliente: data.correoCliente,
        direccionEntrega: data.direccionEntrega,
        referenciaPago: data.referenciaPago,
        observacionPedido: data.observacionPedido,
        items: cart.map((item) => ({
            productoId: Number(item.id),
            cantidad: Number(item.quantity)
        }))
    };

    if (submit) {
        submit.disabled = true;
        submit.textContent = "Registrando...";
    }

    showOrderMessage(message, "Estamos registrando tu pedido. Espera un momento.", "info");

    try {
        const response = await fetch(endpoint, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify(payload)
        });
        const result = await response.json();

        const isSuccessful = result.esExitoso ?? result.EsExitoso;
        const resultMessage = result.mensaje ?? result.Mensaje;
        const resultCode = result.codigo ?? result.Codigo;
        const resultTotal = result.total ?? result.Total;

        if (!response.ok || !isSuccessful) {
            showOrderMessage(message, resultMessage || "No se pudo registrar el pedido. Revisa los datos e inténtalo nuevamente.", "error");
            return;
        }

        localStorage.setItem("vibra.pendingOrder", JSON.stringify({
            number: resultCode,
            status: "Pendiente de verificación",
            customer: data,
            items: cart,
            totals: {
                total: resultTotal
            },
            createdAt: new Date().toISOString()
        }));
        localStorage.removeItem("vibra.catalogCart");
        updateCartNavigationCount();
        window.location.href = "/Carrito/Confirmacion";
    } catch {
        showOrderMessage(message, "No se pudo conectar con el servidor. Revisa tu conexión e inténtalo nuevamente.", "error");
    } finally {
        if (submit) {
            submit.disabled = false;
            submit.textContent = originalText;
        }
    }
}

function setupOrderConfirmationPage() {
    const page = document.querySelector("[data-vu-order-confirmation]");

    if (!page) {
        return;
    }

    const order = JSON.parse(localStorage.getItem("vibra.pendingOrder") || "null");

    if (!order) {
        return;
    }

    const number = page.querySelector("[data-vu-confirm-order-number]");
    const total = page.querySelector("[data-vu-confirm-order-total]");
    const customer = page.querySelector("[data-vu-confirm-order-customer]");

    if (number) {
        number.textContent = order.number || "VU-PENDIENTE";
    }

    if (total) {
        total.textContent = formatCurrency(order.totals?.total || 0);
    }

    if (customer) {
        customer.textContent = order.customer?.nombreCliente || "Sin datos";
    }
}

function renderCartItem(item) {
    const image = item.image
        ? `<img src="${escapeHtml(item.image)}" alt="${escapeHtml(item.name)}">`
        : `<div class="vu-cart-item-fallback">${escapeHtml((item.name || "VU").slice(0, 2).toUpperCase())}</div>`;
    const stock = Number(item.stock || 1);
    const quantity = Math.max(1, Math.min(Number(item.quantity || 1), stock));

    return `
        <article class="vu-cart-item">
            <div class="vu-cart-item-media">${image}</div>
            <div class="vu-cart-item-body">
                <div>
                    <span>${escapeHtml(item.category || "Producto")}</span>
                    <h3>${escapeHtml(item.name || "Producto")}</h3>
                    <p>Talla ${escapeHtml(item.size || "N/D")} · ${escapeHtml(item.color || "N/D")}</p>
                </div>
                <strong>${formatCurrency(Number(item.price || 0))}</strong>
            </div>
            <div class="vu-cart-item-controls">
                <label for="cantidad-${escapeHtml(item.id)}">Cantidad</label>
                <input id="cantidad-${escapeHtml(item.id)}" class="form-control" type="number" min="1" max="${stock}" value="${quantity}" data-vu-cart-quantity data-vu-product-id="${escapeHtml(item.id)}">
                <button class="btn btn-outline-danger" type="button" data-vu-remove-cart data-vu-product-id="${escapeHtml(item.id)}">Eliminar</button>
            </div>
            <div class="vu-cart-item-subtotal">${formatCurrency(Number(item.price || 0) * quantity)}</div>
        </article>`;
}

function readCatalogCart() {
    try {
        return JSON.parse(localStorage.getItem("vibra.catalogCart") || "[]")
            .filter((item) => item && item.id && Number(item.stock || 0) > 0)
            .map((item) => {
                const stock = Number(item.stock || 1);
                const quantity = Math.max(1, Math.min(Number(item.quantity || 1), stock));
                const price = Number(item.price || 0);

                return {
                    ...item,
                    price,
                    quantity,
                    stock,
                    subtotal: price * quantity
                };
            });
    } catch {
        return [];
    }
}

function writeCatalogCart(cart) {
    const normalized = cart.map((item) => {
        const stock = Number(item.stock || 1);
        const quantity = Math.max(1, Math.min(Number(item.quantity || 1), stock));
        const price = Number(item.price || 0);

        return {
            ...item,
            price,
            quantity,
            stock,
            subtotal: price * quantity
        };
    });

    localStorage.setItem("vibra.catalogCart", JSON.stringify(normalized));
}

function calculateCartTotals(cart) {
    const subtotal = cart.reduce((sum, item) => sum + Number(item.subtotal || Number(item.price || 0) * Number(item.quantity || 0)), 0);
    const quantity = cart.reduce((sum, item) => sum + Number(item.quantity || 0), 0);
    const tax = subtotal * 0.13;

    return {
        subtotal,
        tax,
        total: subtotal + tax,
        quantity
    };
}

function updateCartNavigationCount() {
    const count = document.querySelector("[data-vu-cart-count]");

    if (!count) {
        return;
    }

    const total = readCatalogCart().reduce((sum, item) => sum + Number(item.quantity || 0), 0);
    count.textContent = total.toString();
    count.hidden = total === 0;
}

function formatCurrency(value) {
    return new Intl.NumberFormat("es-CR", {
        style: "currency",
        currency: "CRC",
        maximumFractionDigits: 0
    }).format(value || 0);
}

function showOrderValidationMessages(form, message) {
    const invalidFields = Array.from(form.querySelectorAll("input, textarea, select"))
        .filter((field) => !field.checkValidity());

    invalidFields.forEach((field) => {
        showOrderFieldMessage(field, getFriendlyFieldMessage(field));
    });

    const list = invalidFields
        .map((field) => `<li>${escapeHtml(getFieldLabel(field))}: ${escapeHtml(getFriendlyFieldMessage(field))}</li>`)
        .join("");

    showOrderMessage(
        message,
        `<strong>Revisa los datos del pedido.</strong><ul>${list}</ul>`,
        "error",
        true);
}

function getFieldLabel(field) {
    const label = field.id ? document.querySelector(`label[for="${field.id}"]`) : null;
    return label?.textContent?.trim() || field.name || "Campo";
}

function getFriendlyFieldMessage(field) {
    const label = getFieldLabel(field);

    if (field.validity.valueMissing) {
        return `${label} es obligatorio.`;
    }

    if (field.validity.patternMismatch) {
        if (field.name === "cedulaCliente") {
            return "La cédula debe tener 9 dígitos numéricos.";
        }

        if (field.name === "telefonoCliente") {
            return "El teléfono debe tener 8 dígitos numéricos.";
        }
    }

    if (field.validity.typeMismatch && field.type === "email") {
        return "Ingresa un correo válido, por ejemplo nombre@correo.com.";
    }

    if (field.validity.tooLong) {
        return `${label} supera la longitud permitida.`;
    }

    return field.validationMessage || `Revisa el campo ${label}.`;
}

function showOrderFieldMessage(field, text) {
    if (!field) {
        return;
    }

    field.classList.add("is-invalid");
    field.setAttribute("aria-invalid", "true");

    let feedback = field.parentElement?.querySelector(`[data-vu-field-error="${field.name}"]`);

    if (!feedback) {
        feedback = document.createElement("div");
        feedback.className = "vu-field-error";
        feedback.dataset.vuFieldError = field.name;
        field.insertAdjacentElement("afterend", feedback);
    }

    feedback.textContent = text;
}

function clearOrderFieldMessage(field) {
    if (!field) {
        return;
    }

    field.classList.remove("is-invalid");
    field.removeAttribute("aria-invalid");
    field.parentElement?.querySelector(`[data-vu-field-error="${field.name}"]`)?.remove();
}

function clearOrderFieldMessages(form) {
    if (!form) {
        return;
    }

    form.querySelectorAll("input, textarea, select").forEach(clearOrderFieldMessage);
}

function showOrderMessage(message, text, type = "error", allowHtml = false) {
    if (!message) {
        return;
    }

    message.hidden = false;
    message.classList.toggle("is-success", type === "success");
    message.classList.toggle("is-info", type === "info");
    message.classList.toggle("is-error", type === "error");
    message.setAttribute("role", "alert");

    if (allowHtml) {
        message.innerHTML = text;
    } else {
        message.textContent = text;
    }
}

function showCartFeedback(feedback, message, type) {
    if (!feedback) {
        return;
    }

    feedback.hidden = false;
    feedback.classList.toggle("is-success", type === "success");
    feedback.classList.toggle("is-error", type === "error");
    feedback.setAttribute("role", "alert");
    feedback.innerHTML = message;
    window.setTimeout(() => {
        feedback.hidden = true;
        feedback.classList.remove("is-success");
        feedback.classList.remove("is-error");
    }, 3000);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
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
