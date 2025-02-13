﻿using NUnit.Framework;

using System;
using System.IO;

namespace Tesseract.Tests
{
    /// <summary>
    /// Determines what action is taken when the test result doesn't match the expected (reference) result.
    /// </summary>
    public interface ITestDifferenceHandler
    {
        void Execute(string actualResultFilename, string expectedResultFilename);
    }

    /// <summary>
    /// Fails the test if the actual result file doesn't match the expected result (ignoring line ending type(s)).
    /// </summary>
    public class FailTestDifferenceHandler : ITestDifferenceHandler
    {
        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            if (File.Exists(expectedResultFilename))
            {
                string actualResult = TestUtils.NormaliseNewLine(File.ReadAllText(actualResultFilename));
                string expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                if (expectedResult != actualResult)
                {
                    Assert.Fail("Expected results to be \"{0}\" but was \"{1}\".", expectedResultFilename, actualResultFilename);
                }
            }
            else
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
            }
        }
    }

    /// <summary>
    /// Launches P4Merge to allow the user to update the reference if required
    /// 
    /// This is handy when updating the underlying tesseract engine dlls and/or tessdata as the output of a number of tests may 
    /// have changed due to internal changes in tesseract.
    /// </summary>
    public class P4MergeTestDifferenceHandler : ITestDifferenceHandler
    {
        public void Execute(string actualResultFilename, string expectedResultFilename)
        {
            string actualResult = TestUtils.NormaliseNewLine(File.ReadAllText(actualResultFilename));

            if (File.Exists(expectedResultFilename))
            {
                // Load the expected results and verify that they match
                string expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                if (expectedResult != actualResult)
                {
                    Console.WriteLine($"Expected results to be \"{expectedResultFilename}\" but was \"{actualResultFilename}\", launching merge tool.");
                    Merge(actualResultFilename, expectedResultFilename);

                    // User may have updated expected results, only fail if it's still different
                    expectedResult = TestUtils.NormaliseNewLine(File.ReadAllText(expectedResultFilename));
                    if (expectedResult != actualResult)
                    {
                        Assert.Fail("Expected results to be \"{0}\" but was \"{1}\".", expectedResultFilename, actualResultFilename);
                    }
                }
            }
            else
            {
                File.Copy(actualResultFilename, expectedResultFilename);
                Console.WriteLine($"Expected result did not exist, the file \"{actualResultFilename}\" was used as a reference. Please check the file");
            }
        }

        /// <summary>
        /// Attempts to merge actual into expected result file
        /// </summary>
        /// <param name="actualResultFileName">This is the file generated by the test</param>
        /// <param name="expectedResultFilename">This is the reference file which may be updated by the configured merge tool</param>
        private void Merge(string actualResultFileName, string expectedResultFilename) =>
            // Note currently merge tool is hard coded and expected the command line "merge.exe %base %left %right %merged"
            TestUtils.Cmd("p4merge.exe", expectedResultFilename, actualResultFileName, expectedResultFilename, expectedResultFilename);
    }
}
