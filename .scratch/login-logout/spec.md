# PBI 7: User Login and Logout

Status: ready-for-agent
Branch: 14-pbi-7-feature-user-login-and-logout
Issue: #14

## Problem Statement

Anyone who can reach the app can open the Dashboard page and import campaign data. There is no way to tell who is using it, and no way to keep someone out. A person with no business seeing campaign figures can read every Channel's ROI. Worse, they can import a CSV, which adds Daily Results and Campaigns that everyone else then sees on the Dashboard. Only authorized personnel, a Marketer or an Admin, should be able to view the dashboard or import campaign data.

## Solution

A User must log in before opening the Dashboard or Import CSV pages.
- **Login page:** a separate page with a username box, a password box and a Log in button. A correct username and password logs the User in and takes them to the Dashboard.
- **Two Users:** one Marketer and one Admin exist from the start. Their passwords come from the developer's User Secrets and are stored only as hashes.
- **Protected pages:** while logged out, opening the Dashboard or Import CSV page goes to the login page. Home and Privacy stay open to everyone.
- **Error messages:**
  - A wrong password or an unknown username shows "Invalid username or password." The User stays logged out.
  - An empty username or password shows "Enter a username and password."
  - If the database can't be reached, the page shows "Logging in is unavailable right now. Try again later."
  - If logging in fails, the username stays filled in and the password box is cleared.
- **Nav bar:** a logged-in User sees "Logged in as <username>" and a Log out link. A logged-out visitor sees a Log in link.
- **Log out:** logs the User out and returns them to the login page. After that, the Dashboard and Import CSV pages need the User to log in again, and pressing Back does not show them.
- **Session:** the User is logged out when the browser closes, or after 30 minutes without activity.

## User Stories

1. As a Marketer, I want to log in with my username and password, so that I can view the dashboard and import campaign data.
2. As an Admin, I want to log in with my username and password, so that I can view the dashboard and import campaign data.
3. As a User, I want to land on the Dashboard page after logging in, so that I see campaign performance straight away.
4. As a User, I want the login page to be separate from the Dashboard and Import CSV pages, so that nothing protected shows before I log in.
5. As a User, I want my password box to hide what I type, so that people nearby can't read my password.
6. As a User, I want "Invalid username or password." when I enter a wrong password, so that I know I wasn't logged in.
7. As a User, I want the same "Invalid username or password." message when I enter a username that doesn't exist, so that nobody can find out which usernames are real.
8. As a User, I want my username to stay filled in when I fail to log in, so that I only need to retype my password.
9. As a User, I want my password box cleared when I fail to log in, so that a wrong password isn't left on the screen.
10. As a User, I want "Enter a username and password." when I leave either box empty, so that I know what's missing.
11. As a User, I want my username to work whatever its capitalisation, so that typing "Admin" instead of "admin" doesn't lock me out.
12. As a User, I want my password to have to match exactly, including capitalisation, so that a close guess doesn't get in.
13. As a User, I want a clear "Logging in is unavailable right now. Try again later." message when the database is down, so that I don't think my password is wrong.
14. As a User, I want to see an error message, not a crash page, when logging in can't reach the database, so that the app still looks under control.
15. As a logged-out visitor, I want to be sent to the login page when I open the Dashboard page, so that I know I must log in first.
16. As a logged-out visitor, I want to be sent to the login page when I open the Import CSV page, so that I know I must log in first.
17. As the team, I want submitting a CSV import while logged out to be refused, so that nobody can add Daily Results without logging in.
18. As a logged-out visitor, I want the Home and Privacy pages to stay open, so that I can read about the app before logging in.
19. As a logged-out visitor, I want a Log in link in the nav bar, so that I can find the login page from any page.
20. As a logged-out visitor, I want the Dashboard and Import CSV links to stay in the nav bar, so that I can see what the app offers. Following them takes me to the login page.
21. As a logged-in User, I want the nav bar to show "Logged in as <username>", so that I know which User I'm logged in as.
22. As a logged-in User, I want a Log out link in the nav bar on every page, so that I can log out from wherever I am.
23. As a logged-in User, I want logging out to send me to the login page, so that I can see I'm logged out.
24. As a User who has logged out, I want the Dashboard and Import CSV pages to need me to log in again, so that the next person at my computer can't use the app as me.
25. As a User who has logged out, I want pressing Back not to show the Dashboard or Import CSV page again, so that the campaign figures I was looking at stay private.
26. As a User, I want to be logged out when I close the browser, so that a shared computer doesn't stay logged in.
27. As a User, I want to be logged out after 30 minutes without activity, so that a computer left unattended doesn't stay logged in.
28. As a User who is still working, I want each page I open to restart the 30 minutes, so that I'm not logged out in the middle of work.
29. As a logged-in User, I want opening the login page to take me to the Dashboard, so that I don't log in twice.
30. As the team, I want another website not to be able to log a User out, so that log out only happens when the User asks for it.
31. As the team, I want every password stored only as a salted hash, so that reading the users table does not reveal any password.
32. As the developer, I want the Marketer and Admin Users created automatically when the app starts, so that a fresh database is ready without hand-made hashes.
33. As the developer, I want the two seed passwords to come from User Secrets, so that no password is in the code or the repository.
34. As the developer, I want the app to refuse to start with a clear message when a User is missing and its seed password isn't set, so that I know exactly which secret to set.
35. As the developer, I want starting the app again not to overwrite an existing User, so that a password already in the database is never silently replaced.
36. As the developer, I want each User to have exactly one Role, Marketer or Admin, and nothing else allowed, so that a misspelled Role can't get into the users table.
37. As the developer, I want usernames to be unique, so that two Users can never share one.
38. As the developer, I want the logged-in User's Role kept while they are logged in, so that a later PBI can give Marketers and Admins different access without changing how logging in works.
39. As the developer, I want automated tests for logging in and the seeding, against the test database, so that a change that breaks logging in is caught.
40. As the developer, I want an automated test that fails if the Dashboard or Import CSV page stops requiring the User to be logged in, so that the protection can't be removed by accident.
41. As the developer, I want the test results written to a verification report, so that I can show the checks passed, as in earlier PBIs.

