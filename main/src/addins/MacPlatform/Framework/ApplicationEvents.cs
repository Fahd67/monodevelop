// 
// ApplicationEvents.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;

namespace OSXIntegration.Framework
{
	public static class ApplicationEvents
	{
		static object lockObj = new object ();
		
		#region Quit
		
		static EventHandler<ApplicationEventArgs> quit;
		static IntPtr quitHandlerRef;
		
		public static event EventHandler<ApplicationEventArgs> Quit {
			add {
				lock (lockObj) {
					quit += value;
					if (quitHandlerRef == IntPtr.Zero)
						quitHandlerRef = Carbon.InstallApplicationEventHandler (HandleQuit, CarbonEventApple.QuitApplication);
				}
			}
			remove {
				lock (lockObj) {
					quit -= value;
					if (quit == null && quitHandlerRef != IntPtr.Zero)
						Carbon.RemoveEventHandler (quitHandlerRef);
				}
			}
		}
		
		static CarbonEventHandlerStatus HandleQuit (IntPtr callRef, IntPtr eventRef, IntPtr user_data)
		{
			var args = new ApplicationEventArgs ();
			quit (null, args);
			return args.HandledStatus;
		}
		
		#endregion
		
		#region Reopen
		
		static EventHandler<ApplicationEventArgs> reopen;
		static IntPtr reopenHandlerRef;
		
		public static event EventHandler<ApplicationEventArgs> Reopen {
			add {
				lock (lockObj) {
					reopen += value;
					if (reopenHandlerRef == IntPtr.Zero)
						reopenHandlerRef = Carbon.InstallApplicationEventHandler (HandleReopen, CarbonEventApple.ReopenApplication);
				}
			}
			remove {
				lock (lockObj) {
					reopen -= value;
					if (reopen == null && reopenHandlerRef != IntPtr.Zero)
						Carbon.RemoveEventHandler (reopenHandlerRef);
				}
			}
		}
		
		static CarbonEventHandlerStatus HandleReopen (IntPtr callRef, IntPtr eventRef, IntPtr user_data)
		{
			var args = new ApplicationEventArgs ();
			reopen (null, args);
			return args.HandledStatus;
		}
		
		#endregion
		
		#region OpenDocuments
		
		static EventHandler<ApplicationDocumentEventArgs> openDocuments;
		static IntPtr openDocumentsHandlerRef;
		
		public static event EventHandler<ApplicationDocumentEventArgs> OpenDocuments {
			add {
				lock (lockObj) {
					openDocuments += value;
					if (openDocumentsHandlerRef == IntPtr.Zero)
						openDocumentsHandlerRef = Carbon.InstallApplicationEventHandler (HandleOpenDocuments, CarbonEventApple.OpenDocuments);
				}
			}
			remove {
				lock (lockObj) {
					openDocuments -= value;
					if (openDocuments == null && openDocumentsHandlerRef != IntPtr.Zero)
						Carbon.RemoveEventHandler (openDocumentsHandlerRef);
				}
			}
		}
		
		static CarbonEventHandlerStatus HandleOpenDocuments (IntPtr callRef, IntPtr eventRef, IntPtr user_data)
		{
			try {
				var docs = Carbon.GetFileListFromEventRef (eventRef);
				var args = new ApplicationDocumentEventArgs (docs);
				openDocuments (null, args);
				return args.HandledStatus;
			} catch (Exception ex) {
				System.Console.WriteLine (ex);
				return CarbonEventHandlerStatus.NotHandled;
			}
		}
		
		#endregion
		
		#region OpenUrls
		
		static EventHandler<ApplicationUrlEventArgs> openUrls;
		static IntPtr openUrlsHandlerRef;
		
		public static event EventHandler<ApplicationUrlEventArgs> OpenUrls {
			add {
				lock (lockObj) {
					openUrls += value;
					if (openUrlsHandlerRef == IntPtr.Zero)
						openUrlsHandlerRef = Carbon.InstallApplicationEventHandler (HandleOpenUrls, CarbonEventApple.GetUrl);
				}
			}
			remove {
				lock (lockObj) {
					openUrls -= value;
					if (openUrls == null && openUrlsHandlerRef != IntPtr.Zero)
						Carbon.RemoveEventHandler (openUrlsHandlerRef);
				}
			}
		}
		
		static CarbonEventHandlerStatus HandleOpenUrls (IntPtr callRef, IntPtr eventRef, IntPtr user_data)
		{
			try {
				var urls = Carbon.GetUrlListFromEventRef (eventRef);
				var args = new ApplicationUrlEventArgs (urls);
				openUrls (null, args);
				return args.HandledStatus;
			} catch (Exception ex) {
				System.Console.WriteLine (ex);
				return CarbonEventHandlerStatus.NotHandled;
			}
		}
		
		#endregion
	}
	
	public class ApplicationEventArgs : EventArgs
	{
		public bool Handled { get; set; }
		
		internal CarbonEventHandlerStatus HandledStatus {
			get {
				return Handled? CarbonEventHandlerStatus.Handled : CarbonEventHandlerStatus.NotHandled;
			}
		}
	}
	
	public class ApplicationDocumentEventArgs : ApplicationEventArgs
	{
		public ApplicationDocumentEventArgs (IList<string> documents)
		{
			this.Documents = documents;
		}		
		
		public IList<string> Documents { get; private set; }
	}
	
	public class ApplicationUrlEventArgs : ApplicationEventArgs
	{
		public ApplicationUrlEventArgs (IList<string> urls)
		{
			this.Urls = urls;
		}		
		
		public IList<string> Urls { get; private set; }
	}
}

