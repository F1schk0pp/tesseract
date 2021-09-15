using NUnit.Framework;

using System;
using System.IO;

namespace Tesseract.Tests
{
    [TestFixture]
    public class AnalyseResultTests : TesseractTestBase
    {
        private string ResultsDirectory => TestResultPath(@"Analysis\");

        private const string ExampleImagePath = @"Ocr\phototest.tif";

        #region Setup\TearDown

        private TesseractEngine? engine;

        [TearDown]
        public void Dispose()
        {
            if (engine != null)
            {
                engine.Dispose();
                engine = null;
            }
        }

        [SetUp]
        public void Init()
        {
            if (!Directory.Exists(ResultsDirectory))
            {
                Directory.CreateDirectory(ResultsDirectory);
            }

            engine = CreateEngine("osd");
        }

        #endregion Setup\TearDown

        #region Tests

        [Test]
        [TestCase(null)]
        [TestCase(90f)]
        [TestCase(180f)]
        public void AnalyseLayout_RotatedImage(float? angle)
        {
            _ = TestFilePath("Ocr/phototest.tif");
            using Pix img = LoadTestImage(ExampleImagePath);
            using Pix rotatedImage = angle.HasValue ? img.Rotate(MathHelper.ToRadians(angle.Value)) : img.Clone();
            rotatedImage.Save(TestResultRunFile(string.Format(@"AnalyseResult\AnalyseLayout_RotateImage_{0}.png", angle)));

            engine.DefaultPageSegMode = PageSegMode.AutoOsd;
            using Page page = engine.Process(rotatedImage);
            using ResultIterator pageLayout = page.GetIterator();
            pageLayout.Begin();
            do
            {
                ElementProperties result = pageLayout.GetProperties();

                ExpectedOrientation(angle ?? 0, out Orientation orient, out float deskew);
                Assert.That(result.Orientation, Is.EqualTo(orient));

                if (angle.HasValue)
                {
                    if (angle == 180f)
                    {
                        // This isn't correct...
                        Assert.That(result.WritingDirection, Is.EqualTo(WritingDirection.LeftToRight));
                        Assert.That(result.TextLineOrder, Is.EqualTo(TextLineOrder.TopToBottom));
                    }
                    else if (angle == 90f)
                    {
                        Assert.That(result.WritingDirection, Is.EqualTo(WritingDirection.LeftToRight));
                        Assert.That(result.TextLineOrder, Is.EqualTo(TextLineOrder.TopToBottom));
                    }
                    else
                    {
                        Assert.Fail("Angle not supported.");
                    }
                }
                else
                {
                    Assert.That(result.WritingDirection, Is.EqualTo(WritingDirection.LeftToRight));
                    Assert.That(result.TextLineOrder, Is.EqualTo(TextLineOrder.TopToBottom));
                }
            } while (pageLayout.Next(PageIteratorLevel.Block));
        }

        [Test]
        public void CanDetectOrientationForMode(
            [Values(PageSegMode.Auto,
                PageSegMode.AutoOnly,
                PageSegMode.AutoOsd,
                PageSegMode.CircleWord,
                PageSegMode.OsdOnly,
                PageSegMode.SingleBlock,
                PageSegMode.SingleBlockVertText,
                PageSegMode.SingleChar,
                PageSegMode.SingleColumn,
                PageSegMode.SingleLine,
                PageSegMode.SingleWord)]
            PageSegMode pageSegMode)
        {
            using Pix img = LoadTestImage(ExampleImagePath);
            using Pix rotatedPix = img.Rotate((float)Math.PI);
            using Page page = engine.Process(rotatedPix, pageSegMode);

            page.DetectBestOrientationAndScript(out int orientation, out float confidence, out string scriptName, out float scriptConfidence);

            Assert.That(orientation, Is.EqualTo(180));
            Assert.That(scriptName, Is.EqualTo("Latin"));
        }

        [Test]
        [TestCase(0)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        public void DetectOrientation_Degrees_RotatedImage(int expectedOrientation)
        {
            using Pix img = LoadTestImage(ExampleImagePath);
            using Pix rotatedPix = img.Rotate((float)expectedOrientation / 360 * (float)Math.PI * 2);
            using Page page = engine.Process(rotatedPix, PageSegMode.OsdOnly);

            page.DetectBestOrientationAndScript(out int orientation, out float confidence, out string scriptName, out float scriptConfidence);

            Assert.That(orientation, Is.EqualTo(expectedOrientation));
            Assert.That(scriptName, Is.EqualTo("Latin"));
        }

        [Test]
        [TestCase(0)]
        [TestCase(90)]
        [TestCase(180)]
        [TestCase(270)]
        public void DetectOrientation_Legacy_RotatedImage(int expectedOrientationDegrees)
        {
            using Pix img = LoadTestImage(ExampleImagePath);
            using Pix rotatedPix = img.Rotate((float)expectedOrientationDegrees / 360 * (float)Math.PI * 2);
            using Page page = engine.Process(rotatedPix, PageSegMode.OsdOnly);

            page.DetectBestOrientation(out Orientation orientation, out float confidence);

            ExpectedOrientation(expectedOrientationDegrees, out Orientation expectedOrientation, out float expectedDeskew);

            Assert.That(orientation, Is.EqualTo(expectedOrientation));
        }


        [Test]
        public void GetImage(
            [Values(PageIteratorLevel.Block, PageIteratorLevel.Para, PageIteratorLevel.TextLine, PageIteratorLevel.Word, PageIteratorLevel.Symbol)] PageIteratorLevel level,
            [Values(0, 3)] int padding)
        {
            using Pix img = LoadTestImage(ExampleImagePath);
            using Page page = engine.Process(img);
            using ResultIterator pageLayout = page.GetIterator();
            pageLayout.Begin();
            // get symbol
            using Pix elementImg = pageLayout.GetImage(level, padding, out int x, out int y);
            string elementImgFilename = string.Format(@"AnalyseResult\GetImage\ResultIterator_Image_{0}_{1}_at_({2},{3}).png", level, padding, x, y);

            // TODO: Ensure generated pix is equal to expected pix, only saving it if it's not.
            string destFilename = TestResultRunFile(elementImgFilename);
            elementImg.Save(destFilename, ImageFormat.Png);
        }

        #endregion Tests

        #region Helpers


        private void ExpectedOrientation(float rotation, out Orientation orientation, out float deskew)
        {
            rotation %= 360f;
            rotation = rotation < 0 ? rotation + 360 : rotation;

            if (rotation is >= 315 or < 45)
            {
                orientation = Orientation.PageUp;
                deskew = -rotation;
            }
            else if (rotation is >= 45 and < 135)
            {
                orientation = Orientation.PageRight;
                deskew = 90 - rotation;
            }
            else if (rotation is >= 135 and < 225)
            {
                orientation = Orientation.PageDown;
                deskew = 180 - rotation;
            }
            else if (rotation is >= 225 and < 315)
            {
                orientation = Orientation.PageLeft;
                deskew = 270 - rotation;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(rotation));
            }
        }

        private Pix LoadTestImage(string path)
        {
            string fullExampleImagePath = TestFilePath(path);
            return Pix.LoadFromFile(fullExampleImagePath);
        }

        #endregion
    }
}