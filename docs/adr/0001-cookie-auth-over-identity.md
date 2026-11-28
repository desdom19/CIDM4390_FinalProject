# Cookie authentication with our own users table, not ASP.NET Core Identity

Logging in uses ASP.NET Core's built-in cookie authentication, checked against a hand-made `users` table (`username`, `password_hash`, `role`) read with MySqlConnector, and passwords are hashed with the built-in `PasswordHasher<T>` (salted PBKDF2). We chose this over ASP.NET Core Identity because Identity brings EF Core and its own `AspNet*` tables. That would not match the users table the PBI asks for or the hand-written SQL the rest of the app uses, and the app needs only two seeded Users with no registration, password reset, or lockout. Neither choice needs a new package.

## Consequences

- Features Identity gives for free, such as lockout after failed attempts, password reset, and registration, would have to be written by hand if they are ever needed. If several of them become needed, that is the time to look at Identity again.
- Moving to Identity later means replacing the `users` table and seeding the Users again. The stored hashes use the same format as Identity's, so they could be copied over.
