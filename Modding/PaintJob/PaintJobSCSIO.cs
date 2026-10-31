using SCS_Mod_Helper.Class.Base;
using SCS_Mod_Helper.Utils;
using System.IO;
using System.Windows.Media;

namespace SCS_Mod_Helper.Modding.PaintJob; 
public class PaintJobSCSIO(): SCSIO {
	protected const string HeaderAccPaintJob = "accessory_paint_job_data";
	private const string NameDisplayName = "name";
	private const string NamePrice = "price";
	private const string NameUnlock = "unlock";
	private const string NameIcon = "icon";
	private const string NamePartType = "part_type";
	private const string NameSuitableFor = "suitable_for";
	private const string NameConflictWith = "conflict_with";
	private const string NameDefaults = "defaults";
	private const string NameOverrides = "overrides";
	private const string NameRequire = "require";
	private const string NameSyncOverNetwork = "sync_over_network";

	private const string NameBaseColor = "base_color";
	private const string NameBaseColorLocked = "base_color_locked";
	private const string NamePaintJobMask = "paint_job_mask";
	private const string NameBaseTextureOverride = "base_texture_override";
	private const string NameAlternateUVSet = "alternate_uvset";

	private const string NameAirbrush = "airbrush";
	private const string NameFlipFlake = "flipflake";
	private const string NameAlternateFlipFlakeUVSet = "alternate_flipflake_uvset";
	private const string NameFlipColor = "flip_color";
	private const string NameFlipColorLocked = "flip_color_locked";
	private const string NameFlakeColor = "flake_color";
	private const string NameFlakeColorLocked = "flake_color_locked";
	private const string NameFlipStrength = "flip_strength";
	private const string NameFlakeShininess = "flake_shininess";
	private const string NameFlakeDensity = "flake_density";
	private const string NameFlakeClearcoatRolloff = "flake_clearcoat_rolloff";
	private const string NameFlakeUVScale = "flake_uvscale";
	private const string NameFlakeVRatio = "flake_vratio";
	private const string NameFlakeNoise = "flake_noise";

	private const string NameMaskRColor = "mask_r_color";
	private const string NameMaskRLocked = "mask_r_locked";
	private const string NameMaskGColor = "mask_g_color";
	private const string NameMaskGLocked = "mask_g_locked";
	private const string NameMaskBColor = "mask_b_color";
	private const string NameMaskBLocked = "mask_b_locked";
	private const string NameColorVariant = "color_variant";


	protected const string HeaderSimplePaintJob = "simple_paint_job_data";
	private const string NameAccList = "acc_list";



	protected override bool HasQuote(string name) => name switch {
		NameDisplayName or
		NameIcon or
		NamePartType or
		NameSuitableFor or
		NameConflictWith or
		NameDefaults or
		NameOverrides or
		NameRequire or
		NameBaseTextureOverride or
		NamePaintJobMask or
		NameFlakeNoise => true,
		_ => false
	};

	protected override bool IsArray(string name) => name switch {
		NameSuitableFor or
		NameConflictWith or
		NameDefaults or
		NameOverrides or
		NameRequire or
		NameColorVariant => true,
		_ => false
	};

	private void WriteAccPaintJobData(StreamWriter sw, string paintJobName, string vehicleID, Action data) {
		WriteDataHeader(sw, HeaderAccPaintJob, $"{paintJobName}.{vehicleID}.paint_job", data);
	}

