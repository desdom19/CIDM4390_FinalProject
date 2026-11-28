# 04: Final manual check in the real app

**What to build:** An end-to-end check of the PBI's acceptance criteria. The developer runs it in the real app against `campaign_analytics`, logged in as the seeded Users, and records each result and screenshot.

The agent's only parts are:
- preparing the report template
- filling in the report from what the developer observes

**The agent never writes `login-logout-flow.md` and does not do the check that `password_hash` holds hashes.** Both are the developer's own tasks.

**Blocked by:** 01: Tracer bullet: a seeded User logs in and reaches the Dashboard; 02: Errors and edge cases when logging in; 03: Log out, the nav bar, and keeping the login safe

**Status:** ready-for-human

**Spec:** `.scratch/login-logout/spec.md`

## PBI acceptance criteria delivered (verbatim from issue #14)

- [ ] The users table has username, password hash, and role fields.
- [ ] A seeded user can log in with the correct password and end up on the Dashboard page.
- [ ] A wrong username or password shows an error message and does not allow the user to log in.
- [ ] A marketer and admin account exist in the users table.
- [ ] There's a login page that is separate from the Dashboard and Import CSV pages.
- [ ] Users cannot access Dashboard and Import CSV pages unless they're successfully logged in. Users not logged in that try to open the Dashboard and Import CSV pages are redirected to login page.
- [ ] Clicking the log out link allows a user to log out their account, and disables access to dashboard and Import CSV pages. Logging out sends a user back to the login page.

Developer's own tasks:

- [ ] The password field is hashed, and does not hold plain-text.
- [ ] No plain-text passwords are stored.
- [ ] login-logout-flow.md is committed and shows the process of how logging in and out works.

## Steps

1. **Agent prepares** the template `.scratch/login-logout/verification-report-manual.md`, with one row per check below.
2. **Developer runs** `DESCRIBE users;` and `SELECT user_id, username, role FROM users;` in `campaign_analytics`. This confirms the four columns and the two Users. The query deliberately leaves out `password_hash`; checking it is the developer's own task.
3. **Developer runs these seven checks in the browser**, with a screenshot of checks 1 to 5 in `.scratch/login-logout/images/`:
   1. Log in as `marketer`, then log out and log in as `admin`. Each lands on the Dashboard.
   2. Try a wrong password, then an unknown username. Both show "Invalid username or password." and stay on the login page.
   3. Submit with an empty box. "Enter a username and password." shows.
   4. While logged out, type `/Dashboard` and `/Import` into the address bar. Both go to the login page.
   5. Log out. You land on the login page, and opening `/Dashboard` goes back to the login page.
   6. Press Back after logging out. The Dashboard doesn't appear.
   7. While logged in, open `/Account/Login`. It goes to the Dashboard.
4. **Agent fills in** `verification-report-manual.md` from what the developer reports.
5. **Developer does their own tasks:** checks `password_hash` and writes `login-logout-flow.md`.

## Acceptance criteria

- [ ] `DESCRIBE users;` shows `user_id`, `username`, `password_hash` and `role`, and the two Users are listed with the right Roles.
- [ ] All seven browser checks pass, and checks 1 to 5 are screenshotted.
- [ ] The results are recorded in `verification-report-manual.md`.
- [ ] The developer completes their own tasks.
