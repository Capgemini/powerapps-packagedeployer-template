namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests
{
    using System.IO;
    using FluentAssertions;
    using NPOI.XWPF.UserModel;
    using Xunit;

    public class ZipFileUtilitiesTests
    {
        public ZipFileUtilitiesTests()
        {
        }

        [Fact]
        public void FindAndReplace_WordDocument_ReplacesText()
        {
            var documentPath = TestUtilities.GetResourcePath("Word Document.docx");

            ZipFileUtilities.FindAndReplace(documentPath, "Original", "Replacement");

            using (var fs = File.OpenRead(documentPath))
            {
                var doc = new XWPFDocument(fs);
                doc.Paragraphs[0].Text.Should().Be("Replacement");
            }
        }
    }
}
