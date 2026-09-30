(function () {
    const graficas = new Map();

    function horas(valor) {
        const minutos = Math.round(valor * 60);
        const h = Math.floor(minutos / 60);
        const m = minutos % 60;
        if (h === 0) return `${m} min`;
        if (m === 0) return `${h} h`;
        return `${h} h ${m} min`;
    }

    function limpiar(id) {
        const previa = graficas.get(id);
        if (previa) {
            previa.dispose();
            graficas.delete(id);
        }
    }

    function crear(id) {
        const el = document.getElementById(id);
        if (!el || !window.echarts) return null;
        limpiar(id);
        const chart = echarts.init(el);
        graficas.set(id, chart);
        return chart;
    }

    function dibujar() {
        const nodo = document.getElementById("datos-dashboard");
        if (!nodo) {
            limpiar("grafica-distribucion");
            limpiar("grafica-dias");
            return;
        }

        let datos;
        try {
            datos = JSON.parse(nodo.textContent || "{}");
        } catch {
            return;
        }

        const distribucion = crear("grafica-distribucion");
        if (distribucion) {
            distribucion.setOption({
                tooltip: {
                    trigger: "item",
                    formatter: (p) => `${p.name}<br/>${horas(p.value)} · ${p.data.porcentaje}%`,
                },
                series: [
                    {
                        type: "pie",
                        radius: ["48%", "78%"],
                        avoidLabelOverlap: true,
                        label: { show: false },
                        emphasis: { label: { show: false } },
                        data: datos.distribucion || [],
                    },
                ],
            });
        } else {
            limpiar("grafica-distribucion");
        }

        const dias = crear("grafica-dias");
        if (dias) {
            const filas = datos.dias || [];
            dias.setOption({
                color: ["#f47a20"],
                tooltip: {
                    trigger: "axis",
                    formatter: (params) => {
                        const p = params[0];
                        return `${p.name}<br/>${horas(p.value)}`;
                    },
                },
                grid: { left: 36, right: 12, top: 16, bottom: 28 },
                xAxis: {
                    type: "category",
                    data: filas.map((d) => d.etiqueta),
                    axisLabel: { interval: datos.vista === "mes" ? "auto" : 0 },
                },
                yAxis: { type: "value", axisLabel: { formatter: (v) => `${v} h` } },
                series: [
                    {
                        type: "bar",
                        data: filas.map((d) => d.horas),
                        barMaxWidth: 36,
                        itemStyle: { borderRadius: [4, 4, 0, 0] },
                    },
                ],
            });
        } else {
            limpiar("grafica-dias");
        }
    }

    window.addEventListener("resize", () => {
        graficas.forEach((g) => g.resize());
    });

    function contieneDatos(elt) {
        if (!elt || elt === document.body) return true;
        if (elt.id === "datos-dashboard") return true;
        return typeof elt.querySelector === "function" && !!elt.querySelector("#datos-dashboard");
    }

    if (window.htmx) {
        htmx.onLoad((elt) => {
            if (contieneDatos(elt)) dibujar();
        });
        htmx.on("htmx:afterSettle", (event) => {
            const elt = event.detail && event.detail.elt;
            if (!elt || elt.id !== "captura-formulario" || !elt.querySelector("[data-captura-exito]")) return;
            const tarea = elt.querySelector("#Tarea");
            if (tarea) tarea.focus();
        });
    } else {
        document.addEventListener("DOMContentLoaded", dibujar);
    }
})();
