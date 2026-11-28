-- Creates the users table in campaign_analytics_test and campaign_analytics.
-- Run as root (MySQL Workbench, or:
--   & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p < .scratch\login-logout\create-users-table.sql
-- ), only after the developer has approved it.
--
-- No seed rows: the app adds the Marketer and the Admin at startup, with hashes
-- of the passwords in User Secrets (Seed:MarketerPassword, Seed:AdminPassword).
-- The users table has no link to the campaign tables.
-- username uses the column's default collation, which ignores case, so "ADMIN"
-- finds "admin" and UNIQUE stops "Admin" being added beside "admin".

CREATE TABLE campaign_analytics_test.users (
    user_id       INT AUTO_INCREMENT PRIMARY KEY,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role          ENUM('marketer', 'admin') NOT NULL
);

CREATE TABLE campaign_analytics.users (
    user_id       INT AUTO_INCREMENT PRIMARY KEY,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role          ENUM('marketer', 'admin') NOT NULL
);

-- Checks: each must show the four columns above, with no rows yet.
DESCRIBE campaign_analytics_test.users;
DESCRIBE campaign_analytics.users;
