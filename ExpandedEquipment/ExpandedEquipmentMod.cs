using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Threading;
using Harmony;
using ReikaKalseki.ExpandedEquipment;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.ExpandedEquipment
{
  public class ExpandedEquipmentMod : FCoreMod
  {
    public const string MOD_KEY = "ReikaKalseki.ExpandedEquipment";
    public const string CUBE_KEY = "ReikaKalseki.ExpandedEquipment_Key";
    
    private static Config<EEConfig.ConfigEntries> config;
    
    public ExpandedEquipmentMod() : base("ExpandedEquipment") {
    	config = new Config<EEConfig.ConfigEntries>(this);
    }
    
    public static Config<EEConfig.ConfigEntries> getConfig() {
    	return config;
    }

	protected override void loadMod(ModRegistrationData registrationData) {        
        config.load();
        
        runHarmony();      
                
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.ItemMagnet"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.ItemMagnetV2"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.NightVision"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.SpringBoots"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.SandBlaster"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.HeatSuit"].ItemID);
    }
    
    public static DroppedItemData doPlayerItemCollection(ItemManager mgr, long x, long y, long z, Vector3 off, float magRange, float magStrength, float range, int maxStack, Player p) {
    	
    	//FUtil.log("Has magnet "+id+" : "+inv.GetSuitAndInventoryItemCount(id));
    	float pwr = config.getFloat(EEConfig.ConfigEntries.MAGNET_COST);
    	float pt = pwr*Time.deltaTime;
    	float pwr2 = config.getFloat(EEConfig.ConfigEntries.MAGNET_V2_COST);
    	float pt2 = pwr2*Time.deltaTime;
    	if (SurvivalPowerPanel.mrSuitPower >= pt2 && SuitUtil.isSuitItemPresent(p, "ReikaKalseki.ItemMagnetV2")) {
    		range *= 12;
    		magRange *= 12;
    		//ARTHERPetSurvival.instance.SetARTHERReadoutText("Magnet v2 active", 5, false, true);
    		SurvivalPowerPanel.mrSuitPower -= pt2;
		}
		else if (SurvivalPowerPanel.mrSuitPower >= pt && SuitUtil.isSuitItemPresent(p, "ReikaKalseki.ItemMagnet")) {
    		range *= 4;
    		magRange *= 4;
    		//ARTHERPetSurvival.instance.SetARTHERReadoutText("Magnet v1 active", 5, false, true);
    		SurvivalPowerPanel.mrSuitPower -= pt;
		}
    	//DroppedItemData droppedItem = mgr.UpdateCollection(x, y, z, off, magRange, magStrength, range, maxStack);
    	DroppedItemData droppedItem = replacedItemCollection(p, mgr, x, y, z, off, magRange, magStrength, range, maxStack);
    	return droppedItem;
    }
    
	public static DroppedItemData replacedItemCollection(Player p, ItemManager mgr, long x, long y, long z, Vector3 offset, float lrMagRange, float lrMagStrength, float lrCollectRange, int lnMaxStack)
	{
		lrMagRange *= lrMagRange;
		lrCollectRange *= lrCollectRange;
		long num2;
		long num3;
		long num4;
		int r = 1;
		if (SuitUtil.isSuitItemPresent(p, "ReikaKalseki.ItemMagnetV2"))
			r = 6;
		else if (SuitUtil.isSuitItemPresent(p, "ReikaKalseki.ItemMagnet"))
			r = 2;
		WorldHelper.GetSegmentCoords(x, y, z, out num2, out num3, out num4);
		for (int i = -r; i <= r; i++)
		{
			for (int j = -r; j <= r; j++)
			{
				for (int k = -r; k <= r; k++)
				{
					long x2 = num2 + (long)(k * 16);
					long y2 = num3 + (long)(i * 16);
					long z2 = num4 + (long)(j * 16);
					Segment segment = WorldScript.instance.GetSegment(x2, y2, z2);
					//FUtil.log("Scanning segment for item: "+segment+" @ "+i+","+j+","+k);
					if (segment != null && segment.mbInitialGenerationComplete && !segment.mbDestroyed)
					{
						if (segment.HasDroppedItems())
						{
							int count = segment.mDroppedItems.Count;
							for (int l = 0; l < count; l++)
							{
								DroppedItemData droppedItemData = segment.mDroppedItems[l];
								if (droppedItemData != null)
								{
									if (droppedItemData.mItem.mType != ItemType.ItemStack || (droppedItemData.mItem as ItemStack).mnAmount < lnMaxStack)
									{
										if (updateCollectionItem(mgr, droppedItemData, x, y, z, offset, lrMagRange, lrMagStrength, lrCollectRange))
										{
											if (droppedItemData.mWrapper != null)
											{
												if (droppedItemData.mWrapper.mbHasGameObject)
												{
													SurvivalPlayerScript.mItemCollectionPos = droppedItemData.mWrapper.mUnityPosition;
												}
												SpawnableObjectManagerScript.instance.ClearObject(droppedItemData.mWrapper);
												droppedItemData.mWrapper = null;
											}
											mgr.RemoveItemFromSegment(segment, l);
											segment.RequestDelayedSave();
											return droppedItemData;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		return null;
	}
	
	private static bool updateCollectionItem(ItemManager mgr, DroppedItemData itemData, long x, long y, long z, Vector3 offset, float lrMagRange, float lrMagStrength, float lrCollectRange)
	{
		if (itemData.mbHandledExternally)
		{
			return false;
		}
		if (!itemData.mbOnGround)
		{
			return false;
		}
		if (itemData.mrAwakeTime < 1.5f)
		{
			return false;
		}
		Vector3 a = new Vector3((float)(x - itemData.mWorldX), (float)(y - itemData.mWorldY), (float)(z - itemData.mWorldZ)) + offset - itemData.mBlockInternalOffset;
		float sqrMagnitude = a.sqrMagnitude;
		if (sqrMagnitude < lrMagRange)
		{
			if (sqrMagnitude < lrCollectRange)
			{
				return true;
			}
			float num = sqrMagnitude / lrMagRange;
			num = 1f - num;
			num *= lrMagStrength;
			a.Normalize();
			itemData.mVelocity += num * a;
		}
		return false;
	}
    
    private static float nightVisionBrightness = 0;
    
    public static void onSetSurvivalDepth(int depth) {
    	SurvivalFogManager.GlobalDepth = depth;
    	depth = -depth; // is otherwise < 0 in caves
    	bool flag = false;
    	if (depth > 24) {
	    	float pwr = config.getFloat(EEConfig.ConfigEntries.NV_COST);
    		float pt = pwr*Time.deltaTime;
			if (SurvivalPowerPanel.mrSuitPower >= pt && SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.NightVision")) {
	    		SurvivalPowerPanel.mrSuitPower -= pt;
	    		flag = true;	    		
			}
    	}
    	if (flag) {
    		float f = config.getFloat(EEConfig.ConfigEntries.NV_STRENGTH)*0.33F;
    		nightVisionBrightness = Mathf.Min(f, RenderSettings.ambientIntensity+0.25F*Time.deltaTime*f);
    	}
    	else {
    		nightVisionBrightness = Mathf.Max(0, RenderSettings.ambientIntensity-0.125F*Time.deltaTime);
    	}
    	if (depth > 24 && SurvivalPowerPanel.mrSuitPower > 0) {
	    	RenderSettings.ambientIntensity = nightVisionBrightness;
			RenderSettings.ambientLight = new Color(173/255F, 234/255F, 1, 1)*nightVisionBrightness;
			DynamicGI.UpdateEnvironment();
    	}
    	else {
    		RenderSettings.ambientIntensity = 0;
    		RenderSettings.ambientLight = Color.black;
    	}
    }
    
    public static float getFallDamage(float amt) {
    	if (amt > 0) {
    		//player has 100 health
    		float pwr = Mathf.Lerp(amt/100F, config.getFloat(EEConfig.ConfigEntries.FALL_BOOT_COST_MIN), config.getFloat(EEConfig.ConfigEntries.FALL_BOOT_COST_MAX));
    		if (SurvivalPowerPanel.mrSuitPower >= pwr && SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.SpringBoots")) {
	    		float orig = amt;
	    		amt = (amt*0.8F)-10;
    			bool kill = orig >= SurvivalPowerPanel.CurrentHealth;
	    		bool lethalSave = orig >= 100F && SurvivalPowerPanel.CurrentHealth >= 100;	
	    		bool stillKill = amt >= SurvivalPowerPanel.CurrentHealth;
	    		SurvivalPowerPanel.mrSuitPower -= pwr;
	    		if (stillKill) {
	    			ARTHERPetSurvival.instance.SetARTHERReadoutText("Spring Boots reduced the fall injury but it was still enough to kill you", 15, false, true);
	    		}
	    		else if (lethalSave) {
	    			amt = Mathf.Min(amt, 90);
	    			ARTHERPetSurvival.instance.SetARTHERReadoutText("Spring Boots saved you from a guaranteed fatal fall", 15, false, true);
	    		}
	    		else if (kill) {
	    			ARTHERPetSurvival.instance.SetARTHERReadoutText("Spring Boots saved you from a fall that would have killed you", 15, false, true);
	    		}
	    		else if (amt <= 0) {
	    			ARTHERPetSurvival.instance.SetARTHERReadoutText("Spring Boots prevented your injury from the fall, saving "+orig.ToString("0.0")+"% of your health", 15, false, true);
	    		}
	    		else {
	    			ARTHERPetSurvival.instance.SetARTHERReadoutText("Spring Boots reduced your injury from the fall, saving "+(orig-amt).ToString("0.0")+"% of your health", 15, false, true);
	    		}
    		}
    	}
    	return amt;
    }
    
    public static void onDoNonOreDig(PlayerBuilder pb, Segment s, long x, long y, long z, SurvivalDigScript scr, ushort id) {
    	pb.Dig(s, x, y, z);
    	bool flag = false;
    	if (id == eCubeTypes.Sand || id == eCubeTypes.SandFluid) {
    		float pwr = config.getFloat(EEConfig.ConfigEntries.SAND_BLAST_COST);
    		if (SurvivalPowerPanel.mrSuitPower >= pwr && SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.SandBlaster")) {
	    		SurvivalPowerPanel.mrSuitPower -= pwr;
    			//ARTHERPetSurvival.instance.SetARTHERReadoutText("Dug sand @ "+x+", "+y+", "+z, 15, false, true);
    			int r = config.getInt(EEConfig.ConfigEntries.SAND_BLAST_RADIUS);
				for (int i = -r; i <= r; i++) {
					for (int j = -r; j <= r; j++) {
						for (int k = -r; k <= r; k++) {
    						long dx = x+i;
    						long dy = y+j;
    						long dz = z+k;
							Segment s2 = WorldScript.instance.GetSegment(dx, dy, dz);
							if (s2.isSegmentValid() && !s2.mbIsEmpty) {
								ushort at = s2.GetCube(dx, dy, dz);
								if (at == eCubeTypes.Sand || at == eCubeTypes.SandFluid) {
									flag = true;
									WorldScript.instance.BuildFromEntity(s2, dx, dy, dz, eCubeTypes.Air);
									DroppedItemData stack = ItemManager.DropNewCubeStack(eCubeTypes.SandFluid, 0, 1, dx, dy, dz, Vector3.zero);
								}
							}
    					}
    				}
    			}
    		}
    	}
    	if (flag) {
    		Vector3 position = WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(x, y, z) + WorldHelper.DefaultBlockOffset;
			SurvivalParticleManager.instance.SandDropParticles.transform.position = position;
			SurvivalParticleManager.instance.SandDropParticles.Emit(60);
			AudioHUDManager.instance.BFLFire(position);
    	}
    }
    
    public static float getSuitInsulation(float orig, SurvivalHazardPanel surv, float Tamb) {
    	if (Tamb >= 100 && SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.HeatSuit")) {
    		return orig*4;
    	}
    	return orig;
    }
    
    public static bool isInLavaForHeatCalc(bool orig) {
    	return orig && !SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.HeatSuit");
    }
    
    public static bool checkHurtStayTrigger(Collider c, string tag, HurtPlayerOnStay hurt) {
    	return c.CompareTag(tag) && !isHeatProtected(hurt);
    }
    
    private static bool isHeatProtected(HurtPlayerOnStay hurt) {
    	FUtil.log("Attempt hurt from "+hurt+" in "+hurt.gameObject+" of "+hurt.getFullHierarchyPath()+" with C=["+string.Join(", ", hurt.gameObject.GetComponents<Component>().Select(c => c.GetType().Name).ToArray())+"]");
    	return SuitUtil.isSuitItemPresent(WorldScript.mLocalPlayer, "ReikaKalseki.HeatSuit") && isHeatTypeHurt(hurt.gameObject.getRoot());
    }
    
    public static bool isHeatTypeHurt(GameObject go) {
    	string name = go.name;
    	return name.Contains("JetTurbine") || name.Contains("PyrothermicGenerator") || name.Contains("Conduit") || name.Contains("Laser Upgrade") || name.Contains("LaserTransferHolder") || name.Contains("Dazzler") || name.Contains("CryoBurner");
    }
    /*
    public static void debugLPT(LaserPowerTransmitter lpt, GameObject beam) {
    	FUtil.log("LPT @ "+lpt.machineToString()+" beam object:");
    	FUtil.dumpObjectData(beam);
    }*/

  }
}
