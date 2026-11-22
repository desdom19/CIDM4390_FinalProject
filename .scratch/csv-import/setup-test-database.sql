-- Creates campaign_analytics_test as a schema copy of campaign_analytics
-- and seeds the same four Channels. Run as root (MySQL Workbench, or:
--   & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p < .scratch\csv-import\setup-test-database.sql
-- ). Reads campaign_analytics but never changes it.

CREATE DATABASE IF NOT EXISTS campaign_analytics_test;

-- CREATE TABLE ... LIKE copies columns, types, NULL rules, keys and
-- AUTO_INCREMENT, but not foreign keys, so those are added after.
CREATE TABLE campaign_analytics_test.channels  LIKE campaign_analytics.channels;
CREATE TABLE campaign_analytics_test.campaigns LIKE campaign_analytics.campaigns;
CREATE TABLE campaign_analytics_test.results   LIKE campaign_analytics.results;

ALTER TABLE campaign_analytics_test.campaigns
    ADD FOREIGN KEY (channel_id) REFERENCES campaign_analytics_test.channels (channel_id);
ALTER TABLE campaign_analytics_test.results
    ADD FOREIGN KEY (campaign_id) REFERENCES campaign_analytics_test.campaigns (campaign_id);

-- Same four Channels, same IDs.
INSERT INTO campaign_analytics_test.channels
SELECT * FROM campaign_analytics.channels;

GRANT SELECT, INSERT, UPDATE, DELETE
    ON campaign_analytics_test.* TO 'campaign_app_user'@'localhost';

-- Checks: four Channels, and roi_goal must say IS_NULLABLE = YES.
SELECT * FROM campaign_analytics_test.channels;
SELECT COLUMN_NAME, IS_NULLABLE
  FROM information_schema.COLUMNS
 WHERE TABLE_SCHEMA = 'campaign_analytics_test'
   AND TABLE_NAME = 'campaigns'
   AND COLUMN_NAME = 'roi_goal';
