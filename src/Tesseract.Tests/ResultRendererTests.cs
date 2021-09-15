using NUnit.Framework;

using System.Collections.Generic;
using System.IO;

namespace Tesseract.Tests
{
    [TestFixture]
    public class ResultRendererTests : TesseractTestBase
    {
        #region Test setup and teardown

        private TesseractEngine _engine;

        [SetUp]
        public void Inititialse() => _engine = CreateEngine();

        [TearDown]
        public void Dispose()
        {
            if (_engine != null)
            {
                _engine.Dispose();
                _engine = null;
            }
        }

        #endregion Test setup and teardown

        [Test]
        public void CanRenderResultsIntoTextFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\Text\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreateTextRenderer(resultPath))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "txt");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a Text file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoPdfFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\PDF\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessImageFile(renderer, examplePixPath);
            }

            Assert.That(File.Exists(expectedOutputFilename), $"Expected a PDF file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoPdfFile1()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\PDF\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessImageFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a PDF file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderMultiplePageDocumentToPdfFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\PDF\multi-page");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("processing/multi-page.tif");
                ProcessMultipageTiff(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("processing/multi-page.tif");
                ProcessImageFile(renderer, examplePixPath);
            }

            Assert.That(File.Exists(expectedOutputFilename), $"Expected a PDF file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderMultiplePageDocumentToPdfFile1()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\PDF\multi-page");
            using (IResultRenderer renderer = ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false))
            {
                string examplePixPath = TestFilePath("processing/multi-page.tif");
                ProcessImageFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a PDF file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoHOcrFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\HOCR\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreateHOcrRenderer(resultPath))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "hocr");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a HOCR file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoUnlvFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\UNLV\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreateUnlvRenderer(resultPath))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "unlv");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a Unlv file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoBoxFile()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\Box\phototest");
            using (IResultRenderer renderer = ResultRenderer.CreateBoxRenderer(resultPath))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "box");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a Box file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderResultsIntoMultipleOutputFormats()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\PDF\phototest");
            List<RenderedFormat> formats = new() { RenderedFormat.HOCR, RenderedFormat.PDF, RenderedFormat.TEXT };
            using (IResultRenderer renderer = ResultRenderer.CreateRenderers(resultPath, DataPath, formats))
            {
                string examplePixPath = TestFilePath("Ocr/phototest.tif");
                ProcessFile(renderer, examplePixPath);
            }

            string expectedOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a PDF file \"{expectedOutputFilename}\" to have been created; but none was found.");
            expectedOutputFilename = Path.ChangeExtension(resultPath, "hocr");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a HOCR file \"{expectedOutputFilename}\" to have been created; but none was found.");
            expectedOutputFilename = Path.ChangeExtension(resultPath, "txt");
            Assert.That(File.Exists(expectedOutputFilename), $"Expected a TEXT file \"{expectedOutputFilename}\" to have been created; but none was found.");
        }

        [Test]
        public void CanRenderMultiplePageDocumentIntoMultipleResultRenderers()
        {
            string resultPath = TestResultRunFile(@"ResultRenderers\Aggregate\multi-page");
            using (AggregateResultRenderer renderer = new(ResultRenderer.CreatePdfRenderer(resultPath, DataPath, false), ResultRenderer.CreateTextRenderer(resultPath)))
            {
                string examplePixPath = TestFilePath("processing/multi-page.tif");
                ProcessMultipageTiff(renderer, examplePixPath);
            }

            string expectedPdfOutputFilename = Path.ChangeExtension(resultPath, "pdf");
            Assert.That(File.Exists(expectedPdfOutputFilename), $"Expected a PDF file \"{expectedPdfOutputFilename}\" to have been created; but none was found.");

            string expectedTxtOutputFilename = Path.ChangeExtension(resultPath, "txt");
            Assert.That(File.Exists(expectedTxtOutputFilename), $"Expected a Text file \"{expectedTxtOutputFilename}\" to have been created; but none was found.");
        }

        private void ProcessMultipageTiff(IResultRenderer renderer, string filename)
        {
            string imageName = Path.GetFileNameWithoutExtension(filename);
            using PixArray pixA = PixArray.LoadMultiPageTiffFromFile(filename);
            int expectedPageNumber = -1;
            using (renderer.BeginDocument(imageName))
            {
                Assert.AreEqual(renderer.PageNumber, expectedPageNumber);
                foreach (Pix pix in pixA)
                {
                    using Page page = _engine.Process(pix, imageName);
                    bool addedPage = renderer.AddPage(page);
                    expectedPageNumber++;

                    Assert.That(addedPage, Is.True);
                    Assert.That(renderer.PageNumber, Is.EqualTo(expectedPageNumber));
                }
            }

            Assert.That(renderer.PageNumber, Is.EqualTo(expectedPageNumber));
        }

        private void ProcessFile(IResultRenderer renderer, string filename)
        {
            string imageName = Path.GetFileNameWithoutExtension(filename);
            using Pix pix = Pix.LoadFromFile(filename);
            using (renderer.BeginDocument(imageName))
            {
                Assert.AreEqual(renderer.PageNumber, -1);
                using Page page = _engine.Process(pix, imageName);
                bool addedPage = renderer.AddPage(page);

                Assert.That(addedPage, Is.True);
                Assert.That(renderer.PageNumber, Is.EqualTo(0));
            }

            Assert.AreEqual(renderer.PageNumber, 0);
        }

        private void ProcessImageFile(IResultRenderer renderer, string filename)
        {
            string imageName = Path.GetFileNameWithoutExtension(filename);
            using PixArray pixA = ReadImageFileIntoPixArray(filename);
            int expectedPageNumber = -1;
            using (renderer.BeginDocument(imageName))
            {
                Assert.AreEqual(renderer.PageNumber, expectedPageNumber);
                foreach (Pix pix in pixA)
                {
                    using Page page = _engine.Process(pix, imageName);
                    bool addedPage = renderer.AddPage(page);
                    expectedPageNumber++;

                    Assert.That(addedPage, Is.True);
                    Assert.That(renderer.PageNumber, Is.EqualTo(expectedPageNumber));
                }
            }

            Assert.That(renderer.PageNumber, Is.EqualTo(expectedPageNumber));
        }

        private PixArray ReadImageFileIntoPixArray(string filename)
        {
            if (filename.ToLower().EndsWith(".tif") || filename.ToLower().EndsWith(".tiff"))
            {
                return PixArray.LoadMultiPageTiffFromFile(filename);
            }
            else
            {
                PixArray pa = PixArray.Create(0);
                pa.Add(Pix.LoadFromFile(filename));
                return pa;
            }
        }
    }
}