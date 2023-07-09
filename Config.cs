using System.ComponentModel;
using nights.test.nearestneighboursegasaturndreams.Template.Configuration;

namespace nights.test.nearestneighboursegasaturndreams.Configuration;

public class Config : Configurable<Config>
{
	/*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

	[DisplayName("3D Models")]
    [Description("3D Models with have Nearest-Neighbour sampling on their textures.")]
	[DefaultValue(true)]
    public bool NearestNeighbour3D { get; set; } = true;
	[DisplayName("2D UI")]
	[Description("2D UI with have Nearest-Neighbour sampling on their textures.")]
	[DefaultValue(true)]
    public bool NearestNeighbour2D { get; set; } = true;
	[DisplayName("Where")]
	[Description("Where Nearest-Neighbour sampling will be applied.")]
	[DefaultValue(NearestNeighbourWhere.SegaSaturnDreams)]
    public NearestNeighbourWhere Where { get; set; } = NearestNeighbourWhere.SegaSaturnDreams;
}

public enum NearestNeighbourWhere {
	SegaSaturnDreams,
	Everywhere
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
