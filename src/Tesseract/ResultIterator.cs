﻿using System;
using System.Collections.Generic;
using System.Text;

using Tesseract.Interop;

namespace Tesseract
{
    public sealed class ResultIterator : PageIterator
    {
        internal ResultIterator(Page page, IntPtr handle)
            : base(page, handle)
        {
        }

        public float GetConfidence(PageIteratorLevel level)
        {
            VerifyNotDisposed();
            return handle.Handle == IntPtr.Zero ? 0f : TessApi.Native.ResultIteratorGetConfidence(handle, level);
        }

        public string GetText(PageIteratorLevel level)
        {
            VerifyNotDisposed();
            return handle.Handle == IntPtr.Zero ? string.Empty : TessApi.ResultIteratorGetUTF8Text(handle, level);
        }

        private Dictionary<int, FontInfo> _fontInfoCache = new();
        public FontAttributes GetWordFontAttributes()
        {
            VerifyNotDisposed();
            if (handle.Handle == IntPtr.Zero)
            {
                return null;
            }


            // per docs (ltrresultiterator.h:104 as of 4897796 in github:tesseract-ocr/tesseract)
            // this return value points to an internal table and should not be deleted.
            IntPtr nameHandle =
                TessApi.Native.ResultIteratorWordFontAttributes(
                    handle,
                    out bool isBold, out bool isItalic, out bool isUnderlined,
                    out bool isMonospace, out bool isSerif, out bool isSmallCaps,
                    out int pointSize, out int fontId);

            // This can happen in certain error conditions
            if (nameHandle == IntPtr.Zero)
            {
                return null;
            }

            if (!_fontInfoCache.TryGetValue(fontId, out FontInfo fontInfo))
            {
                string fontName = MarshalHelper.PtrToString(nameHandle, Encoding.UTF8);
                fontInfo = new FontInfo(fontName, fontId, isItalic, isBold, isMonospace, isSerif);
                _fontInfoCache.Add(fontId, fontInfo);
            }

            return new FontAttributes(fontInfo, isUnderlined, isSmallCaps, pointSize);
        }

        public string GetWordRecognitionLanguage()
        {
            VerifyNotDisposed();
            return handle.Handle == IntPtr.Zero ? null : TessApi.ResultIteratorWordRecognitionLanguage(handle);
        }

        public bool GetWordIsFromDictionary()
        {
            VerifyNotDisposed();
            return handle.Handle != IntPtr.Zero && TessApi.Native.ResultIteratorWordIsFromDictionary(handle);
        }

        public bool GetWordIsNumeric()
        {
            VerifyNotDisposed();
            return handle.Handle != IntPtr.Zero && TessApi.Native.ResultIteratorWordIsNumeric(handle);
        }

        public bool GetSymbolIsSuperscript()
        {
            VerifyNotDisposed();
            return handle.Handle != IntPtr.Zero && TessApi.Native.ResultIteratorSymbolIsSuperscript(handle);
        }

        public bool GetSymbolIsSubscript()
        {
            VerifyNotDisposed();
            return handle.Handle != IntPtr.Zero && TessApi.Native.ResultIteratorSymbolIsSubscript(handle);
        }

        public bool GetSymbolIsDropcap()
        {
            VerifyNotDisposed();
            return handle.Handle != IntPtr.Zero && TessApi.Native.ResultIteratorSymbolIsDropcap(handle);
        }

        /// <summary>
        /// Gets an instance of a choice iterator using the current symbol of interest. The ChoiceIterator allows a one-shot iteration over the
        /// choices for this symbol and after that is is useless.
        /// </summary>
        /// <returns>an instance of a Choice Iterator</returns>
        public ChoiceIterator GetChoiceIterator()
        {
            IntPtr choiceIteratorHandle = TessApi.Native.ResultIteratorGetChoiceIterator(handle);
            return choiceIteratorHandle == IntPtr.Zero ? null : new ChoiceIterator(choiceIteratorHandle);
        }
    }
}
