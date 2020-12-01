using System;
using System.IO;
using NPOI.XWPF.UserModel;
using Xunit;
using FluentAssertions;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests
{
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
