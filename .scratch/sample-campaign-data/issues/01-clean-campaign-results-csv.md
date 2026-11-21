# 01: Clean campaign results CSV

**What to build:** A clean sample CSV of Daily Results in the `sample-data` folder at the repo root, so the developer has known-good data for building the CSV import and the dashboard. It covers 8 Campaigns (2 per Channel) for 30 days, with realistic marketing numbers: 6 profitable Campaigns and 2 Weak Campaigns. It's produced with Claude agentic coding (a seeded generator kept in the scratchpad, not committed), checked by a throwaway verification script that reads only the finished file, and committed after the developer reviews the report. See the parent spec: `.scratch/sample-campaign-data/spec.md`.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

## Data

- File name: `campaign_results_clean.csv`
- Header: `campaign_name,channel,date,spend,impressions,clicks,leads,revenue`
- Channels (lowercase, matching the seeded `channel_name` values): `email`, `social`, `search`, `display`
- Campaigns:
  - email: Fall Newsletter Email, Cart Reminder Email
  - social: Back to School Social, Influencer Promo Social (Weak)
  - search: Brand Keywords Search, Competitor Keywords Search
  - display: Retargeting Display, Awareness Banner Display (Weak)
- Dates 2026-09-01 → 2026-09-30, one Daily Result per Campaign per day, sorted by Campaign then date
- Realism: each Channel has its own spend, CTR, lead rate and value per lead; daily noise; a weekend dip; revenue comes from leads
- ROI = (revenue − spend) ÷ spend over 30-day totals. Strong Campaigns target about 0.50–3.00, Weak about −0.40 to −0.15. Individual days may go either way.

## Acceptance criteria

- [ ] The file exists in `sample-data/` at the repo root, UTF-8 without BOM, with the exact header above and no quoted fields
- [ ] There are 240 data rows, not counting the header
- [ ] There are 4 Channels with 2 Campaigns each, for 8 distinct Campaigns, and each Campaign has exactly 30 Daily Results covering 2026-09-01 → 2026-09-30
- [ ] Dates are ISO `yyyy-MM-dd`; spend/revenue are plain decimals with 2 places; impressions/clicks/leads are non-negative integers
- [ ] No row has clicks > impressions
- [ ] No row has leads > clicks
- [ ] No row has leads = 0 with revenue > 0
- [ ] 6 Campaigns have 30-day total revenue **strictly greater** than total spend
- [ ] 2 Campaigns (Influencer Promo Social, Awareness Banner Display) have 30-day total revenue **lower** than total spend
- [ ] The verification report prints the actual figure for every criterion above next to the expected one, including a per-Campaign table of total spend, total revenue and ROI, and the developer has reviewed it
- [ ] The file is committed on `5-pbi-3-feature-sample-campaign-data-creation`, with a commit message referencing #5
