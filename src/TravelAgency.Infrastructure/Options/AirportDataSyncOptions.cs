namespace TravelAgency.Infrastructure.Options;

public sealed class AirportDataSyncOptions
{
	public const string SectionName = "AirportDataSync";

	public bool Enabled { get; set; } = true;
	public bool SyncOnStartup { get; set; } = true;
	public int StartupDelaySeconds { get; set; } = 5;
	public int RefreshIntervalHours { get; set; } = 24;
	public int PeriodicCheckHours { get; set; } = 6;
	public int RequestTimeoutSeconds { get; set; } = 90;
	public int ImportBatchSize { get; set; } = 500;
	public int MinimumAirportCount { get; set; } = 5000;
	public bool ImportClosedAirports { get; set; }
	public string AirportsCsvUrl { get; set; } =
		"https://davidmegginson.github.io/ourairports-data/airports.csv";
	public string CountriesCsvUrl { get; set; } =
		"https://davidmegginson.github.io/ourairports-data/countries.csv";
}
