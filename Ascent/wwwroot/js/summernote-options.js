var summernoteOptions = {};

summernoteOptions.standard = (height) => ({
    height: height ? height : 350,
    toolbar: [
        ['style', ['style']],
        ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
        ['fontname', ['fontname']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['table', ['table']],
        ['insert', ['link', 'picture', 'video', 'hr']],
        ['view', ['fullscreen']]
    ]
});

summernoteOptions.minimal = (height) => ({
    height: height ? height : 200,
    toolbar: [
        ['font', ['bold', 'italic', 'underline']],
        ['para', ['ul', 'ol']],
        ['insert', ['link']]
    ]
});

// NOTE: The full option adds code view and a format button. The format button uses Prettier and
// requires the two Pretter scripts to be included in the page.
summernoteOptions.full = (selector, height) => ({
    height: height ? height : 600,
    toolbar: [
        ['style', ['style']],
        ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
        ['fontname', ['fontname']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['table', ['table']],
        ['insert', ['link', 'picture', 'video', 'hr']],
        ['view', ['format', 'codeview', 'fullscreen']]
    ],
    buttons: {
        format: function () {
            var ui = $.summernote.ui;
            var button = ui.button({
                contents: "<i class='bi bi-card-text'></i>",
                tooltip: "Format HTML",
                click: async function () {
                    if (prettier && prettierPlugins) {
                        let content = await prettier.format($(selector).summernote("code"), {
                            parser: "html",
                            plugins: prettierPlugins
                        });
                        $(selector).summernote("code", content);
                    }
                }
            });
            return button.render();
        }
    }
});
