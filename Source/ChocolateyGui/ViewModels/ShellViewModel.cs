﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="ShellViewModel.cs">
//   Copyright 2014 - Present Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Caliburn.Micro;
using ChocolateyGui.Models.Messages;
using ChocolateyGui.Providers;
using ChocolateyGui.Services;
using ChocolateyGui.Utilities;
using ChocolateyGui.ViewModels.Items;

namespace ChocolateyGui.ViewModels
{
    public class ShellViewModel : Conductor<object>.Collection.OneActive,
        IHandle<ShowPackageDetailsMessage>,
        IHandle<ShowSourcesMessage>,
        IHandle<ShowSettingsMessage>,
        IHandle<SettingsGoBackMessage>
    {
        private readonly IChocolateyService _chocolateyPackageService;
        private readonly IVersionNumberProvider _versionNumberProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly SourcesViewModel _sourcesViewModel;
        private object _lastActiveItem;

        public ShellViewModel(
            IChocolateyService chocolateyPackageService,
            IVersionNumberProvider versionNumberProvider,
            IEventAggregator eventAggregator,
            SourcesViewModel sourcesViewModel)
        {
            _chocolateyPackageService = chocolateyPackageService;
            _versionNumberProvider = versionNumberProvider;
            _eventAggregator = eventAggregator;
            _sourcesViewModel = sourcesViewModel;
            Sources = new BindableCollection<SourceViewModel>();
            ActiveItem = _sourcesViewModel;

            GetSources();
        }

        public string AboutInformation
            => ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.ABOUT.md");

        public string ReleaseNotes
            => ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.CHANGELOG.md");

        public string Credits
            => ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.CREDITS.md");

        public string VersionNumber => _versionNumberProvider.Version;

        public BindableCollection<SourceViewModel> Sources { get; set; }

        public SourcesViewModel SourcesSelectorViewModel => _sourcesViewModel;

        public void Handle(ShowPackageDetailsMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Package == null)
            {
                throw new ArgumentNullException(nameof(message.Package));
            }

            var packageViewModel = ActiveItem as PackageViewModel;
            if (packageViewModel != null && string.Equals(packageViewModel.Package.Id, message.Package.Id, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var packageVm = IoC.Get<PackageViewModel>();
            packageVm.Package = message.Package;
            SetActiveItem(packageVm);
        }

        public void Handle(ShowSourcesMessage message)
        {
            SetActiveItem(_sourcesViewModel);
        }

        public void Handle(ShowSettingsMessage message)
        {
            ShowSettings();
        }

        public void Handle(SettingsGoBackMessage message)
        {
            SetActiveItem(_lastActiveItem);
        }

        public void ShowSettings()
        {
            if (ActiveItem is SettingsViewModel)
            {
                return;
            }

            SetActiveItem(IoC.Get<SettingsViewModel>());
        }

        protected override void OnInitialize()
        {
            _eventAggregator.Subscribe(this);
        }

        private void SetActiveItem<T>(T newItem)
        {
            if (_lastActiveItem != null && _lastActiveItem.Equals(newItem))
            {
                _lastActiveItem = null;
            }
            else
            {
                _lastActiveItem = ActiveItem;
            }

            ActivateItem(newItem);
            if (_lastActiveItem is PackageViewModel)
            {
                this.CloseItem(_lastActiveItem);
            }
        }

        private async void GetSources()
        {
            var sources =
                (await _chocolateyPackageService.GetSources()).Select(
                    source => new SourceViewModel { Name = source.Id, Url = source.Value });
            Sources.AddRange(sources);
        }
    }
}