## Implementation Decisions

- **How logging in works:** ASP.NET Core's built-in cookie authentication, checked against our own users table, read with MySqlConnector like the rest of the app. ASP.NET Core Identity is not used (see ADR 0001). No new packages are installed.
- **Password hashing:** the built-in `PasswordHasher<T>`, which uses salted PBKDF2. Its stored format starts with the same version marker for every hash and is unique per User even when two passwords are the same. Verifying a password uses the hasher's own verify call, never a plain string comparison.
- **Schema change:** a new `users` table, created by hand from a SQL script kept in the feature folder. It is run in both `campaign_analytics` and `campaign_analytics_test`, only after the developer approves:
  - `user_id`: integer primary key, auto-increment
  - `username`: varchar(50), required, unique
  - `password_hash`: varchar(255), required
  - `role`: enum of `marketer` and `admin`, required

  The `users` table has no link to the campaign tables.
- **Username matching:** case is ignored, using MySQL's default comparison for the column. The password must match exactly.
- **New module, `UserService`:** built from the connection string like `DashboardService` and `CsvImportService`, and registered the same way. It is the only code that reads or writes the users table, and it has two operations.
  - **Log in** takes a username and password. It returns one of four results:
    - **Success**, which carries the username and Role
    - **InvalidCredentials**, for a wrong password or an unknown username alike
    - **MissingInput**, when the username or password is empty or whitespace; the database is not checked
    - **Unavailable**, when the database can't be reached; the error is logged and nothing is thrown to the caller
  - **Seed** takes the Marketer's and Admin's seed passwords. For each fixed username, `marketer` and `admin`, it adds the User with the hash and the matching Role if that User doesn't exist. It never updates an existing User. If a User is missing and its password is not given, it throws an error naming the User Secrets key to set.
- **Seeding at startup:** after the startup connection check that already exists, the app reads `Seed:MarketerPassword` and `Seed:AdminPassword` from configuration (User Secrets in development) and calls Seed. Only the passwords are secret; the usernames are fixed in code. Changing a seed password later does not change the stored hash. To reset a User's password, delete that User's row and start the app again.
- **New `AccountController`:**
  - **Login page (GET):** shows the form. A User who is already logged in is sent to the Dashboard.
  - **Login submit (POST):** checks the anti-forgery token and calls Log in.
    - On Success, it logs the User in with a cookie that holds their username and Role, and sends them to the Dashboard.
    - On any other result, it shows the form again with the matching message, keeping the username and clearing the password.
    - Any `returnUrl` is ignored. Success always goes to the Dashboard.
  - **Log out (POST):** checks the anti-forgery token, logs the User out, and sends them to the login page. It is POST-only, so another site can't log a User out with a link.
- **Cookie settings:**
  - The login path is the Account login page.
  - Cookies are not persistent, so there is no remember-me and the User is logged out when the browser closes.
  - The User is logged out after 30 minutes of inactivity, and each request restarts the 30 minutes.
