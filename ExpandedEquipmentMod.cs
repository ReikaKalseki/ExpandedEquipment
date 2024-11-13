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

    public override ModRegistrationData Register()
    {
        ModRegistrationData registrationData = new ModRegistrationData();
        
        config.load();
        
        runHarmony();      
                
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.ItemMagnet"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.NightVision"].ItemID);
        UIManager.instance.mSuitPanel.ValidItems.Add(ItemEntry.mEntriesByKey["ReikaKalseki.SpringBoots"].ItemID);
        
        return registrationData;
    }
    
    public static DroppedItemData doPlayerItemCollection(ItemManager mgr, long x, long y, long z, Vector3 off, float magRange, float magStrength, float range, int maxStack, Player p) {
    	PlayerInventory inv = p.mInventory;
    	int id = ItemEntry.GetIDFromKey("ReikaKalseki.ItemMagnet", true);
    	//FUtil.log("Has magnet "+id+" : "+inv.GetSuitAndInventoryItemCount(id));
    	float pwr = config.getFloat(EEConfig.ConfigEntries.MAGNET_COST);
    	float pt = pwr*Time.deltaTime;
		if (SurvivalPowerPanel.mrSuitPower >= pt && id > 0 && inv.GetSuitAndInventoryItemCount(id) > 0) { //TODO cache this for performance
    		range *= 6;
    		magRange *= 6;
    		SurvivalPowerPanel.mrSuitPower -= pt;
		}
    	DroppedItemData droppedItem = mgr.UpdateCollection(x, y, z, off, magRange, magStrength, range, maxStack);
    	return droppedItem;
    }
    
    private static float nightVisionBrightness = 0;
    
    public static void onSetSurvivalDepth(int depth) {
    	SurvivalFogManager.GlobalDepth = depth;
    	depth = -depth; // is otherwise < 0 in caves
    	bool flag = false;
    	if (depth > 24) {
	    	int id = ItemEntry.GetIDFromKey("ReikaKalseki.NightVision", true);
	    	float pwr = config.getFloat(EEConfig.ConfigEntries.NV_COST);
    		float pt = pwr*Time.deltaTime;
			if (SurvivalPowerPanel.mrSuitPower >= pt && id > 0 && WorldScript.mLocalPlayer.mInventory.GetSuitAndInventoryItemCount(id) > 0) { //TODO cache this for performance
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
    		int id = ItemEntry.GetIDFromKey("ReikaKalseki.SpringBoots", true);
    		//player has 100 health
    		float pwr = Mathf.Lerp(amt/100F, config.getFloat(EEConfig.ConfigEntries.FALL_BOOT_COST_MIN), config.getFloat(EEConfig.ConfigEntries.FALL_BOOT_COST_MAX));
    		if (SurvivalPowerPanel.mrSuitPower >= pwr && id > 0 && WorldScript.mLocalPlayer.mInventory.GetSuitAndInventoryItemCount(id) > 0) {
	    		float orig = amt;
	    		amt = (amt*0.8F)-10;
    			bool kill = orig >= SurvivalPowerPanel.CurrentHealth;
	    		bool lethalSave = orig >= 100F && SurvivalPowerPanel.CurrentHealth >= 100;	
	    		bool stillKill = amt >= SurvivalPowerPanel.CurrentHealth;
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

  }
}
