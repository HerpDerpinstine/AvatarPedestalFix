using System;
using System.Reflection;
using Harmony;
using UnityEngine;
using VRCModLoader;
using VRC.Core;
using VRCSDK2;

namespace AvatarPedestalFix
{
    public static class ModInfo
    {
        public const string Name = "AvatarPedestalFix";
        public const string Author = "Herp Derpinstine";
        public const string Company = "NanoNuke @ nanonuke.net";
        public const string Version = "1.0.2";
    }
    [VRCModInfo(ModInfo.Name, ModInfo.Version, ModInfo.Author)]

    public class AvatarPedestalFix : VRCMod
    {
        private static FieldInfo instance = null;
        private static HarmonyInstance harmonyInstance = null;
        private static bool initialized = false;
        private static ApiWorld currentRoom = null;

        void OnApplicationStart()
        {
            instance = typeof(VRC_AvatarPedestal).GetField("Instance", BindingFlags.NonPublic | BindingFlags.Instance);
            harmonyInstance = HarmonyInstance.Create("AvatarPedestalFix");
            harmonyInstance.Patch(typeof(VRC_EventDispatcherRFC).GetMethod("TriggerEvent", BindingFlags.Public | BindingFlags.Instance), new HarmonyMethod(typeof(AvatarPedestalFix).GetMethod("TriggerEvent", BindingFlags.NonPublic | BindingFlags.Static)));
        }

        void OnLevelWasLoaded(int level)
        {
            if (!initialized && level == ((Application.platform == RuntimePlatform.WindowsPlayer) ? 1 : 2))
                initialized = true;
        }

        void OnUpdate()
        {
            if (initialized)
            {
                if ((RoomManager.currentRoom != null) && !string.IsNullOrEmpty(RoomManager.currentRoom.id) && !string.IsNullOrEmpty(RoomManager.currentRoom.currentInstanceIdOnly))
                {
                    if (currentRoom == null)
                    {
                        currentRoom = RoomManager.currentRoom;
                        VRC_AvatarPedestal[] pedestalsInWorld = Resources.FindObjectsOfTypeAll<VRC_AvatarPedestal>();
                        foreach (VRC_AvatarPedestal p in pedestalsInWorld)
                            if (!string.IsNullOrEmpty(p.blueprintId))
                                foreach (VRC_Trigger trigger in p.GetComponents<VRC_Trigger>())
                                    trigger.ExecuteTrigger = new Action<VRC_Trigger.TriggerEvent>((VRC_Trigger.TriggerEvent evt) => Networking.RPC(VRC_EventHandler.VrcTargetType.Local, (GameObject)instance.GetValue(p), "SetAvatarUse", new object[0]));
                    }
                }
                else
                    currentRoom = null; 
            }
        }

        private static bool TriggerEvent(VRC_EventHandler __0, VRC_EventHandler.VrcEvent __1, VRC_EventHandler.VrcBroadcastType __2, int __3, float __4)
        {
            if ((__2 != VRC_EventHandler.VrcBroadcastType.Local) && !string.IsNullOrEmpty(__1.ParameterString) && __1.ParameterString.Equals("SetAvatarUse"))
                return false;
            return true;
        }
    }
}
