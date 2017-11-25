﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="MainWindowViewModel.cs">
//   Copyright 2014 - Present Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ChocolateyGui.ViewModels.Windows
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using ChocolateyGui.Base;
    using ChocolateyGui.Models;
    using ChocolateyGui.Providers;
    using ChocolateyGui.Services;
    using ChocolateyGui.Utilities;
    using ChocolateyGui.ViewModels.Items;

    public class MainWindowViewModel : ObservableBase, IMainWindowViewModel, IWeakEventListener
    {
        private readonly IProgressService _progressService;
        private readonly ISourceService _sourceService;
        private readonly IVersionNumberProvider _versionNumberProvider;

        private string _newSourceName;
        private string _newSourceUrl;
        private SourceViewModel _selectedSourceViewModel;

        public MainWindowViewModel(ISourceService sourceService, IProgressService progressService, IVersionNumberProvider versionNumberProvider)
        {
            if (sourceService == null)
            {
                throw new ArgumentNullException("sourceService");
            }

            this._sourceService = sourceService;
            this._progressService = progressService;
            this._versionNumberProvider = versionNumberProvider;
            this.Sources = new ObservableCollection<SourceViewModel>(this._sourceService.GetSources());

            SourcesChangedEventManager.AddListener(sourceService, this);
        }

        public bool IsSelectedSource
        {
            get { return this._selectedSourceViewModel != null; }
        }

        public string NewSourceName
        {
            get { return this._newSourceName; }
            set { this.SetPropertyValue(ref this._newSourceName, value); }
        }

        public string NewSourceUrl
        {
            get { return this._newSourceUrl; }
            set { this.SetPropertyValue(ref this._newSourceUrl, value); }
        }

        public string AboutInformation
        {
            get
            {
                return ResourceReader.GetFromResources(this.GetType().Assembly, "ChocolateyGui.Resources.ABOUT.md");
            }
        }

        public string ReleaseNotes
        {
            get
            {
                return ResourceReader.GetFromResources(this.GetType().Assembly, "ChocolateyGui.Resources.CHANGELOG.md");
            }
        }

        public string Credits
        {
            get
            {
                return ResourceReader.GetFromResources(this.GetType().Assembly, "ChocolateyGui.Resources.CREDITS.md");
            }
        }

        public string VersionNumber
        {
            get
            {
                return this._versionNumberProvider.Version;
            }
        }

        public SourceViewModel SelectedSource
        {
            get
            {
                return this._selectedSourceViewModel;
            }

            set
            {
                this.SetPropertyValue(ref this._selectedSourceViewModel, value);
                this.NotifyPropertyChanged("IsSelectedSource");
            }
        }

        public ObservableCollection<SourceViewModel> Sources { get; set; }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        public async void AddSource()
        {
            if (string.IsNullOrWhiteSpace(this.NewSourceName))
            {
                this._progressService.ShowMessageAsync("New Source", "Source must have a name.").ConfigureAwait(false);
                return;
            }

            if (this.Sources.Any(s => string.Compare(s.Name, this.NewSourceName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                this._progressService.ShowMessageAsync("New Source", "There's already a source with that name.").ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(this.NewSourceUrl))
            {
                this._progressService.ShowMessageAsync("New Source", "Source must have a Url.").ConfigureAwait(false);
                return;
            }

            if (this.Sources.Any(s => string.Compare(s.Url, this.NewSourceUrl, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                this._progressService.ShowMessageAsync("New Source", "There's already a source with that url.").ConfigureAwait(false);
                return;
            }

            Uri url;
            if (!Uri.TryCreate(this.NewSourceUrl, UriKind.Absolute, out url))
            {
                this._progressService.ShowMessageAsync("New Source", "Source url is malformed.").ConfigureAwait(false);
                return;
            }

            this._sourceService.AddSource(new SourceViewModel { Name = this.NewSourceName, Url = this.NewSourceUrl });
            this.NewSourceName = string.Empty;
            this.NewSourceUrl = string.Empty;
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        public bool CanAddSource()
        {
            if (string.IsNullOrWhiteSpace(this.NewSourceName))
            {
                return false;
            }

            if (
                this.Sources.Any(
                    s => string.Compare(s.Name, this.NewSourceName, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.NewSourceUrl))
            {
                return false;
            }

            if (
                this.Sources.Any(
                    s => string.Compare(s.Url, this.NewSourceUrl, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return false;
            }

            Uri url;
            if (!Uri.TryCreate(this.NewSourceUrl, UriKind.Absolute, out url))
            {
                return false;
            }

            return true;
        }

        public bool CanRemoveSource()
        {
            return this.IsSelectedSource;
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender is ISourceService)
            {
                var eventArgs = e as SourcesChangedEventArgs;
                if (eventArgs != null && (eventArgs.AddedSources != null && eventArgs.AddedSources.Count > 0))
                {
                    foreach (var source in eventArgs.AddedSources)
                    {
                        this.Sources.Add(source);
                    }
                }

                if (eventArgs != null && (eventArgs.RemovedSources != null && eventArgs.RemovedSources.Count > 0))
                {
                    foreach (var source in eventArgs.RemovedSources)
                    {
                        this.Sources.Remove(source);
                    }
                }
            }

            return true;
        }

        public void RemoveSource()
        {
            if (this.SelectedSource == null)
            {
                this._progressService.ShowMessageAsync("Remove Source", "There's no selected source.");
                return;
            }

            if (this.Sources.Count < 2)
            {
                this._progressService.ShowMessageAsync("Remove Source", "You must have at least one source.");
                return;
            }

            var oldSource = this.SelectedSource;
            this.SelectedSource = null;
            this._sourceService.RemoveSource(oldSource);
        }
    }
}