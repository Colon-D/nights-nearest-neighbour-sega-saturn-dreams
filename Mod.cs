using Reloaded.Mod.Interfaces;
using nights.test.nearestneighboursegasaturndreams.Template;
using nights.test.nearestneighboursegasaturndreams.Configuration;
using Reloaded.Hooks.Definitions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Sources;
using Reloaded.Hooks.Definitions.Enums;

namespace nights.test.nearestneighboursegasaturndreams;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
	/// <summary>
	/// Provides access to the mod loader API.
	/// </summary>
	private readonly IModLoader _modLoader;

	/// <summary>
	/// Provides access to the Reloaded.Hooks API.
	/// </summary>
	/// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
	private readonly IReloadedHooks _hooks;

	/// <summary>
	/// Provides access to the Reloaded logger.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// Entry point into the mod, instance that created this class.
	/// </summary>
	private readonly IMod _owner;

	/// <summary>
	/// Provides access to this mod's configuration.
	/// </summary>
	private Config _configuration;

	/// <summary>
	/// The configuration of the currently executing mod.
	/// </summary>
	private readonly IModConfig _modConfig;

	public Mod(ModContext context) {
		_modLoader = context.ModLoader;
		_hooks = context.Hooks;
		_logger = context.Logger;
		_owner = context.Owner;
		_configuration = context.Configuration;
		_modConfig = context.ModConfig;

		unsafe {
			const byte nop = 0x90;
			// call replacement code
			string[] asmCallNearestNeighbourCode = {
				$"use32",
				$"PUSHAD",
				$"{_hooks.Utilities.GetAbsoluteCallMnemonics(NearestNeighbour, out NearestNeighbourReverseWrapper)}",
				$"POPAD",
			};
			// - 3d
			if (_configuration.NearestNeighbour3D) {
				for (int i = 0x58CEA7; i < 0x58CEB8; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				NearestNeighbour3DHook = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x58CEB8, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
			}
			// - 2d
			if (_configuration.NearestNeighbour2D) {
				for (int i = 0x5921F9; i < 0x59221F; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x59FC77; i < 0x59FC88; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x5DA230; i < 0x5DA242; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x5D93D9; i < 0x5D93EB; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				NearestNeighbour2DHook = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5921F9, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook2 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x59FC77, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook3 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5DA230, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook4 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5D93D9, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
			}
		}
	}

	[Function(CallingConventions.Cdecl)]
	public unsafe delegate void NearestNeighbourType();
	public IReverseWrapper<NearestNeighbourType> NearestNeighbourReverseWrapper;
	public IAsmHook NearestNeighbour3DHook;
	public IAsmHook NearestNeighbour2DHook;
	public IAsmHook NearestNeighbour2DHook2;
	public IAsmHook NearestNeighbour2DHook3;
	public IAsmHook NearestNeighbour2DHook4;
	public unsafe void NearestNeighbour() {
		var idk = *(int**)0x24C4FD4;
		var filtering = &idk[10];

		var target = 2;
		if (_configuration.Where == NearestNeighbourWhere.Everywhere) {
			target = 1;
		} else {
			var bnd_ss_xmas = (DreamType*)0x8B13C8;
			if (*bnd_ss_xmas == DreamType.SegaSaturnDreams) {
				target = 1;
			}
		}

		if (*filtering != target) {
			*filtering = target;
			idk[25] |= 7; // idk what this does, but it is in the original
		}
	}

	#region Standard Overrides
	public override void ConfigurationUpdated(Config configuration)
	{
		// Apply settings from configuration.
		// ... your code here.
		_configuration = configuration;
		_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
	}
	#endregion

	#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
}

public enum DreamType {
	BrandNewDreams,
	SegaSaturnDreams,
	ChristmasDreams
}