- **Protected pages:**
  - `[Authorize]` is placed on the whole Dashboard and Import controllers, so GET and POST are both covered. That includes the CSV import POST.
  - Home, Privacy and the Account login page stay open to everyone.
  - Authentication is added to the request pipeline before the authorization step that already exists.
- **No caching:** responses from the Dashboard and Import controllers are sent with `no-store`, so the browser can't show a saved copy after log out.
- **Nav bar:** the shared layout shows "Logged in as <username>" and a Log out button, styled as a link and posting with an anti-forgery token, when a User is logged in. It shows a Log in link when nobody is. The Dashboard and Import CSV links stay visible either way.
- **Roles:** this PBI stores the Role and carries it while the User is logged in, but it does not limit access by Role. Marketers and Admins can do the same things.
- **Wording:** the UI, code and tickets say "User", "log in" and "log out", never "account" or "sign in", following the glossary. The controller name `AccountController` follows the ASP.NET convention for the login path and is the only exception.

## Testing Decisions

- **What a good test is:** a test checks what the module does from the outside, such as which result Log in returns or which Users exist after Seed. It never checks how the module does it. Expected outcomes come from this spec and the tickets, not from reading the code.
- **Seam 1, `UserService`, against `campaign_analytics_test`:**
  - Each test empties the users table first, then adds its own Users with made-up passwords such as `testpass-123`, through Seed.
  - The real seed passwords in User Secrets are never read by the tests.
  - The existing guard that refuses to run unless the database is exactly `campaign_analytics_test` stays in place.
  - The tests check that Log in returns Success with the right Role for the right password, for both the Marketer and the Admin.
  - The tests check that Log in returns InvalidCredentials for a wrong password, for an unknown username, and for a password that differs only in capitalisation.
  - The tests check that Log in returns Success for a username that differs only in capitalisation, such as `ADMIN`.
  - The tests check that Log in returns MissingInput for an empty or whitespace-only username or password.
  - The tests check that Log in returns Unavailable, without throwing, when the connection string points at an unreachable database. This follows the database-failure tests in the dashboard PBIs.
  - The tests check that Seed adds both Users with the right Roles to an empty table.
  - The tests check that Seed run a second time with different passwords leaves the stored hashes unchanged, so the original password still logs in and the new one does not.
  - The tests check that Seed with a missing password for a missing User throws an error naming that User.
- **Seam 2, controller attributes:**
  - A test reads the `[Authorize]` and `[AllowAnonymous]` attributes. It checks that the Dashboard and Import controllers require the User to be logged in and that the Account login actions do not. No HTTP calls are made and no package is installed.
- **Verification report:** the tests write `verification-report-tests.md` in the feature folder, through the existing verification report fixture.
- **Not automated:** redirects, the cookie, log out, the nav bar, `no-store`, and Back after log out. Testing those through HTTP would need the `Mvc.Testing` package, which was chosen against. The developer's manual check in the last ticket covers them.
- **Prior art:**
  - The `DashboardServiceTests` and `CsvImportServiceTests` integration tests, which use the shared test database collection
  - The test database helper, which gets a method to empty the users table
  - The verification report fixture

## Out of Scope

- Locking a User out after repeated failed attempts to log in, and limiting how often a User can try to log in
- Changing, resetting or recovering a password
- Registering new Users, and any page for managing Users
- Remember-me, and staying logged in after the browser closes
- Different access for Marketers and Admins
- Returning a User to the page they first asked for after logging in
- Automated HTTP-level tests of redirects, cookies and log out
- Checking by hand that the `password_hash` column holds hashes, not plain text: the developer does this
- The `login-logout-flow.md` knowledge artifact: the developer writes this

## Further Notes

- **Tickets:** four tickets, built and verified one at a time.
  1. A first working slice: the users table, `UserService`, seeding, logging in with a cookie, the login page, and the protected pages. This is the only ticket that changes the database.
  2. Errors and edge cases when logging in, including the database-unavailable message.
  3. Log out, the nav bar, the session timeout, and `no-store`.
  4. The developer's manual check, marked `ready-for-human`.
- **Approvals:** the developer approves each SQL change before it is run. No package installs are planned.
- **Before the first run:** set `Seed:MarketerPassword` and `Seed:AdminPassword` in the app project's User Secrets. Otherwise startup stops with a message naming the missing key.
- **References:** see the glossary's People and access section and ADR 0001 (cookie authentication over Identity).
