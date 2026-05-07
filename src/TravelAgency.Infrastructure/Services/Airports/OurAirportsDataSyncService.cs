using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using TravelAgency.Infrastructure.Database;
using TravelAgency.Infrastructure.Database.Entities;
using TravelAgency.Infrastructure.Options;

namespace TravelAgency.Infrastructure.Services.Airports;

public sealed class OurAirportsDataSyncService(
	IHttpClientFactory httpClientFactory,
	IOptions<AirportDataSyncOptions> options,
	TravelDbContext db,
	ILogger<OurAirportsDataSyncService> logger) : IAirportDataSyncService
{
	private const string SourceName = "OurAirports";

	public async Task<AirportDataSyncResult> SyncIfNeededAsync(bool force, CancellationToken cancellationToken)
	{
		var opts = options.Value;
		if (!opts.Enabled) return new AirportDataSyncResult(false, true, 0, "Airport data sync is disabled.");

		if (!force)
		{
			var syncReason = await GetSyncReasonAsync(opts, cancellationToken);
			if (syncReason is null)
				return new AirportDataSyncResult(false, true, 0, "Airport data is current.");

			logger.LogInformation("Airport data sync required: {Reason}", syncReason);
		}

		try
		{
			var result = await ImportAsync(opts, cancellationToken);
			await SaveSuccessStatusAsync(result.ImportedCount, result.SourceRecordCount, cancellationToken);
			logger.LogInformation("Airport data sync completed. Imported {ImportedCount} IATA airports from {SourceRecordCount} source rows.",
				result.ImportedCount, result.SourceRecordCount);

			return new AirportDataSyncResult(true, true, result.ImportedCount, "Airport data synced.");
		}
		catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Airport data sync failed.");
			await TrySaveFailureStatusAsync(ex, cancellationToken);
			return new AirportDataSyncResult(true, false, 0, ex.Message);
		}
	}

	private async Task<string?> GetSyncReasonAsync(AirportDataSyncOptions opts, CancellationToken cancellationToken)
	{
		var activeCount = await db.Airports.CountAsync(x => x.IsActive, cancellationToken);
		if (activeCount == 0) return "airport table is empty";

		var missingNormalizedFields = await db.Airports.AnyAsync(
			x => x.IsActive && (x.NameSearch == null || x.CountrySearch == null ||
			                    (x.City != null && x.CitySearch == null)),
			cancellationToken);
		if (missingNormalizedFields) return "airport search normalization fields are missing";

		if (opts.MinimumAirportCount > 0 && activeCount < opts.MinimumAirportCount)
			return $"airport table has only {activeCount} active records";

		var status = await db.AirportDataSyncStatuses.FindAsync(new object[] { SourceName }, cancellationToken);
		if (status?.LastSucceededAtUtc is null) return "airport sync has never completed";

		if (status.ImportedAirportCount > activeCount)
			return $"airport table is missing records from the previous sync ({activeCount}/{status.ImportedAirportCount})";

		var refreshInterval = TimeSpan.FromHours(Math.Max(1, opts.RefreshIntervalHours));
		if (DateTime.UtcNow - status.LastSucceededAtUtc.Value >= refreshInterval)
			return $"airport data is older than {refreshInterval.TotalHours:0} hours";

		return null;
	}

	private async Task<ImportResult> ImportAsync(AirportDataSyncOptions opts, CancellationToken cancellationToken)
	{
		var client = httpClientFactory.CreateClient("ourairports");
		using var countriesStream = new MemoryStream(await client.GetByteArrayAsync(opts.CountriesCsvUrl, cancellationToken));
		using var airportsStream = new MemoryStream(await client.GetByteArrayAsync(opts.AirportsCsvUrl, cancellationToken));

		var countries = LoadCountries(countriesStream, cancellationToken);
		var source = LoadAirports(airportsStream, countries, opts.ImportClosedAirports, cancellationToken);
		var now = DateTime.UtcNow;
		var existing = (await db.Airports.ToListAsync(cancellationToken))
			.ToDictionary(x => x.IataCode, StringComparer.OrdinalIgnoreCase);
		var sourceCodes = source.Airports.Select(x => x.IataCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

		var changed = 0;
		foreach (var airport in source.Airports)
		{
			if (!existing.TryGetValue(airport.IataCode, out var entity))
			{
				entity = new AirportEntity { IataCode = airport.IataCode };
				db.Airports.Add(entity);
			}

			CopyToEntity(airport, entity, now);
			changed++;

			if (changed % Math.Max(1, opts.ImportBatchSize) == 0)
				await db.SaveChangesAsync(cancellationToken);
		}

		foreach (var entity in existing.Values.Where(x => !sourceCodes.Contains(x.IataCode)))
		{
			if (!entity.IsActive) continue;
			entity.IsActive = false;
			entity.LastSyncedAtUtc = now;
			changed++;

			if (changed % Math.Max(1, opts.ImportBatchSize) == 0)
				await db.SaveChangesAsync(cancellationToken);
		}

		await db.SaveChangesAsync(cancellationToken);
		return new ImportResult(source.Airports.Count, source.SourceRecordCount);
	}

	private static void CopyToEntity(SourceAirport source, AirportEntity entity, DateTime now)
	{
		entity.IcaoCode = source.IcaoCode;
		entity.Ident = source.Ident;
		entity.Name = source.Name;
		entity.City = source.City;
		entity.CountryCode = source.CountryCode;
		entity.CountryName = source.CountryName;
		entity.CitySearch = AirportTextNormalizer.Normalize(source.City);
		entity.NameSearch = AirportTextNormalizer.Normalize(source.Name);
		entity.CountrySearch = AirportTextNormalizer.Normalize(source.CountryName);
		entity.AirportType = source.AirportType;
		entity.ScheduledService = source.ScheduledService;
		entity.Latitude = source.Latitude;
		entity.Longitude = source.Longitude;
		entity.IsActive = true;
		entity.LastSyncedAtUtc = now;
	}

	private async Task SaveSuccessStatusAsync(int importedCount, int sourceRecordCount, CancellationToken cancellationToken)
	{
		var status = await db.AirportDataSyncStatuses.FindAsync(new object[] { SourceName }, cancellationToken);
		if (status is null)
		{
			status = new AirportDataSyncStatusEntity { Source = SourceName };
			db.AirportDataSyncStatuses.Add(status);
		}

		var now = DateTime.UtcNow;
		status.LastAttemptedAtUtc = now;
		status.LastSucceededAtUtc = now;
		status.ImportedAirportCount = importedCount;
		status.SourceRecordCount = sourceRecordCount;
		status.ErrorMessage = null;
		await db.SaveChangesAsync(cancellationToken);
	}

	private async Task TrySaveFailureStatusAsync(Exception ex, CancellationToken cancellationToken)
	{
		try
		{
			db.ChangeTracker.Clear();
			var status = await db.AirportDataSyncStatuses.FindAsync(new object[] { SourceName }, cancellationToken);
			if (status is null)
			{
				status = new AirportDataSyncStatusEntity { Source = SourceName };
				db.AirportDataSyncStatuses.Add(status);
			}

			status.LastAttemptedAtUtc = DateTime.UtcNow;
			status.ErrorMessage = ex.Message.Length <= 1024 ? ex.Message : ex.Message[..1024];
			await db.SaveChangesAsync(cancellationToken);
		}
		catch (Exception saveEx)
		{
			logger.LogWarning(saveEx, "Failed to save airport data sync failure status.");
		}
	}

	private static Dictionary<string, string> LoadCountries(Stream stream, CancellationToken cancellationToken)
	{
		using var reader = new StreamReader(stream);
		using var parser = CreateParser(reader);
		var header = ReadHeader(parser);
		var countries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		while (!parser.EndOfData)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var fields = parser.ReadFields();
			if (fields is null) continue;

			var code = Field(fields, header, "code").ToUpperInvariant();
			var name = Field(fields, header, "name");
			if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
				countries[code] = name;
		}

		return countries;
	}

	private static AirportLoadResult LoadAirports(Stream stream, IReadOnlyDictionary<string, string> countries,
		bool importClosedAirports, CancellationToken cancellationToken)
	{
		using var reader = new StreamReader(stream);
		using var parser = CreateParser(reader);
		var header = ReadHeader(parser);
		var airports = new Dictionary<string, SourceAirport>(StringComparer.OrdinalIgnoreCase);
		var sourceRecordCount = 0;

		while (!parser.EndOfData)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var fields = parser.ReadFields();
			if (fields is null) continue;
			sourceRecordCount++;

			var iata = Field(fields, header, "iata_code").ToUpperInvariant();
			if (iata.Length != 3) continue;

			var airportType = Field(fields, header, "type");
			if (!importClosedAirports && string.Equals(airportType, "closed_airport", StringComparison.OrdinalIgnoreCase))
				continue;

			var name = Field(fields, header, "name");
			var countryCode = Field(fields, header, "iso_country").ToUpperInvariant();
			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(countryCode)) continue;

			var countryName = countries.TryGetValue(countryCode, out var resolvedCountryName)
				? PortugueseCountryNames.Resolve(countryCode, resolvedCountryName)
				: PortugueseCountryNames.Resolve(countryCode, countryCode);

			var airport = new SourceAirport(
				iata,
				EmptyToNull(Field(fields, header, "icao_code").ToUpperInvariant()),
				Field(fields, header, "ident").ToUpperInvariant(),
				name,
				EmptyToNull(Field(fields, header, "municipality")),
				countryCode,
				countryName,
				airportType,
				string.Equals(Field(fields, header, "scheduled_service"), "yes", StringComparison.OrdinalIgnoreCase),
				ParseDouble(Field(fields, header, "latitude_deg")),
				ParseDouble(Field(fields, header, "longitude_deg")));

			if (!airports.TryGetValue(iata, out var existing) || Priority(airport) > Priority(existing))
				airports[iata] = airport;
		}

		return new AirportLoadResult(airports.Values.ToList(), sourceRecordCount);
	}

	private static TextFieldParser CreateParser(TextReader reader)
	{
		var parser = new TextFieldParser(reader)
		{
			TextFieldType = FieldType.Delimited,
			HasFieldsEnclosedInQuotes = true,
			TrimWhiteSpace = false
		};
		parser.SetDelimiters(",");
		return parser;
	}

	private static Dictionary<string, int> ReadHeader(TextFieldParser parser)
	{
		var fields = parser.ReadFields() ?? throw new InvalidOperationException("CSV header is missing.");
		return fields.Select((name, index) => new { Name = name, Index = index })
			.ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);
	}

	private static string Field(string[] fields, IReadOnlyDictionary<string, int> header, string name)
	{
		return header.TryGetValue(name, out var index) && index < fields.Length
			? fields[index].Trim()
			: "";
	}

	private static string? EmptyToNull(string value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}

	private static double? ParseDouble(string value)
	{
		return double.TryParse(value, System.Globalization.NumberStyles.Float,
			System.Globalization.CultureInfo.InvariantCulture, out var parsed)
			? parsed
			: null;
	}

	private static int Priority(SourceAirport airport)
	{
		var typeScore = airport.AirportType switch
		{
			"large_airport" => 30,
			"medium_airport" => 20,
			"small_airport" => 10,
			_ => 0
		};

		return (airport.ScheduledService ? 100 : 0) + typeScore;
	}

	private sealed record SourceAirport(
		string IataCode,
		string? IcaoCode,
		string Ident,
		string Name,
		string? City,
		string CountryCode,
		string CountryName,
		string AirportType,
		bool ScheduledService,
		double? Latitude,
		double? Longitude);

	private sealed record AirportLoadResult(List<SourceAirport> Airports, int SourceRecordCount);
	private sealed record ImportResult(int ImportedCount, int SourceRecordCount);
}
