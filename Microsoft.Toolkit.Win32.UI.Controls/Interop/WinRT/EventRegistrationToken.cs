// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CsWinRT = WinRT;

namespace Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT
{
    /// <summary>
    /// <see cref="CsWinRT.EventRegistrationToken"/>
    /// </summary>
    public class EventRegistrationToken
    {
        private CsWinRT.EventRegistrationToken UwpInstance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRegistrationToken"/> class, a
        /// Wpf-enabled wrapper for <see cref="CsWinRT.EventRegistrationToken"/>
        /// </summary>
        public EventRegistrationToken(CsWinRT.EventRegistrationToken instance)
        {
            this.UwpInstance = instance;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="CsWinRT.EventRegistrationToken"/> to <see cref="EventRegistrationToken"/>.
        /// </summary>
        /// <param name="args">The <see cref="CsWinRT.EventRegistrationToken"/> instance containing the event data.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator EventRegistrationToken(
            CsWinRT.EventRegistrationToken args)
        {
            return FromEventRegistrationToken(args);
        }

        /// <summary>
        /// Creates a <see cref="EventRegistrationToken"/> from <see cref="CsWinRT.EventRegistrationToken"/>.
        /// </summary>
        /// <param name="args">The <see cref="CsWinRT.EventRegistrationToken"/> instance containing the event data.</param>
        /// <returns><see cref="EventRegistrationToken"/></returns>
        public static EventRegistrationToken FromEventRegistrationToken(CsWinRT.EventRegistrationToken args)
        {
            return new EventRegistrationToken(args);
        }
    }
}