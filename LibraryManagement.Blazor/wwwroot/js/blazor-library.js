window.libraryManagement = window.libraryManagement || {};

window.libraryManagement.downloadFileFromBase64 = function (fileName, contentType, base64) {
    const link = document.createElement("a");
    link.href = `data:${contentType};base64,${base64}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.libraryManagement.dashboard = (() => {
    let performanceChart;
    let borrowChart;
    let categoryChart;
    let statusChart;

    function destroy(chart) {
        if (chart) {
            chart.destroy();
        }
    }

    function render(model) {
        if (typeof Chart === "undefined") {
            return;
        }

        Chart.defaults.color = "rgba(255, 255, 255, 0.4)";
        Chart.defaults.font.family = "'Inter', sans-serif";

        destroy(performanceChart);
        destroy(borrowChart);
        destroy(categoryChart);
        destroy(statusChart);

        const performanceCanvas = document.getElementById("performanceChart");
        const borrowCanvas = document.getElementById("borrowChart");
        const categoryCanvas = document.getElementById("categoryChart");
        const statusCanvas = document.getElementById("statusChart");

        if (!performanceCanvas || !borrowCanvas || !categoryCanvas || !statusCanvas) {
            return;
        }

        performanceChart = new Chart(performanceCanvas.getContext("2d"), {
            type: "bar",
            data: {
                labels: model.monthlyLabels,
                datasets: [
                    {
                        label: "New Members",
                        data: model.monthlyRegistrations,
                        backgroundColor: "rgba(234, 179, 8, 0.4)",
                        borderColor: "#eab308",
                        borderWidth: 2,
                        borderRadius: 12,
                        yAxisID: "yUsers"
                    },
                    {
                        label: "Borrowing Activity",
                        data: model.monthlyBorrows,
                        type: "line",
                        borderColor: "#eab308",
                        backgroundColor: "transparent",
                        borderWidth: 4,
                        pointBackgroundColor: "#eab308",
                        pointRadius: 6,
                        tension: 0.4,
                        yAxisID: "yActivity"
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: "top", align: "end" },
                    tooltip: { backgroundColor: "rgba(0,0,0,0.8)", padding: 12 }
                },
                scales: {
                    yActivity: {
                        type: "linear",
                        position: "left",
                        grid: { color: "rgba(255,255,255,0.05)" },
                        ticks: { precision: 0 }
                    },
                    yUsers: {
                        type: "linear",
                        position: "right",
                        grid: { display: false },
                        ticks: { precision: 0 }
                    },
                    x: { grid: { display: false } }
                }
            }
        });

        borrowChart = new Chart(borrowCanvas.getContext("2d"), {
            type: "line",
            data: {
                labels: model.monthlyLabels,
                datasets: [
                    {
                        label: "Borrows",
                        data: model.monthlyBorrows,
                        borderColor: "#eab308",
                        backgroundColor: "rgba(234, 179, 8, 0.1)",
                        fill: true,
                        tension: 0.4,
                        borderWidth: 3,
                        pointRadius: 4
                    },
                    {
                        label: "Returns",
                        data: model.monthlyReturns,
                        borderColor: "#10b981",
                        backgroundColor: "rgba(16, 185, 129, 0.1)",
                        fill: true,
                        tension: 0.4,
                        borderWidth: 3,
                        pointRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: "top", align: "end" } },
                scales: {
                    y: { grid: { color: "rgba(255,255,255,0.05)" }, ticks: { precision: 0 } },
                    x: { grid: { display: false } }
                }
            }
        });

        categoryChart = new Chart(categoryCanvas.getContext("2d"), {
            type: "doughnut",
            data: {
                labels: model.categoryLabels,
                datasets: [{
                    data: model.categoryCounts,
                backgroundColor: ["#eab308", "#f59e0b", "#10b981", "#a855f7", "#ec4899", "#f97316", "#14b8a6", "#64748b"],
                    borderWidth: 2,
                    borderColor: "#1e293b",
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "70%",
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            pointStyle: "circle"
                        }
                    },
                    tooltip: { backgroundColor: "rgba(0,0,0,0.8)", padding: 12, cornerRadius: 8 }
                }
            }
        });

        const statusColors = ["#10b981", "#eab308", "#f59e0b", "#f43f5e", "#ef4444"];
        statusChart = new Chart(statusCanvas.getContext("2d"), {
            type: "doughnut",
            data: {
                labels: model.bookStatusLabels,
                datasets: [{
                    data: model.bookStatusCounts,
                    backgroundColor: statusColors.slice(0, model.bookStatusLabels.length),
                    borderWidth: 2,
                    borderColor: "#1e293b",
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "70%",
                plugins: {
                    legend: {
                        position: "bottom",
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            pointStyle: "circle"
                        }
                    },
                    tooltip: { backgroundColor: "rgba(0,0,0,0.8)", padding: 12, cornerRadius: 8 }
                }
            }
        });
    }

    return { render };
})();
