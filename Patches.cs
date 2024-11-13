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
	
	static class Lib {
		
	}
}
