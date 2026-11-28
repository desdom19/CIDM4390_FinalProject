# Campaign Performance Analytics

Tracks the daily performance of marketing campaigns across channels so their return on spend can be compared on a dashboard.

## Language

**Channel**:
The marketing medium a campaign runs through: email, social, search, or display.
_Avoid_: Platform, medium, source

**Campaign**:
A named marketing effort that runs on exactly one channel over a date range.
_Avoid_: Ad, promotion, initiative

**Daily Result**:
One campaign's spend, impressions, clicks, leads, and revenue on a single date.
_Avoid_: Record, entry, metric, row

**Duplicate Daily Result**:
A Daily Result for a campaign and date that already has one. The first one stands and the duplicate is not imported, even if its figures differ.
_Avoid_: Repeat, double, copy

**ROI**:
Return on investment: (revenue − spend) ÷ spend, for a campaign, a channel, or all campaigns together over a period, always computed from total revenue and total spend. 1.00 means a 100% return; 0 means break-even. A $100 campaign earning $300 and a $10,000 campaign earning $5,000 have an ROI together of (5,300 − 10,100) ÷ 10,100 = −47.5%, not the +75% average of their ROIs.
_Avoid_: ROAS, return (unqualified), average ROI

**ROI Goal**:
The ROI a campaign is expected to reach.
_Avoid_: ROAS goal, target, ROI target

**ROAS**:
Return on ad spend: revenue ÷ spend, for a campaign or a channel over a period. Always one more than ROI for the same figures.
_Avoid_: ROI

**Weak Campaign**:
A campaign whose total revenue over the period is lower than its total spend (ROI below 0).
_Avoid_: Losing campaign, failed campaign

### People and access

**User**:
A person who can log in to see the dashboard and import campaign data. Every User has exactly one Role.
_Avoid_: Account, login (as a noun), member

**Role**:
What kind of User someone is: Marketer or Admin.
_Avoid_: Permission, type, level

**Marketer**:
A User who works with campaign data. For now a Marketer can do everything an Admin can.
_Avoid_: Analyst, standard user

**Admin**:
A User who administers the app. For now an Admin can do everything a Marketer can.
_Avoid_: Administrator, superuser

**Log in / Log out**:
To start or end a User's access to the dashboard and campaign data import.
_Avoid_: Sign in, sign out, sign on
