using Reloaded.Mod.Interfaces;
using nights.test.nearestneighboursegasaturndreams.Template;
using nights.test.nearestneighboursegasaturndreams.Configuration;
using Reloaded.Hooks.Definitions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Sources;
using Reloaded.Hooks.Definitions.Enums;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;

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
				for (int i = 0x58CE91; i < 0x58CEB8; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				NearestNeighbour3DHook = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x58CE91, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
			}
			// - 2d
			if (_configuration.NearestNeighbour2D) {
				for (int i = 0x5921F9; i < 0x59221F; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x59FC66; i < 0x59FC88; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x5DA21E; i < 0x5DA242; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				for (int i = 0x5D93C7; i < 0x5D93EB; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				NearestNeighbour2DHook = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5921F9, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook2 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x59FC66, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook3 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5DA21E, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
				NearestNeighbour2DHook4 = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5D93C7, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
			}

			// framebuffer filter
			if (_configuration.FBNearestNeighbour) {
				for (int i = 0x5986C3; i < 0x5986E5; ++i) {
					Memory.Instance.SafeWrite(i, nop);
				}
				NearestNeighbourFB = _hooks.CreateAsmHook(
					asmCallNearestNeighbourCode, 0x5986C3, AsmHookBehaviour.DoNotExecuteOriginal
				).Activate();
			}

			if (_configuration.FBWhere != NearestNeighbourWhere.Nowhere) {
				// this allows new framebuffers to be created,
				// probably a memory leak but I don't really care
				Memory.Instance.SafeWrite(0x69AA53, nop);
				Memory.Instance.SafeWrite(0x69AA54, nop);

				MainGameLoopHook = _hooks.CreateHook<MainGameLoop>(MainGameLoopImpl, 0x40A460).Activate();
			}

			_dreamType = (DreamType*)0x8B13C8;
			_smallFB = false;
		}
	}

	private unsafe bool ResizeFramebuffer(int width, int height) {
		// maybe
		var D3D9DeviceMaybe1 = (uint*)0x24C5000;
		if (D3D9DeviceMaybe1 == null) {
			return false;
		}
		var D3D9DeviceMaybe2 = (uint*)(*D3D9DeviceMaybe1 + 0x34);
		if (D3D9DeviceMaybe2 == null) {
			return false;
		}

		// idk what this is, I thought this was the d3d9 device too, but maybe not?
		uint* pointer1 = (uint*)0x24C4FDC;
		if (pointer1 == null) {
			return false;
		}
		// idk what this is either
		uint* pointer2 = (uint*)(*pointer1 + 0xC0);
		if (pointer2 == null) {
			return false;
		}
		// idk what this is but it stores the framebuffer texture
		uint* pointer3 = (uint*)(*pointer2 + 0x24);
		if (pointer3 == null) {
			return false;
		}

		// idk what's going on in case that isn't clear, I trial and errored this

		// call function that then further calls create texture
		// (memory leak)
		var CreateTextureCallerWrapper = _hooks.CreateWrapper<CreateTextureCaller>(0x69AA40, out _);
		CreateTextureCallerWrapper(height, (uint*)*pointer3, width);

		return true;
	}

	[Function(new[] { Register.edi, Register.esi }, Register.eax, StackCleanup.Callee)]
	public unsafe delegate byte CreateTextureCaller(int height, uint* a2, int width2);
	public IHook<CreateTextureCaller> CreateTextureCallerHook;

	private unsafe DreamType* _dreamType;
	private bool _smallFB;

	private Timer _fbTimer;

	[Function(CallingConventions.Cdecl)]
	public unsafe delegate void NearestNeighbourType();
	public IReverseWrapper<NearestNeighbourType> NearestNeighbourReverseWrapper;
	public IAsmHook NearestNeighbour3DHook;
	public IAsmHook NearestNeighbour2DHook;
	public IAsmHook NearestNeighbour2DHook2;
	public IAsmHook NearestNeighbour2DHook3;
	public IAsmHook NearestNeighbour2DHook4;
	public IAsmHook NearestNeighbourFB;
	public unsafe void NearestNeighbour() {
		var idk = *(int**)0x24C4FD4;
		var min_filtering = &idk[9];
		var max_filtering = &idk[10];

		const int nearest = 1;
		const int bilinear = 2;

		var target = bilinear;
		if (_configuration.Where == NearestNeighbourWhere.Everywhere) {
			target = nearest;
		} else if (_configuration.Where == NearestNeighbourWhere.SegaSaturnDreams) {
			if (*_dreamType == DreamType.SegaSaturnDreams) {
				target = nearest;
			}
		}

		if (*min_filtering != target) {
			if (_configuration.MinFilter) {
				*min_filtering = target;
			} else {
				*min_filtering = bilinear;
			}
			idk[25] |= 7; // idk what this does, but it is in the original
		}
		if (*max_filtering != target) {
			*max_filtering = target;
			idk[25] |= 7; // idk what this does, but it is in the original too
		}
	}

	[Function(CallingConventions.MicrosoftThiscall)]
	public unsafe delegate int D3DXCreateTexture(
		uint* pDevice, uint Width, uint Height, uint MipLevels, uint Usage, uint Format, uint Pool, uint* ppTexture
	);

	[Function(CallingConventions.Stdcall)]
	public unsafe delegate int MainGameLoop();
	public IHook<MainGameLoop> MainGameLoopHook;
	// only hooked if fb can be resized
	public unsafe int MainGameLoopImpl() {
		int result = MainGameLoopHook.OriginalFunction();

		// inner window size
		int* width = (int*)0x24A75BC;
		int* height = (int*)0x24A75B8;

		// make fb smaller
		if (
			_smallFB == false
			&& (
				*_dreamType == DreamType.SegaSaturnDreams
				|| _configuration.FBWhere == NearestNeighbourWhere.Everywhere
			)
		) {
			float aspectRatio = (float)*width / *height;
			int newWidth = (int)(aspectRatio * _configuration.FBHeight + 0.5);
			if (ResizeFramebuffer(newWidth, _configuration.FBHeight)) {
				_smallFB = true;
			}
		}
		// make fb bigger
		else if (
			_smallFB == true
			&& *_dreamType != DreamType.SegaSaturnDreams
			&& _configuration.FBWhere != NearestNeighbourWhere.Everywhere
		) {
			if (ResizeFramebuffer(*width, *height)) {
				_smallFB = false;
			}
		}

		return result;
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
