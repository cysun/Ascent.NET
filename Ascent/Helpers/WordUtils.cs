using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Ascent.Helpers;

public static class WordUtils
{
    public static void AddHeading1(Body body, string text) =>
        body.Append(new Paragraph(
            new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" }),
            new Run(new Text(text))
        ));

    public static void AddParagraph(Body body, string text) =>
        body.Append(new Paragraph(new Run(new Text(text))));

    public static void AddLinkedText(MainDocumentPart mainPart, string text, string url)
    {
        var relId = mainPart.AddHyperlinkRelationship(new Uri(url), true).Id;
        var run = new Run(
            new RunProperties(
                new Color() { Val = "0563C1" },
                new Underline() { Val = UnderlineValues.Single }
            ),
            new Text(text)
        );
        var hyperlink = new Hyperlink(run) { Id = relId };
        mainPart.Document!.Body!.Append(new Paragraph(hyperlink));
    }

    public static void AddStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles();

        // Normal style is required as the base
        var normalStyle = new Style { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
        normalStyle.Append(new StyleName { Val = "Normal" });
        stylesPart.Styles.Append(normalStyle);

        // Heading 1 style
        var heading1 = new Style { Type = StyleValues.Paragraph, StyleId = "Heading1" };
        heading1.Append(new StyleName { Val = "heading 1" });
        heading1.Append(new BasedOn { Val = "Normal" });
        heading1.Append(new NextParagraphStyle { Val = "Normal" });

        var rPr = new StyleRunProperties();
        rPr.Append(new Bold());
        rPr.Append(new Color { Val = "2F5496" });
        rPr.Append(new FontSize { Val = "32" }); // 16 pt
        rPr.Append(new FontSizeComplexScript { Val = "32" });
        heading1.Append(rPr);

        stylesPart.Styles.Append(heading1);
        stylesPart.Styles.Save();
    }
}
