# Dashboard Tile Creation Notes

**How each KPI tile is derived:**
- Each KPI total (spend, revenue, leads, impressions, clicks) is calculated from the results SQL table. They're summed over the entirety of all campaigns and channels.
- Each of the five totals are reads in a single SELECT query instead of five separate ones. By doing so, the totals are derived from the same snapshot of data, preventing inconsistencies that could occur from new imports in between queries.
- Since a sum over zero rows gives NULL instead of 0, each sum is wrapped in COALESCE (..., 0), so an empty results table gives 0 instead. 
- ROI is calculated in C# outside of SQL utilizing (total revenue − total spend) ÷ total spend. Overall ROI is not calculated from the average of each campaign's ROI because doing so would give a higher spend campaign the same weight as a campaign with lower spend. Instead, overall ROI is calculated from the spend and revenue totals.
- If total spend is 0, ROI has no value since nothing can be divided by 0. The tile then shows "N/A (spend is 0)", even if there is a revenue number.

**How each KPI tile is formatted:**
- KPI tiles such as total spend and total revenue are formatted in dollar currency. A '$" is followed by the number with commas and two decimals.
- KPI tiles such as total leads, total impressions, and total clicks are formatted as whole numbers with no commas and no decimals.
- The overall ROI KPI tile is formatted as a fraction and then multiplied by 100 to be converted into a percentage. It is rounded to one decimal and the number is followed by a '%'.
- The formatter uses the invariant culture, a fixed set of formatting rules that do not change with the user's computer language or region settings. Keeping the same set of formatting rules allows tests that compare exact strings to pass (e.g., $45,256.13).

**How the KPI tiles were verified manually:**
1. The results and campaigns tables were emptied manually by doing the following:
    - USE campaign_analytics;
    SELECT COUNT(*) FROM results;
    SELECT COUNT(*) FROM campaigns;
2. The dashboard was opened with the tables empty. The dashboard displayed $0.00, $0.00, 0, 0, 0 across the five total tiles. It also displayed "N/A (spend is 0)" on the ROI tile. A hint was also printed above the tiles stating, "No Daily Results yet. Import a CSV to see your totals." with an import CSV link that led to the Import CSV page.
3. The clean sample CSV file was imported through the Import CSV page. The import summary showed 240 imported, 0 duplicates and 0 rejected.
4. The dashboard was reopened with the clean CSV file imported and every tile matched its expected value: $45,256.13, $82,078.31, 1,630, 4,995,183, 37,882 and 81.4%. 
5. Each KPI tile was verified manually with the following SQL queries:
    - USE campaign_analytics;
    SELECT SUM(spend) FROM results;   
    SELECT SUM(revenue) FROM results;  
    SELECT SUM(leads) FROM results;  
    SELECT SUM(impressions) FROM results;  
    SELECT SUM(clicks) FROM results;    
    SELECT ROUND((SUM(revenue) - SUM(spend)) / SUM(spend) * 100, 1) 
    FROM results;                
6. The results and campaigns table count were verified to be 240 and 8 respectively through the following manual SQL queries:
    - USE campaign_analytics;
    SELECT COUNT(*) FROM results;
    SELECT COUNT(*) FROM campaigns;