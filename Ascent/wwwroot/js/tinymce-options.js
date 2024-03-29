var tinymceOptions = {};

tinymceOptions.standard = (selector, height) => ({
    selector: selector,
    plugins: "autolink code image link lists table wordcount fullscreen",
    menubar: false,
    convert_urls: false,
    browser_spellcheck: true,
    toolbar: "styles fontfamily forecolor backcolor bullist numlist indent outdent hr link image table code fullscreen",
    height: height ? height : 350
});

tinymceOptions.minimal = (selector, height) => ({
    selector: selector,
    plugins: "autolink code image link lists wordcount fullscreen",
    menubar: false,
    convert_urls: false,
    browser_spellcheck: true,
    toolbar: "bold italic underline bullist numlist link",
    height: height ? height : 200
});
