// Copyright (c) Files Community
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace Files.App.Data.Items
{
	/// <summary>
	/// Represents item for a column shown in <see cref="DetailsLayoutPage"/>.
	/// </summary>
	[WinRT.GeneratedBindableCustomProperty([nameof(LengthPixels), nameof(LengthIncludingGridSplitterPixels), nameof(Visibility)], [])]
	public sealed partial class DetailsLayoutColumnItem : ObservableObject
	{
		public const int GRID_SPLITTER_WIDTH = 12;

		public double UserLengthPixels
		{
			get => UserLength.Value;
			set => UserLength = new GridLength(value, GridUnitType.Pixel);
		}

		private double _NormalMaxLength = 800;

		[RegistryIgnore]
		public double NormalMaxLength
		{
			get => _NormalMaxLength;
			set => SetProperty(ref _NormalMaxLength, value);
		}

		private bool _UserCollapsed;
		public bool UserCollapsed
		{
			get => _UserCollapsed;
			set
			{
				if (SetProperty(ref _UserCollapsed, value))
					UpdateVisibility();
			}
		}

		[RegistryIgnore]
		public bool IsResizable { get; set; } = true;

		private bool _IsCollapsedToFit;

		/// <summary>
		/// Gets or sets whether the column is temporarily hidden so the remaining columns fit the view. Not persisted.
		/// </summary>
		[RegistryIgnore, JsonIgnore]
		public bool IsCollapsedToFit
		{
			get => _IsCollapsedToFit;
			set
			{
				if (SetProperty(ref _IsCollapsedToFit, value))
					UpdateVisibility();
			}
		}

		private double _FitLength = double.NaN;

		/// <summary>
		/// Gets or sets the width the column is temporarily narrowed to so the columns fit the view, or NaN when not narrowed. Not persisted.
		/// </summary>
		[RegistryIgnore, JsonIgnore]
		public double FitLength
		{
			get => _FitLength;
			set
			{
				if (SetProperty(ref _FitLength, value))
					UpdateVisibility();
			}
		}

		private bool IsCollapsed
			=> UserCollapsed || IsHidden || IsCollapsedToFit;

		private GridLength EffectiveLength
			=> double.IsNaN(FitLength) || UserLength.GridUnitType != GridUnitType.Pixel || FitLength >= UserLength.Value
				? UserLength
				: new(FitLength, GridUnitType.Pixel);

		public double MinLength
			=> IsCollapsed ? 0 : NormalMinLength;

		public Visibility Visibility
			=> IsCollapsed ? Visibility.Collapsed : Visibility.Visible;

		public GridLength Length
			=> IsCollapsed ? new GridLength(0) : EffectiveLength;

		[RegistryIgnore, JsonIgnore]
		public double LengthPixels => Length.Value;

		public GridLength LengthIncludingGridSplitter =>
			IsCollapsed
				? new(0)
				: new(EffectiveLength.Value + (IsResizable ? GRID_SPLITTER_WIDTH : 0));

		[RegistryIgnore, JsonIgnore]
		public double LengthIncludingGridSplitterPixels => LengthIncludingGridSplitter.Value;

		public double MaxLength
			=> IsCollapsed ? 0 : NormalMaxLength;

		private bool _IsHidden;

		public bool IsHidden
		{
			get => _IsHidden;
			set => SetProperty(ref _IsHidden, value);
		}

		private double _NormalMinLength = 50;

		[RegistryIgnore]
		public double NormalMinLength
		{
			get => _NormalMinLength;
			set
			{
				if (SetProperty(ref _NormalMinLength, value))
					OnPropertyChanged(nameof(MinLength));
			}
		}

		private GridLength _UserLength = new(200, GridUnitType.Pixel);

		[RegistryIgnore]
		public GridLength UserLength
		{
			get => _UserLength;
			set
			{
				if (SetProperty(ref _UserLength, value))
				{
					OnPropertyChanged(nameof(Length));
					OnPropertyChanged(nameof(LengthPixels));
					OnPropertyChanged(nameof(LengthIncludingGridSplitter));
					OnPropertyChanged(nameof(LengthIncludingGridSplitterPixels));
				}
			}
		}

		public void Hide()
		{
			IsHidden = true;
			UpdateVisibility();
		}

		public void Show()
		{
			IsHidden = false;
			UpdateVisibility();
		}

		private void UpdateVisibility()
		{
			OnPropertyChanged(nameof(Length));
			OnPropertyChanged(nameof(LengthPixels));
			OnPropertyChanged(nameof(LengthIncludingGridSplitter));
			OnPropertyChanged(nameof(LengthIncludingGridSplitterPixels));
			OnPropertyChanged(nameof(MaxLength));
			OnPropertyChanged(nameof(Visibility));
			OnPropertyChanged(nameof(MinLength));
		}

		// Width ignoring the temporary fit-to-view state, so fitting never makes saved layouts look changed.
		private double PersistedLength
			=> UserCollapsed || IsHidden ? 0 : UserLength.Value;

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;

			if (obj == this)
				return true;

			if (obj is DetailsLayoutColumnItem model)
			{
				return
					model.UserCollapsed == UserCollapsed &&
					model.PersistedLength == PersistedLength &&
					model.IsResizable == IsResizable &&
					model.UserLength.Value == UserLength.Value;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			var hashCode = UserCollapsed.GetHashCode();
			hashCode = (hashCode * 397) ^ PersistedLength.GetHashCode();
			hashCode = (hashCode * 397) ^ IsResizable.GetHashCode();
			hashCode = (hashCode * 397) ^ UserLength.Value.GetHashCode();

			return hashCode;
		}

		public void Update(DetailsLayoutColumnItem other)
		{
			UserLengthPixels = other.UserLengthPixels;
			NormalMaxLength = other.NormalMaxLength;
			UserCollapsed = other.UserCollapsed;
			IsResizable = other.IsResizable;
			IsHidden = other.IsHidden;
			NormalMinLength = other.NormalMinLength;
			UserLength = other.UserLength;
		}
	}
}
