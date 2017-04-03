// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="AppConfiguration.cs">
//   Copyright 2014 - Present Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace ChocolateyGui.Models
{
    public class AppConfiguration
    {
        public string Id { get; set; }

        public bool ShowConsoleOutput { get; set; }

        public bool DefaultToTileViewForLocalSource { get; set; }

        public bool DefaultToTileViewForRemoteSource { get; set; }

        public bool AutomaticCheck { get; set; }

        public bool AutomaticUpdate { get; set; }

        /// <summary>
        /// Gets or sets the automatic period, in hours.
        /// </summary>
        /// <value>
        /// The automatic period, in hours.
        /// </value>
        public int AutomaticPeriod { get; set; } = 24;

        public TimeSpan AutomaticTimespan => TimeSpan.FromHours(AutomaticPeriod);
    }
}