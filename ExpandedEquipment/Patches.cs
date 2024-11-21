/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 04/11/2019
 * Time: 11:28 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.ExpandedEquipment {

	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class PlayerCollectionBooster {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Name == "UpdateCollection") {
						ci.opcode = OpCodes.Call;
						ci.operand = InstructionHandlers.convertMethodOperand(typeof(ExpandedEquipmentMod), "doPlayerItemCollection", false, new Type[]{typeof(ItemManager), typeof(long), typeof(long), typeof(long), typeof(Vector3), typeof(float), typeof(float), typeof(float), typeof(int), typeof(Player)});
						CodeInstruction ldself = new CodeInstruction(OpCodes.Ldarg_0);
						codes.Insert(i, ldself);
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
/*
	[HarmonyPatch(typeof(ItemManager))]
	[HarmonyPatch("UpdateCollection")]
	public static class PlayerCollectiorItemScanRange {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				OpCode[] pattern1 = new OpCode[]{OpCodes.Ldc_I4_M1, OpCodes.Stloc_S, OpCodes.Br};
				OpCode[] pattern2 = new OpCode[]{OpCodes.Ldloc_S, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Stloc_S, OpCodes.Ldloc_S, OpCodes.Ldc_I4_1, OpCodes.Ble};
				for (int i = 0; i < codes.Count; i++) {
					if (InstructionHandlers.matchPattern(codes, i, pattern1)) {
						FileLog.Log("Replacing i4_M1 with i4_-3 in for loop @ "+i);
						codes[i] = new CodeInstruction(OpCodes.Ldc_I4, -3);
					}
					else if (InstructionHandlers.matchPattern(codes, i, pattern2)) {
						FileLog.Log("Replacing i4_1 with i4_3 in for loop @ "+i);
						codes[i+5].opcode = OpCodes.Ldc_I4_3;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
*/	
	[HarmonyPatch(typeof(SurvivalFogManager))]
	[HarmonyPatch("Update")]
	public static class SurvivalFogHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Stsfld);
				codes[idx] = InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "onSetSurvivalDepth", false, new Type[]{typeof(int)});
				
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(LocalPlayerScript))]
	[HarmonyPatch("UpdateMovement")]
	public static class FallDamageHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, typeof(SurvivalPlayerScript), "DoDamageAnim", true, new Type[]{typeof(float)});
				//int idx2 = idx+1;
				//int idx1 = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Ldloc_S);
				
				//do in reverse order to prevent idx1 from changing idx2
				//codes.Insert(idx2+1, InstructionHandlers.createMethodCall("ReikaKalseki.ExpandedEquipment.ExpandedEquipmentMod", "getFallDamage", false, new Type[]{typeof(int)}));
				//codes.Insert(idx1+1, InstructionHandlers.createMethodCall("ReikaKalseki.ExpandedEquipment.ExpandedEquipmentMod", "getFallDamage", false, new Type[]{typeof(int)}));
				
				idx = InstructionHandlers.getLastOpcodeBefore(codes, idx, OpCodes.Stloc_S);
				codes.Insert(idx, InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "getFallDamage", false, new Type[]{typeof(int)}));
				
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(SurvivalDigScript))]
	[HarmonyPatch("DoNonOreDig")]
	public static class SandDigHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, typeof(PlayerBuilder), "Dig", true, new Type[]{typeof(Segment), typeof(long), typeof(long), typeof(long)});
				codes[idx] = InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "onDoNonOreDig", false, new Type[]{typeof(PlayerBuilder), typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(SurvivalDigScript), typeof(ushort)});
				codes.InsertRange(idx, new List<CodeInstruction>{
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(SurvivalDigScript), "mDigTarget")),
				});
				
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(SurvivalHazardPanel))]
	[HarmonyPatch("FixedUpdate")]
	public static class ThermalBalanceHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, typeof(SurvivalHazardPanel), "mrSuitInsulation");
				codes.InsertRange(idx+1, new List<CodeInstruction>{
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand(typeof(SurvivalHazardPanel), "mrExternalTemperature")),
					InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "getSuitInsulation", false, new Type[]{typeof(float), typeof(SurvivalHazardPanel), typeof(float)}),
				});
				FileLog.Log("Done patch A "+MethodBase.GetCurrentMethod().DeclaringType);
				Lib.wrapMagmaCheck(codes, "mbHeadInMagma");
				Lib.wrapMagmaCheck(codes, "mbFeetInMagma");
				
				FileLog.Log("Done patch B "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(HurtPlayerOnStay))]
	[HarmonyPatch("OnTriggerStay")]
	public static class HurtTriggerHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, typeof(UnityEngine.Component), "CompareTag", true, new Type[]{typeof(string)});
				//idx = InstructionHandlers.getFirstOpcode(codes, idx, OpCodes.Ldarg_0);
				codes[idx] = InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "checkHurtStayTrigger", false, new Type[]{typeof(Collider), typeof(string), typeof(HurtPlayerOnStay)});
				codes.InsertRange(idx, new List<CodeInstruction>{
					new CodeInstruction(OpCodes.Ldarg_0),
				});
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	static class Lib {
		
		public static void wrapMagmaCheck(List<CodeInstruction> codes, string field) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, typeof(LocalPlayerScript), field);
			codes.Insert(idx+1, InstructionHandlers.createMethodCall(typeof(ExpandedEquipmentMod), "isInLavaForHeatCalc", false, new Type[]{typeof(bool)}));
		}
		
	}
}
