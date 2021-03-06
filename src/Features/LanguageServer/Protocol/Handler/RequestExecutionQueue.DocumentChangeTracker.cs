﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.LanguageServer.Handler
{
    internal partial class RequestExecutionQueue
    {
        /// <summary>
        /// Keeps track of changes to documents that are opened in the LSP client. Calls MUST not overlap, so this
        /// should be called from a mutating request handler. See <see cref="RequestExecutionQueue"/> for more details.
        /// </summary>
        internal class DocumentChangeTracker : IWorkspaceService
        {
            private readonly Dictionary<Uri, SourceText> _trackedDocuments = new();

            internal void StartTracking(Uri documentUri, SourceText initialText)
            {
                Contract.ThrowIfTrue(_trackedDocuments.ContainsKey(documentUri), "didOpen received for an already open document.");

                _trackedDocuments.Add(documentUri, initialText);
            }

            internal void UpdateTrackedDocument(Uri documentUri, SourceText text)
            {
                Contract.ThrowIfFalse(_trackedDocuments.ContainsKey(documentUri), "didChange received for a document that isn't open.");

                _trackedDocuments[documentUri] = text;
            }

            internal void StopTracking(Uri documentUri)
            {
                Contract.ThrowIfFalse(_trackedDocuments.ContainsKey(documentUri), "didClose received for a document that isn't open.");

                _trackedDocuments.Remove(documentUri);
            }

            internal IEnumerable<(Uri DocumentUri, SourceText Text)> GetTrackedDocuments()
                => _trackedDocuments.Select(k => (k.Key, k.Value));
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly RequestExecutionQueue _queue;

            public TestAccessor(RequestExecutionQueue queue)
                => _queue = queue;

            public List<SourceText> GetTrackedTexts()
                => _queue._documentChangeTracker.GetTrackedDocuments().Select(i => i.Text).ToList();
        }
    }
}
