# 03: Log out, the nav bar, and keeping the login safe

**What to build:** A logged-in User can log out from any page and is returned to the login page. After that, the Dashboard and Import CSV pages need a new login.
- **Nav bar when logged in:** shows "Logged in as <username>" and a Log out link.
- **Nav bar when logged out:** shows a Log in link. The Dashboard and Import CSV links stay visible either way.
- **Log out link:** a button styled as a link that sends a POST with an anti-forgery token, so another site can't log a User out.
- **Session length:** the User is logged out when the browser closes, or after 30 minutes without activity. Each request restarts the 30 minutes.
- **Back button:** the Dashboard and Import CSV pages are sent with `no-store`, so pressing Back after logging out doesn't show them.

**Blocked by:** 01: Tracer bullet: a seeded User logs in and reaches the Dashboard

**Status:** ready-for-agent

**Spec:** `.scratch/login-logout/spec.md`

## Order of work

1. **Add the log out action** to `AccountController`. It is POST-only, checks the anti-forgery token, and sends the User to the login page.
2. **Update the shared layout's nav bar.**
3. **Set the cookie options:**
   - not persistent
   - a 30-minute sliding expiration
4. **Add `no-store`** to responses from the Dashboard and Import controllers.
5. **Run the full test suite.** This ticket adds no automated tests, because the behaviour is checked in the browser and in ticket 04. The `[Authorize]` attribute test from ticket 01 must still pass.

No database or package changes are needed.

## Acceptance criteria

- [ ] In the browser, a logged-in User sees "Logged in as <username>" and a Log out link on every page.
- [ ] In the browser, a logged-out visitor sees a Log in link, and the Dashboard and Import CSV links still show.
- [ ] In the browser, clicking Log out lands on the login page. Opening `/Dashboard` or `/Import` afterwards goes to the login page.
- [ ] In the browser, pressing Back after logging out does not show the Dashboard or Import CSV page.
- [ ] A GET request to the log out address does not log the User out.
- [ ] The login cookie is a session cookie with a 30-minute sliding expiration, as seen in the browser's developer tools and the cookie settings.
- [ ] The full test suite passes.