	public void WritePaintJobData(PaintJobBinding binding) {
		var data = binding.PaintJobData;
		var vehicleID = data.CurrentVehicle!.VehicleID;
		var paintJobName = data.ModelName;
		var siiFile = Paths.AccPaintJobFile(vehicleID, paintJobName);
		using StreamWriter sw = new(siiFile);
		WriteFileStructure(sw, () => {
			WriteAccPaintJobData(sw, paintJobName, vehicleID, () => {
				bool customBase, custom1, custom2, custom3;

				WriteLine(sw, NameDisplayName, data.DisplayName);
				WriteLine(sw, NamePrice, data.Price);
				WriteLine(sw, NameUnlock, data.UnlockLevel);
				WriteLine(sw, NameIcon, data.IconName);
				WriteLine(sw, NamePartType, data.PartType, "unknown");
				WriteLine(sw, NameSyncOverNetwork, data.SyncOverNetwork, false);
				WriteLine(sw, NameSuitableFor, data.CurrentCabin!.CabinID);//当选择多个卡车时需要输出多个卡车
				//WriteLine(sw, NameConflictWith, binding.ConflictWith);

				WriteEmptyLine(sw);
				WriteLine(sw, NameBaseColor, data.BaseColor, Colors.White);
				customBase = data.BaseColorCustomizable;
				WriteLine(sw, NameBaseColorLocked, data.BaseColorLocked, true);
				WriteLine(sw, NamePaintJobMask, data.PaintJobTex);
				WriteLine(sw, NameBaseTextureOverride, data.BaseTexOverride);
				WriteLine(sw, NameAlternateUVSet, data.AlternateUVSet, false);

				WriteEmptyLine(sw);
				bool airbrush = data.Airbrush;
				if (airbrush) {
					WriteLine(sw, NameAirbrush, airbrush, false);
					WriteLine(sw, NameFlipFlake, data.FlipFlake, false);
					WriteLine(sw, NameAlternateFlipFlakeUVSet, data.AlternateFlipFlakeUVSet, false);
					WriteLine(sw, NameFlipColor, data.FlipColor, Colors.Red);
					custom1 = data.FlipColorCustomizable;
					WriteLine(sw, NameFlipColorLocked, data.FlipColorLocked, true);
					WriteLine(sw, NameFlakeColor, data.FlakeColor, Color.FromRgb(0, 255, 0));
					custom2 = data.FlakeColorCustomizable;
					custom3 = false;
					WriteLine(sw, NameFlakeColorLocked, data.FlakeColorLocked, true);
					WriteLine(sw, NameFlipStrength, data.FlipStrength, 0.27f);
					WriteLine(sw, NameFlakeShininess, data.FlakeShininess, 50f);
					WriteLine(sw, NameFlakeDensity, data.FlakeDensity, 1f);
					WriteLine(sw, NameFlakeClearcoatRolloff, data.FlakeClearcoatRolloff, 2.2f);
					WriteLine(sw, NameFlakeUVScale, data.FlakeUVScale, 32f);
					WriteLine(sw, NameFlakeVRatio, data.FlakeVRatio, 1f);
					WriteLine(sw, NameFlakeNoise, data.FlakeNoise, "/material/custom/flake_noise.tobj");
				} else {
					WriteLine(sw, NameMaskRColor, data.MaskRColor, Colors.Red);
					custom3 = data.MaskRCustomizable;
					WriteLine(sw, NameMaskRLocked, data.MaskRLocked, true);
					WriteLine(sw, NameMaskGColor, data.MaskGColor, Color.FromRgb(0, 255, 0));
					custom2 = data.MaskGCustomizable;
					WriteLine(sw, NameMaskGLocked, data.MaskGLocked, true);
					WriteLine(sw, NameMaskBColor, data.MaskBColor, Colors.Blue);
					custom1 = data.MaskBCustomizable;
					WriteLine(sw, NameMaskBLocked, data.MaskBLocked, true);
				}

				if (customBase && custom1 && custom2 && custom3) {
					WriteEmptyLine(sw);
					Color blank = Color.FromRgb(255,255,255);
					foreach (var variant in data.ColorVariantList) {
						Color color = customBase ? variant.ColorBase ?? blank : blank;
						WriteLine(sw, NameColorVariant, color);
						color = custom1 ? variant.Color1 ?? (airbrush ? Colors.Red : Colors.Blue) : blank;
						WriteLine(sw, NameColorVariant, color);
						color = custom2 ? variant.Color2 ?? Color.FromRgb(0, 255, 0) : blank;
						WriteLine(sw, NameColorVariant, color);
						color = custom3 ? variant.Color3 ?? Colors.Red : blank;
						WriteLine(sw, NameColorVariant, color);
					}
				}
			});
		});

		WriteOvrPaintJobData(data);
	}
	private void WriteSimplePaintJobData(StreamWriter sw, string ovrName, Action data) {
		WriteDataHeader(sw, HeaderSimplePaintJob, ovrName, data);
	}

	private void WriteOvrPaintJobData(AccPaintJobData data) {
		if (data.OverrideList.Count == 0)
			return;
		var vehicleID = data.CurrentVehicle!.VehicleID;
		var paintJobName = data.ModelName;
		var siiFile = Paths.SimplePaintJobFile(vehicleID, paintJobName);
		using StreamWriter sw = new(siiFile);
		WriteFileStructure(sw, () => {
			for (int i = 0; i < data.OverrideList.Count; i++) {
				if (i > 0)
					WriteEmptyLine(sw);
				PaintJobOverrideData? ovr = data.OverrideList[i];
				WriteSimplePaintJobData(sw, ovr.OvrName, () => {
					WriteLine(sw, NamePaintJobMask, ovr.AccTex);
					WriteLine(sw, NameFlakeUVScale, ovr.AccFlakeUVScale);
					WriteLine(sw, NameFlakeVRatio, ovr.AccFlakeVRatio);
					foreach(var acc in ovr.AccList) {
						WriteLine(sw, NameAccList, acc.AccID);
					}
				});
			}
		});
	}

	//注意simple的文件名字对应paintjob的模型名（而不是文件名）
}
