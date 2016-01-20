

using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SDKTemplate
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Pegasus";
        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="New Data", ClassType=typeof(New_Data)},
            new Scenario() { Title="Pending Result", ClassType=typeof(Pending_Result)},
            new Scenario() { Title="History Result", ClassType=typeof(Previous_Result)},
            new Scenario() { Title="Settings", ClassType=typeof(Settings)},
            new Scenario() { Title="Help & About", ClassType=typeof(Help)},
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
