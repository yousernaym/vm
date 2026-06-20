// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

namespace CefSharp.Example.RequestEventHandler
{
    public class OnRenderProcessTerminatedEventArgs : BaseRequestEventArgs
    {
        public OnRenderProcessTerminatedEventArgs(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status, int errorCode, string errorMessage)
            : base(browserControl, browser)
        {
            Status = status;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public CefTerminationStatus Status { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}
