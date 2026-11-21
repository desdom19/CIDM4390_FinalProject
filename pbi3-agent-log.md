## PBI 3 Agent log

**Tool:** 
Claude Agentic Coding was used to generate both the clean and error CSVs. 

**Process used:** 
The grill-with-docs process was used to turn PBI 3 into a spec and two tickets. This was followed by the implementation that generated the sample data in both CSVs.

**What it generated:** 
- campaign_results_clean.csv
- campaign_results_errors.csv
- spec.md 
- verification-report-clean.md
- verification-report-errors.md
- .scratch/sample-campaign-data/
- issues/01-clean-campaign-results-csv.md
- issues/02-error-campaign-results-csv.md

**How the data generation was checked:** 
Functions such as "(Import-Csv sample-data\campaign_results_errors.csv).Count" were used to verify correct row counts. Defect lines were verified manually by opening the file and using CTRL+G to find the defect lines.

**Decisions made:** 
- The agent flagged a mismatch during the grill-with-docs process. The sample data was being built around revenue / spend (ROAS) but the schema column is roi_goal (ROI). The agent’s suggestion was to treat roi_goal as a ROAS target. If left unresolved, the future dashboard could compare a ROAS value against an ROI goal. I chose to keep roi_goal as true ROI ((revenue - spend) / spend) because it matched the column name and the planned dashboard formula.
- .scratch/ will be committed as proof of the process.

**What was learned:** 
The difference between an ROI and ROAS. While a ROAS is measured by dividing revenue / spend, an ROI is measured by subtracting spend from revenue then dividing by spend. A ROAS measures revenue from a more isolated standpoint, only taking into account ad spend in relation to it. In general, an ROI measures profit by accounting for costs beyond ad spend, but in this project it uses only ad spend. Put another way, ROI = ROAS - 1.  
