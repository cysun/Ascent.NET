var summernoteOptions = {};

summernoteOptions.standard = {
    height: 200,
    callbacks: {
        // Fixed the BS5 dropdown list bug. See https://github.com/summernote/summernote/issues/4170
        onInit: function () {
            $("button[data-toggle='dropdown']").each(function (index) {
                $(this).removeAttr("data-toggle").attr("data-bs-toggle", "dropdown");
            });
        }
    },
    toolbar: [
        ['style', ['style']],
        ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
        ['fontname', ['fontname']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['table', ['table']],
        ['insert', ['link', 'picture', 'video', 'hr']],
        ['view', ['codeview', 'fullscreen']],
    ],
    styleTags: ["p",
        { tag: "div", title: "Normal (div)", value: "div" },
        { tag: "pre", title: "Code", value: "pre" },
        { tag: "blockquote", title: "Blockquote", className: "blockquote", value: "blockquote" },
        "h1", "h2", "h3", "h4"]
};

summernoteOptions.minimal = {
    height: 150,
    toolbar: [
        ['font', ['bold', 'italic', 'underline']],
        ['para', ['ul', 'ol']],
        ['view', ['codeview', 'fullscreen']]
    ]
};