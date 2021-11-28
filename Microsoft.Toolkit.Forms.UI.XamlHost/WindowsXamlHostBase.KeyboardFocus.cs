// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Toolkit.Forms.UI.XamlHost.Interop.Win32;
using Microsoft.Toolkit.Win32.UI.XamlHost;
using WinRT;
using windows = Windows;

namespace Microsoft.Toolkit.Forms.UI.XamlHost
{
    /// <summary>
    ///     WindowsXamlHostBase hosts UWP XAML content inside Windows Forms
    /// </summary>
    public partial class WindowsXamlHostBase
    {
        /// <summary>
        /// Last Focus Request GUID to uniquely identify Focus operations, primarily used with error callbacks
        /// </summary>
        private Guid _lastFocusRequest = Guid.Empty;
        private bool _forceFocusNavigation = false;

        /// <summary>
        ///     Gets a value indicating whether this Control currently has focus. Check both the Control's
        ///     window handle and the hosted Xaml window handle. If either has focus
        ///     then this Control currently has focus.
        /// </summary>
        public override bool Focused
        {
            get
            {
                if (IsHandleCreated)
                {
                    // Get currently focused window handle and compare with Control
                    // and hosted Xaml content window handles
                    var focusHandle = SafeNativeMethods.GetFocus();
                    return focusHandle == Handle || (_xamlIslandWindowHandle != IntPtr.Zero && _xamlSource.HasFocus);
                }

                return false;
            }
        }

        /// <summary>
        ///     Activates the Windows Forms WindowsXamlHost Control
        /// </summary>
        protected override void Select(bool directed, bool forward)
        {
            ProcessTabKey(forward);
        }

        /// <summary>
        ///     Processes a tab key, ensuring that Xaml has an opportunity
        ///     to handle the command before normal Windows Forms processing.
        ///     (Xaml must be notified of keys that invoke focus navigation.)
        /// </summary>
        /// <returns>true if the command was processed</returns>
        protected override bool ProcessTabKey(bool forward)
        {
            if (DesignMode)
            {
                return false;
            }

            // Determine if the currently focused element is the last element for the requested
            // navigation direction.  If the currently focused element is not the last element
            // for the requested navigation direction, navigate focus to the next focusable
            // element.
            if (!_xamlSource.HasFocus || _forceFocusNavigation)
            {
                _forceFocusNavigation = false;
                var reason = forward ? windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.First : windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.Last;
                var request = new windows.UI.Xaml.Hosting.XamlSourceFocusNavigationRequest(reason, default(windows.Foundation.Rect));
                _lastFocusRequest = request.CorrelationId;
                var result = _xamlSource.NavigateFocus(request);
                if (result.WasFocusMoved)
                {
                    return true;
                }

                return false;
            }
            else
            {
                // Call windows.UI.Xaml.Input.FocusManager.TryMoveFocus Next or Previous and return
                windows.UI.Xaml.Input.FocusNavigationDirection navigationDirection =
                    forward ? windows.UI.Xaml.Input.FocusNavigationDirection.Next : windows.UI.Xaml.Input.FocusNavigationDirection.Previous;
                return windows.UI.Xaml.Input.FocusManager.TryMoveFocus(navigationDirection);
            }
        }

        /// <summary>
        /// In order to handle keyboard accelerators and TAB input, we must give a chance to <seealso cref="windows.UI.Xaml.Hosting.DesktopWindowXamlSource"/>
        /// to handle the <paramref name="msg"/> using <seealso cref="IDesktopWindowXamlSourceNative2.PreTranslateMessage(ref Message)"/>
        /// </summary>
        /// <param name="msg">The current incomming message in the queue</param>
        /// <returns>True if <seealso cref="_xamlSource"/> was able to handle the <paramref name="msg"/></returns>
        public override bool PreProcessMessage(ref Message msg)
        {
            var desktopXamlSourceNative = this._xamlSource.As<IDesktopWindowXamlSourceNative2>();
            if (desktopXamlSourceNative != null)
            {
                var result = desktopXamlSourceNative.PreTranslateMessage(ref msg);
                if (result)
                {
                    return true;
                }
            }

            return base.PreProcessMessage(ref msg);
        }

        /// <summary>
        /// Responds to DesktopWindowsXamlSource TakeFocusRequested event
        /// </summary>
        /// <param name="sender">DesktopWindowsXamlSource</param>
        /// <param name="args">DesktopWindowXamlSourceTakeFocusRequestedEventArgs</param>
        private void OnTakeFocusRequested(windows.UI.Xaml.Hosting.DesktopWindowXamlSource sender, windows.UI.Xaml.Hosting.DesktopWindowXamlSourceTakeFocusRequestedEventArgs args)
        {
            if (_lastFocusRequest == args.Request.CorrelationId)
            {
                // If we've arrived at this point, then focus is being move back to us
                // therefore, we should complete the operation to avoid an infinite recursion
                // by "Restoring" the focus back to us under a new correlationId
                var newRequest = new windows.UI.Xaml.Hosting.XamlSourceFocusNavigationRequest(
                    windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.Restore);
                _xamlSource.NavigateFocus(newRequest);
                _lastFocusRequest = newRequest.CorrelationId;
            }
            else
            {
                // Focus was not initiated by WindowsXamlHost. Continue processing the Focus request.
                var reason = args.Request.Reason;
                if (reason == windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.First || reason == windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.Last)
                {
                    var forward = reason == windows.UI.Xaml.Hosting.XamlSourceFocusNavigationReason.First;
                    _forceFocusNavigation = true;
                    try
                    {
                        Parent.SelectNextControl(this, forward, tabStopOnly: true, nested: false, wrap: true);
                    }
                    finally
                    {
                        _forceFocusNavigation = false;
                    }
                }
            }
        }
    }
}