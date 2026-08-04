# Lab 17 Reflection — Alaa Hazem Helmy — Lab ID 11

F.1: Lab ID = 11.
MIN_GPA_EDIT = 2.0 + (11 mod 5) * 0.3 = 2.0 + (1 * 0.3) = 2.3
MAX_YEAR_EDIT = (11 mod 3) + 2 = 2 + 2 = 4
(Adapted onto Instructor.YearsOfExperience as the range 2 to 4, since Instructor has
no Gpa/YearOfStudy fields — the lab's actual Edit target turned out to be Instructor,
not Student, based on the provided Views/Instructors folder.)

F.2: Bad input can be rejected in three places, in the order they actually run:
1. In the browser, before the form is even submitted (HTML5 input constraints, if any).
2. On the server, inside ModelState.IsValid, checked automatically by the framework
   using the [Range] attribute on Instructor.YearsOfExperience before my guard clause
   even runs.
3. At the database level, via constraints EF actually migrated (like [Required]/
   [MaxLength] from Session 14), which would reject a bad INSERT/UPDATE even if
   somehow both earlier checks were bypassed.
My Part D change (the [Range(2,4)] boundary) lives in place #2 — it's a
validation-only check, never migrated into the database schema at all. When I typed
a value below 2, the app displayed: "Years of experience must be between 2 and 4 for
this portal (Alaa's boundary)." and SSMS confirmed the row was not changed.

F.3: What changed in the browser is that, after a successful save, the browser is no
longer sitting on the Edit page at all — the redirect instructs the browser to
navigate to a brand new address (/Instructors/AlaaConfirmed/<id>) via a fresh GET
request. The browser's history now records that GET request as the current page, not
the original POST. Pressing F5 re-requests whatever the browser is currently showing,
so F5 now just re-runs a harmless GET instead of re-submitting the original POST form
data — which is exactly why the row count in /Instructors did not grow when I tested it.

F.4: [Required]/[MaxLength] and [Range] are genuinely the same in that both are
written as plain C# attributes directly above a property on the model class, with no
special syntax difference between them. They are genuinely different in what they do
to the database: [Required] and [MaxLength] ARE mapped by Entity Framework into real
schema constraints (NOT NULL, a bounded column length) and required an actual
migration to apply back in Session 14, while [Range] is validation-only — it is
never translated into any column type or constraint, and changing its boundaries
never requires a migration at all, which I confirmed directly in Part D: changing
the Range boundary and rebuilding required zero Add-Migration/Update-Database step.