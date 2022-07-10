/**
 * The SVG trick is from https://stackoverflow.com/questions/12698003/save-a-google-chart-as-svg
 *
 * @param {Chart} chart - The chart object
 * @param {string} chartName - The name of the chart. The assumptions are a) the id of the chart
 * container element is "<name>", and b) the id's of the <a> elements for downloading the SVG and
 * PNG images are "<name>Svg" and "<name>Png".
 */
function setImageLinks(chart, chartName) {
    return function () {
        $(`#${chartName}Png`).attr("href", chart.getImageURI());
        var e = document.getElementById(chartName);
        $(`#${chartName}Svg`).attr("href", "data:image/svg+xml;charset=utf-8," +
            encodeURIComponent(e.getElementsByTagName('svg')[0].outerHTML));
    };
}