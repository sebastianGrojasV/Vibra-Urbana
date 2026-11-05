(() => {
    const centimos = (value) => {
        const match = String(value || 0).match(/^(\d+)(?:\.(\d*))?$/);
        if (!match) return 0;
        const decimals = (match[2] || "").padEnd(3, "0");
        return Number(match[1]) * 100 + Number(decimals.slice(0, 2)) + (Number(decimals[2]) >= 5 ? 1 : 0);
    };

    const calcular = (items, descuento, efectivo) => {
        const subtotal = items.reduce((sum, item) => sum + centimos(item.precioUnitario) * item.cantidad, 0);
        const rebaja = Math.min(centimos(descuento), subtotal);
        const gravable = subtotal - rebaja;
        const impuesto = Math.floor((gravable * 13 + 50) / 100);
        const exacto = gravable + impuesto;
        const total = efectivo ? Math.floor((exacto + 500) / 1000) * 1000 : exacto;
        return {
            subtotal: subtotal / 100,
            descuento: rebaja / 100,
            gravable: gravable / 100,
            impuesto: impuesto / 100,
            ajuste: (total - exacto) / 100,
            total: total / 100
        };
    };

    window.VibraSaleTotals = { calcular };
})();
