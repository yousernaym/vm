﻿// Copyright © 2010-2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;

namespace CefSharp.Example
{
	public class DownloadHandler : IDownloadHandler
	{
		public event EventHandler<DownloadItem> OnBeforeDownloadFired;
		public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

		public bool ShowDialog { get; set; } = true;

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
			OnBeforeDownloadFired?.Invoke(this, downloadItem);

			if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: ShowDialog);
                }
            }
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
			OnDownloadUpdatedFired?.Invoke(this, downloadItem);
		}
    }
}
