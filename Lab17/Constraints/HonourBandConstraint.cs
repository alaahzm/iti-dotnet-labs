// =====================================================================
// HonourBandConstraint — FULL WORKING FALLBACK (Rule 20)
// ITI Summer Training | Web Development Using .NET | Morning Group
// Session 16 — Block 4
//
// This is the file where the framework calls YOUR code to make its own
// routing decision. Every route pattern until now was you asking the
// framework a question. This one makes the framework ask you.
// =====================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Globalization;
using System.Linq;

namespace lab17.Constraints
{
    public class HonourBandConstraint : IRouteConstraint
    {
        // The three band names the university actually uses. Kept in one
        // place, in one casing, so the constraint and the controller can
        // never drift apart.
        private static readonly string[] AllowedBands = { "first", "second", "pass" };

        // Called once per candidate route, WHILE routing is still
        // deciding — before any controller is chosen, before any action
        // runs, before the DbContext for this request even exists.
        //
        // Return true  -> this route is allowed to match.
        // Return false -> routing moves on to the next route in the
        //                 table as though this one were not there. It
        //                 does NOT produce an error; falling through to
        //                 nothing at all is what produces the 404.
        public bool Match(
            HttpContext? httpContext,
            IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            // Guard clause before work. Routing calls this
            // speculatively, so a missing key is a normal answer, not an
            // exception — which is why this is TryGetValue and not an
            // indexer.
            if (!values.TryGetValue(routeKey, out var value) || value is null)
            {
                return false;
            }

            // The value arrives as object, because routing does not know
            // what shape we wanted. InvariantCulture, not the current
            // culture: a URL is not written in the user's local language,
            // and using CurrentCulture here is a bug that only shows up
            // on somebody else's machine.
            var band = Convert.ToString(value, CultureInfo.InvariantCulture);

            // Case-insensitive on purpose. A user who types "First" with
            // a capital F has not made a mistake worth a 404.
            return AllowedBands.Contains(band, StringComparer.OrdinalIgnoreCase);
        }
    }
}
