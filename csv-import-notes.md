# CSV Import Notes

**How the import process works:**
- A file is uploaded on the Import CSV page. Here, ImportController.cs checks that a file was actually chosen, the name ends in .csv, and that the file is 5 MB or smaller.
- If any checks fail, an error message is prompted and nothing gets uploaded.
- The header row (e.g. campaign_name, channel, etc.) is checked, and if a column is missing, extra or repeated, the whole file is refused and nothing gets uploaded to the database. Columns can be in any order and in any capitalization, they will get arranged in their correct order.
- Each row is checked on its own and rejected if any value is blank, the date is not in yyyy-MM-dd form, the number isn’t a number or is negative, the channel isn’t one the database already knows, or the campaign already exists but belongs to a different channel.
- Rejected rows are skipped and the import continues. If a row is rejected, the reason it failed along with its line number is output for the user to see.
- Valid rows are saved and inserted. If a campaign already has a result for the date, the row is skipped and counted as a duplicate with the first result staying. If the campaign doesn’t exist yet, it is created and saved on the row’s channel. 
- Campaign dates are continuously updated. If a campaign gets new data, its start and end dates are reset to its earliest and latest result dates.
- After a CSV import, the user sees a summary with the number of rows read, imported and skipped as duplicates, plus a list of rejected rows and their reasons for rejection.

**What CsvHelper does:**
- CsvHelper turns raw file text into rows of named text fields, one row at a time. CsvHelper is set up to have commas split values, trim spaces around each value, and count each line.
- CsvHelper reads the header row and splits it into column names based on where the comma is located (e.g, campaign_name, channel, etc.). 
- CsvHelper goes row by row reading text characters one at a time, splitting values on commas, trimming spaces off each value, and noting which line the row started on.
- CsvHelper is responsible for reporting back information that other code uses to validate if the rows should be valid, rejected, or set as duplicates. For example, CsvHelper hands over the column names and the code checks that there’s exactly 8 of the required ones. If this check fails, the import stops.

**Validation rules set up:**
- File checks (ImportController.cs)
    - Before a file is read, it checks if a file was actually chosen, if the file name ends in .csv (eg., a file ending in .txt fails), and if the file is 5 MB or smaller. Any failures during these checks prompt an error and nothing is imported.
- Header row checks (CsvImportService.cs)
    - Header checks check the first row, ensuring that the header text is readable as a CSV, that the file isn’t empty, that all 8 required columns are there, there are no extra columns, and there are no repeated columns.
    - The 8 required columns can be in any order and in any letter case.
- Row checks (CsvImportService.cs)
    - Row checks run on every row. These checks ensure that:
        - Each row has exactly 8 values that it can map to the right column headers.
        - No row is empty.
        - Each row value is in its correct format (e.g., date is in yyyy-MM-dd or impressions is a whole number)
        - The channel value in the row exists in the channels table.
        - The campaign exists and the row uses the same channel as the campaign (e.g, a “Awareness banner display” campaign uses display as the channel, not search).
	- Failure to pass these checks cause the import to reject the row and skip them.
- If a campaign already has a result for a date, the row is skipped and counted as a duplicate rather than rejected. The first result stays, even if the numbers within it are different.

**How the Import was tested:**
- The import is tested with 12 automated tests in CampaignPerformanceAnalytics.Tests\CsvImportServiceTests.cs.
- The tests utilize a separate test database, running against campaign_analytics_test instead of the real campaign_analytics.
- Before each test, the results and campaigns tables start clean while the channels table is left alone since the channels are always known to stay the same.
- The tests call the same methods used to import into the real campaign_analytics database, ensuring that the real methods pass whenever they’re used for the real import process.
