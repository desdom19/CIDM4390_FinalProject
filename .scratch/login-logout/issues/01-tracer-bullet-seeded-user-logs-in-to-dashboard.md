# 01: Tracer bullet: a seeded User logs in and reaches the Dashboard

**What to build:** When the app starts, a Marketer and an Admin exist in the users table, each with a hashed password taken from User Secrets.
- **Logging in:** a User opens the login page, enters a correct username and password, and lands on the Dashboard page.
- **Logged out:** opening the Dashboard or Import CSV page sends the visitor to the login page. Submitting a CSV import is refused too.
- **Still open:** Home and Privacy stay open to everyone.

This slice handles the success path only. It leaves out:
- the error messages, empty boxes, the database-unavailable message, and the redirect for an already-logged-in User (ticket 02)
- log out, the nav bar, the session timeout, and `no-store` (ticket 03)

For now, if logging in fails, the form just shows again with no message.

**Blocked by:** None (can start immediately)

**Status:** done

**Spec:** `.scratch/login-logout/spec.md`

## Order of work

1. **Write the SQL script** `.scratch/login-logout/create-users-table.sql`:
   - `user_id` integer primary key, auto-increment
   - `username` varchar(50), required and unique
   - `password_hash` varchar(255), required
   - `role` enum of `marketer` and `admin`, required

   No seed rows go in the script. The app adds them.
2. **Stop for the developer's approval** before running the script. It is a database change. Once approved, it is run in `campaign_analytics_test` and `campaign_analytics`, and checked with `DESCRIBE users;` in each.
3. **Prefactor the test helper:**
   - The test database helper gets a way to empty the users table and a way to build a `UserService`.
   - A `ReportFeature` is added for `login-logout`, so results are written to `.scratch/login-logout/verification-report-tests.md`.
4. **Build `UserService` test-first** (`tdd` skill), against `campaign_analytics_test`, with made-up passwords only:
   - Log in returns Success with the Role, using `PasswordHasher<T>` to verify.
   - Seed adds any missing User, never overwrites an existing one, and throws an error naming the User Secrets key when a missing User has no password.
5. **Connect it to the app:**
   - Seed runs at startup, reading `Seed:MarketerPassword` and `Seed:AdminPassword`.
   - Cookie authentication is set up with the Account login page as its login path.
   - `AccountController` gets the login GET and POST, with an anti-forgery token on the POST.
   - The Dashboard and Import controllers get `[Authorize]`.
   - Authentication is added to the pipeline before authorization.
6. **Ask the developer to set the two seed secrets** in the app project's User Secrets. The agent never sees or chooses the real passwords. Then the developer starts the app.
7. **Run the full test suite.** Every existing test still passes, and the existing verification reports are still written to their own folders.

## Acceptance criteria

- [ ] The developer approves the users SQL script before it is run, and the table exists in both databases with the four columns above.
- [ ] Starting the app with both seed secrets set adds a Marketer (`marketer`) and an Admin (`admin`). Starting it again does not change either row.
- [ ] Starting the app while a User is missing and its seed secret isn't set stops with a message naming the missing key.
- [ ] Test: Log in with the right password returns Success with Role `marketer` for the Marketer and `admin` for the Admin.
- [ ] Test: Log in with `ADMIN` and the Admin's password returns Success.
- [ ] Test: a wrong password, an unknown username, or a password that differs only in capitalisation does not return Success.
- [ ] Test: Seed adds both Users to an empty table with the right Roles.
- [ ] Test: Seed run again with different passwords leaves the hashes unchanged. The original password still works and the new one does not.
- [ ] Test: Seed with a missing password for a missing User throws an error naming that User.
- [ ] Test: the Dashboard and Import controllers require the User to be logged in, and the Account login actions allow anyone.
- [ ] In the browser, logging in with a seeded User's correct password lands on the Dashboard.
- [ ] In the browser, opening `/Dashboard` or `/Import` while logged out goes to the login page.
- [ ] The full test suite passes, and `.scratch/login-logout/verification-report-tests.md` is written.
