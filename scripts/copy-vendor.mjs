import { cpSync, mkdirSync } from "node:fs";
import { dirname } from "node:path";

const copies = [
  ["node_modules/alpinejs/dist/cdn.min.js", "wwwroot/lib/alpine/alpine.min.js"],
  ["node_modules/htmx.org/dist/htmx.min.js", "wwwroot/lib/htmx/htmx.min.js"],
  ["node_modules/echarts/dist/echarts.min.js", "wwwroot/lib/echarts/echarts.min.js"],
  ["node_modules/tabulator-tables/dist/js/tabulator.min.js", "wwwroot/lib/tabulator/tabulator.min.js"],
  ["node_modules/tabulator-tables/dist/css/tabulator.min.css", "wwwroot/lib/tabulator/tabulator.min.css"],
];

for (const [from, to] of copies) {
  mkdirSync(dirname(to), { recursive: true });
  cpSync(from, to);
}
