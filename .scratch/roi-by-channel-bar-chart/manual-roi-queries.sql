-- Manual ROI by Channel checks for PBI 6 (issue #11), ticket 04.
-- Run against the REAL database, campaign_analytics, by the developer only.
--
-- Every statement in this file is a read-only SELECT. It deliberately holds
-- no DELETE: emptying results and campaigns (step 3) needs separate, explicit
-- approval at the time and is typed by hand, never run from this file.
--
-- Each Channel has its own SELECT, filtered by Channel name. This is
-- deliberately NOT the app's grouped query in DashboardService, so a mistake
-- in that query can't hide itself here.

USE campaign_analytics;

-- ---------------------------------------------------------------------------
-- Step 2: read-only row count of results.
-- The clean sample CSV holds 240 Daily Results.
-- ---------------------------------------------------------------------------
SELECT COUNT(*) AS daily_result_count
  FROM results;

-- ---------------------------------------------------------------------------
-- Step 6: one SELECT per Channel. Expected values (clean file loaded):
--   Channel   Spend      Revenue    ROI %   Bar label
--   email     2875.19    7333.83    155.1   155.1%
--   search    18451.80   42691.58   131.4   131.4%
--   display   10178.85   14897.67   46.4    46.4%
--   social    13750.29   17155.23   24.8    24.8%
-- Bar order, top to bottom: Email, Search, Display, Social.
--
-- ROI is (revenue - spend) / spend * 100, to one decimal, from the Channel's
-- totals, never as an average of its Campaigns' ROIs.
-- NULLIF gives NULL instead of a division by zero when total spend is 0.
-- ---------------------------------------------------------------------------

-- Email
SELECT SUM(r.spend)   AS total_spend,
       SUM(r.revenue) AS total_revenue,
       ROUND((SUM(r.revenue) - SUM(r.spend)) / NULLIF(SUM(r.spend), 0) * 100, 1)
                      AS roi_percent
  FROM results r
  JOIN campaigns c ON c.campaign_id = r.campaign_id
  JOIN channels ch ON ch.channel_id = c.channel_id
 WHERE ch.channel_name = 'email';

-- Social
SELECT SUM(r.spend)   AS total_spend,
       SUM(r.revenue) AS total_revenue,
       ROUND((SUM(r.revenue) - SUM(r.spend)) / NULLIF(SUM(r.spend), 0) * 100, 1)
                      AS roi_percent
  FROM results r
  JOIN campaigns c ON c.campaign_id = r.campaign_id
  JOIN channels ch ON ch.channel_id = c.channel_id
 WHERE ch.channel_name = 'social';

-- Search
SELECT SUM(r.spend)   AS total_spend,
       SUM(r.revenue) AS total_revenue,
       ROUND((SUM(r.revenue) - SUM(r.spend)) / NULLIF(SUM(r.spend), 0) * 100, 1)
                      AS roi_percent
  FROM results r
  JOIN campaigns c ON c.campaign_id = r.campaign_id
  JOIN channels ch ON ch.channel_id = c.channel_id
 WHERE ch.channel_name = 'search';

-- Display
SELECT SUM(r.spend)   AS total_spend,
       SUM(r.revenue) AS total_revenue,
       ROUND((SUM(r.revenue) - SUM(r.spend)) / NULLIF(SUM(r.spend), 0) * 100, 1)
                      AS roi_percent
  FROM results r
  JOIN campaigns c ON c.campaign_id = r.campaign_id
  JOIN channels ch ON ch.channel_id = c.channel_id
 WHERE ch.channel_name = 'display';
