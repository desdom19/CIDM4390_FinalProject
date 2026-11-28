# Log In, Log Out Process Notes

## Sequence Diagram


```mermaid
sequenceDiagram
    participant U as User
    participant A as AccountController
    participant US as UserService
    participant D as Database

U->>A: a request to open login page
A-->>U: Asks for username and password
U->>A: submits username and password
A->>US: log in (username, password)
US->>D: Asks to look up user in user table
D-->>US: returns user row with password hash
alt username and password correct
US-->>A: success
A-->>U: user login and lands at dashboard
else username and password incorrect
US-->>A: access denied
A-->>U: display error message
end
U->>A: log out
A-->>U: display login page
```


## Relevant Screenshots

### Marketer login landing page with nav features such as user description and log out link

![Marketer login landing page with nav features such as user description and log out link](.scratch/login-logout/images/MarketerLoginLandingPage.png)

### Admin login landing page with nav features such as user description and log out link

![Admin login landing page with nav features such as user description and log out link](.scratch/login-logout/images/AdminLoginLandingPage.png)

### Password Hash SQL

![Password Hash SQL](.scratch/login-logout/images/PasswordHashSQLCheck.png)

### Invalid Username or Password

![Invalid Username or Password](.scratch/login-logout/images/InvalidUserOrPassCheck.png)

### Empty Username or Password

![Empty Username or Password](.scratch/login-logout/images/EmptyUserAndPassCheck.png)

### Logged out

![Logged out](.scratch/login-logout/images/LoggedOut.png)

### Trying to access Dashboard without being logged in

![Trying to access Dashboard without being logged in](.scratch/login-logout/images/NotLoggedInDashboardCheck.png)

### Trying to access Import CSV without being logged in

![Trying to access Import CSV without being logged in](.scratch/login-logout/images/NotLoggedInImportCheck.png)
