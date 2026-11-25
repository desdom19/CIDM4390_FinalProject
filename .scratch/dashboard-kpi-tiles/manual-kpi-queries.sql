-- Manual KPI checks for PBI 5 (issue #9), ticket 04.
-- Run against the REAL database, campaign_analytics, by the developer only.
--
-- Every statement in this file is a read-only SELECT. It deliberately holds
-- no DELETE: emptying results and campaigns (step 3) needs separate, explicit
-- approval at the time and is typed by hand, never run from this file.
--
-- Each tile has its own SELECT, deliberately NOT the app's combined query in
-- DashboardService, so a mistake in that query can't hide itself here.

USE campaign_analytics;

-- ---------------------------------------------------------------------------
-- Step 2: read-only row count of results.
-- The clean sample CSV holds 240 Daily Results.
-- ---------------------------------------------------------------------------
SELECT COUNT(*) AS daily_result_count
  FROM results;

-- ---------------------------------------------------------------------------
-- Step 6: one SELECT per tile. Expected values (clean file loaded):
--   Total Spend        45256.13   -> tile $45,256.13
--   Total Revenue      82078.31   -> tile $82,078.31
--   Total Leads        1630       -> tile 1,630
--   Total Impressions  4995183    -> tile 4,995,183
--   Total Clicks       37882      -> tile 37,882
--   Overall ROI        81.4       -> tile 81.4%
-- ---------------------------------------------------------------------------

-- Total Spend
SELECT SUM(spend) AS total_spend
  FROM results;

-- Total Revenue
SELECT SUM(revenue) AS total_revenue
  FROM results;

-- Total Leads
SELECT SUM(leads) AS total_leads
  FROM results;

-- Total Impressions
SELECT SUM(impressions) AS total_impressions
  FROM results;

-- Total Clicks
SELECT SUM(clicks) AS total_clicks
  FROM results;

-- Overall ROI as a percentage to one decimal: (revenue - spend) / spend * 100.
-- Computed from the totals, never as an average of each Campaign's ROI.
-- NULLIF gives NULL instead of a division by zero when total spend is 0.
SELECT ROUND((SUM(revenue) - SUM(spend)) / NULLIF(SUM(spend), 0) * 100, 1)
       AS overall_roi_percent
  FROM results;
