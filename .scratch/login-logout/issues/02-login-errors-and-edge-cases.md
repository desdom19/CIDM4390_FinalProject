# 02: Errors and edge cases when logging in

**What to build:** A User who can't log in is told why, and stays logged out.
- **Wrong password or unknown username:** both show the same message, "Invalid username or password.", so nobody can tell which usernames exist.
- **Empty box:** an empty username or password, or one that's only spaces, shows "Enter a username and password." and does not check the database.
- **Database down:** the page shows "Logging in is unavailable right now. Try again later." The error is logged and no error page appears.
- **After a failure:** the username stays filled in and the password box is cleared.
- **Already logged in:** opening the login page goes straight to the Dashboard.

**Blocked by:** 01: Tracer bullet: a seeded User logs in and reaches the Dashboard

**Status:** ready-for-agent

**Spec:** `.scratch/login-logout/spec.md`

## Order of work

1. **Extend `UserService` test-first** (`tdd` skill). Log in returns InvalidCredentials, MissingInput or Unavailable as described in the spec. Unavailable is returned when the database can't be reached, without throwing.
2. **Show each result on the login page** with its message. The username is kept and the password is cleared.
3. **Redirect an already-logged-in User** from the login GET to the Dashboard.
4. **Run the full test suite.**

No database or package changes are needed.

## Acceptance criteria

- [ ] Test: a wrong password returns InvalidCredentials.
- [ ] Test: an unknown username returns InvalidCredentials.
- [ ] Test: a password that differs only in capitalisation returns InvalidCredentials.
- [ ] Test: an empty or whitespace-only username returns MissingInput.
- [ ] Test: an empty or whitespace-only password returns MissingInput.
- [ ] Test: a connection string pointing at an unreachable database returns Unavailable without throwing.
- [ ] In the browser, a wrong password and an unknown username both show "Invalid username or password." and the User stays logged out.
- [ ] In the browser, an empty box shows "Enter a username and password."
- [ ] In the browser, after failing to log in, the username is still filled in and the password box is empty.
- [ ] In the browser, opening `/Account/Login` while logged in goes to the Dashboard.
- [ ] The full test suite passes, and the verification report is rewritten.
