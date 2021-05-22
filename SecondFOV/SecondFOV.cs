using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Receiver2;

using UnityEngine;

namespace QuickZoom
{
    [BepInProcess("Receiver2.exe")]
    [BepInPlugin("SmilingLizard.plugins.SecondFOV", "SecondFOV", "1.0.0")]
    public class SecondFOV : BaseUnityPlugin
    {
        ConfigEntry<int> secFov;
        ConfigEntry<KeyboardShortcut> key;

        float transition = 0f;
        const float transitionSpeed = 5f;

        public void Awake()
        {
            const string titleTxt = "Secondary FOV";

            const string valueName = "FOV Value";
            const string keyName = "Hotkey";

            const string valueDesc = "The FOV that is applied while `" + keyName + "` is held down.";
            const string keyDesc = "The key combination to apply `" + valueName + "`.";

            ConfigDescription fovDesc = new ConfigDescription(
                valueDesc,
                new AcceptableValueRange<int>(1, 179)
            );

            this.secFov = this.Config.Bind(titleTxt, valueName, 60, fovDesc);
            this.key = this.Config.Bind(titleTxt, keyName, KeyboardShortcut.Empty, keyDesc);
        }
        public void Update()
        {
            if (LocalAimHandler.TryGetInstance(out LocalAimHandler lah))
            {
                ref float current =
                    ref AccessTools.FieldRefAccess<LocalAimHandler, float>(lah, "main_camera_fov");

                bool targetSec = Input.GetKey(this.key.Value.MainKey);
                if (targetSec)
                {
                    foreach (KeyCode k in this.key.Value.Modifiers)
                    {
                        if (!Input.GetKey(k))
                        {
                            targetSec = false;
                            break;
                        }
                    }
                }

                float target = targetSec
                    ? this.secFov.Value
                    : ConfigFiles.global.fov;

                float diff = current - target;

                if ((diff < 0 ? -diff : diff) < 1)
                {
                    current = target;
                    return;
                }
                else if (targetSec)
                {
                    this.transition += transitionSpeed * Time.deltaTime;
                }
                else
                {
                    this.transition -= transitionSpeed * Time.deltaTime;
                }
                current = Mathf.Lerp(ConfigFiles.global.fov, this.secFov.Value, this.transition);
            }
        }
    }
}
