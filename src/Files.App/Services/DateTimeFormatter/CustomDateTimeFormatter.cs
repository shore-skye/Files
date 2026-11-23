// Copyright (c) Files Community
// Licensed under the MIT License.

namespace Files.App.Services.DateTimeFormatter
{
	internal sealed class CustomDateTimeFormatter : AbstractDateTimeFormatter
	{
		/// <summary>
		/// ISO 8601 calendar date.
		/// </summary>
		public const string DefaultFormat = "yyyy-MM-dd";

		private readonly IUserSettingsService userSettingsService = Ioc.Default.GetRequiredService<IUserSettingsService>();

		public override string Name
			=> Strings.Custom.GetLocalizedResource();

		public override string ToShortLabel(DateTimeOffset offset)
		{
			if (offset.Year is <= 1601 or >= 9999)
				return " ";

			var format = userSettingsService.GeneralSettingsService.CustomDateTimeFormat;
			if (string.IsNullOrWhiteSpace(format))
				format = DefaultFormat;

			try
			{
				return ToString(offset, format);
			}
			catch (FormatException)
			{
				return ToString(offset, DefaultFormat);
			}
		}
	}
}
