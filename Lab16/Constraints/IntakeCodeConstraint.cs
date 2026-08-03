using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace lab16.Constraints
{
    // Lab ID: 11 -> INTAKE_CODE = itiC (11 mod 3 = 2)
    //
    // This constraint does NOT touch the database, because a route
    // constraint's job is to decide whether a URL SHAPE is acceptable
    // before any controller or query even runs. Comparing against a
    // fixed, hardcoded intake code is a pure string check; involving
    // EF/SQL here would mean every incoming request — including
    // malformed or malicious ones — triggers a real database round-trip
    // before routing has even decided the URL is legitimate.
    public class IntakeCodeConstraint : IRouteConstraint
    {
        private const string MyIntakeCode = "itiC";

        public bool Match(
            HttpContext? httpContext,
            IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var value) || value is null)
            {
                return false;
            }

            var code = Convert.ToString(value, CultureInfo.InvariantCulture);

            return string.Equals(code, MyIntakeCode, StringComparison.OrdinalIgnoreCase);
        }
    }
}