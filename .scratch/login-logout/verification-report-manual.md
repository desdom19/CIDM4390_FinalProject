# Verification report: manual log in and log out check

- Date: 2026-11-27
- Database: `campaign_analytics` (the real database; only the developer touched it)
- Checked by: the developer, in the running app and with read-only queries in `campaign_analytics`
- Ticket: `.scratch/login-logout/issues/04-final-manual-check.md`

Filled in by the agent from what the developer observed. The developer reported that every check passed.

## Step 2: the users table

`DESCRIBE users;`

| Check | Expected | Seen | Result |
|---|---|---|---|
| Columns | `user_id`, `username`, `password_hash`, `role` | `user_id`, `username`, `password_hash`, `role` | match |

`SELECT user_id, username, role FROM users;`

| Check | Expected | Seen | Result |
|---|---|---|---|
| Marketer row | username `marketer`, role marketer | username `marketer`, role marketer | match |
| Admin row | username `admin`, role admin | username `admin`, role admin | match |
| Row count | 2 | 2 | match |

The query leaves out `password_hash` on purpose. Checking that it holds hashes is the developer's own task (see the last section).

## Step 3: browser checks

### 1. Each seeded User logs in and lands on the Dashboard

| Check | Expected | Seen | Result |
|---|---|---|---|
| Log in as `marketer` | lands on `/Dashboard` | lands on `/Dashboard` | match |
| Log out, then log in as `admin` | lands on `/Dashboard` | lands on `/Dashboard` | match |

Screenshots: `images/MarketerLoginLandingPage.png`, `images/AdminLoginLandingPage.png`

### 2. A wrong password or an unknown username is refused

| Check | Expected | Seen | Result |
|---|---|---|---|
| Wrong password | "Invalid username or password." and stays on the login page | "Invalid username or password." and stays on the login page | match |
| Unknown username | "Invalid username or password." and stays on the login page | "Invalid username or password." and stays on the login page | match |

Screenshot: `images/InvalidUserOrPassCheck.png` (unknown username `accounting`)

### 3. An empty box is refused

| Check | Expected | Seen | Result |
|---|---|---|---|
| Submit with an empty box | "Enter a username and password." | "Enter a username and password." | match |

Screenshot: `images/EmptyUserAndPassCheck.png`

### 4. Logged out, the protected pages send you to the login page

| Check | Expected | Seen | Result |
|---|---|---|---|
| Type `/Dashboard` in the address bar | goes to the login page | goes to the login page | match |
| Type `/Import` in the address bar | goes to the login page | goes to the login page | match |

Screenshots: `images/NotLoggedInDashboardCheck.png`, `images/NotLoggedInImportCheck.png`

### 5. Logging out

| Check | Expected | Seen | Result |
|---|---|---|---|
| Click Log out | lands on the login page | lands on `/Account/Login`, nav bar shows "Log in" | match |
| Then open `/Dashboard` | goes back to the login page | goes back to the login page | match |

Screenshot: `images/LoggedOut.png`

### 6. Pressing Back after logging out

| Check | Expected | Seen | Result |
|---|---|---|---|
| Press Back after logging out | the Dashboard doesn't appear | the Dashboard doesn't appear | match |

### 7. Logged in, the login page sends you to the Dashboard

| Check | Expected | Seen | Result |
|---|---|---|---|
| Open `/Account/Login` while logged in | goes to `/Dashboard` | goes to `/Dashboard` | match |

All seven browser checks passed.

## Developer's own tasks (not filled in by the agent)

- [ ] `password_hash` holds hashes, not plain-text passwords
- [ ] No plain-text passwords are stored
- [ ] `login-logout-flow.md` is written and committed

Screenshot of the developer's `password_hash` check: `images/PasswordHashSQLCheck.png`

## Not checked here

Seeding the Users again, capital letters in the username or password, and an unreachable database are covered only by the automated tests in `verification-report-tests.md`.
