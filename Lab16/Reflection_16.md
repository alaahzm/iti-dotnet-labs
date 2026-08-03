# Lab 16 Reflection — Alaa Hazem Helmy — Lab ID 11

F.1: Lab ID = 11.
MAX_YEAR = (11 mod 4) + 1 = 3 + 1 = 4
MIN_GPA = 2.5 + (11 mod 3) * 0.5 = 2.5 + (2 * 0.5) = 3.5
INTAKE_CODE = 11 mod 3 = 2 -> C -> itiC

F.2: For a request to /students/top/5 (one above my MAX_YEAR of 4): the middleware
prints [START] Request path : /students/top/5. Routing then checks the route table
top to bottom, reaches the studentsTop route, and evaluates its chained
:int:range(1,4) constraint against 5 — 5 is a valid integer but falls outside the
inclusive range 1-4, so the constraint returns false and this route is skipped.
Routing continues checking any remaining routes, finds none that match, and the
request falls through to a 404. The middleware then prints [END] Request path :
/students/top/5. What does NOT happen: the Top action is never entered, no
StudentPortalContext query is ever issued to the database, and no view is rendered.

F.3: Genuinely the same: both my IntakeCodeConstraint and the built-in :int
constraint are registered under a short nickname used inside a route pattern, and
both are consulted by the routing system in exactly the same way — routing calls
Match() (or the built-in's equivalent) on each candidate route's constraints before
ever choosing that route, with no special-casing for "built-in" vs "custom."
Genuinely different: the built-in :int constraint is provided by the framework and
already exists in ConstraintMap the moment the app starts; my IntakeCodeConstraint
had to be written by hand as a class implementing IRouteConstraint and manually
added to ConstraintMap myself in Program.cs before routing knew the "intakecode"
nickname existed at all.

F.4: My About action is unreachable at /Students/About. This is a guarantee, not a
limitation — attribute routing on an action completely replaces the conventional
route for that action; once [Route("about/alaa")] is applied, that exact address is
the only one that will ever reach this action, which is precisely what makes
attribute routing trustworthy: nothing in Program.cs's route table can silently add
or change an address for this action later without editing the attribute itself.