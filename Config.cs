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

	[DisplayName("Min Filter")]
	[Description("Should things smaller than their default pixel size be pixelated (true), or blurry (false).")]
	[DefaultValue(true)]
	public bool MinFilter { get; set; } = true;

	[Category("Framebuffer")]
	[DisplayName("Lower Size Where?")]
	[Description("Where the framebuffer size should be lower")]
	[DefaultValue(NearestNeighbourWhere.SegaSaturnDreams)]
	public NearestNeighbourWhere FBWhere { get; set; } = NearestNeighbourWhere.SegaSaturnDreams;
	[Category("Framebuffer")]
	[DisplayName("Height")]
	[Description("The height that the framebuffer should be. Width will be calculated from this.")]
	[DefaultValue(240)]
	public int FBHeight { get; set; } = 240;
	[Category("Framebuffer")]
	[DisplayName("Nearest Neighbour")]
	[Description(
		"Should the framebuffer be pixelated (true), or blurry (false).\n"
		+ "\"Where\" still applies, both conditions must be true."
	)]
	[DefaultValue(true)]
	public bool FBNearestNeighbour { get; set; } = true;
}

public enum NearestNeighbourWhere {
    Nowhere,
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
