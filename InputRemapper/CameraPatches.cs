using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Rewired;
using UnityEngine;
using MelonLoader;

namespace InputRemapper
{
    public class CameraPatches
    {
        public static void Initialize()
        {
            // Patch PlayerCamera Update Method
            MethodInfo method = typeof(PlayerCamera).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            HarmonyMethod harmonyMethodPre = new HarmonyMethod(typeof(CameraPatches).GetMethod("PreUpdate"));
            HarmonyMethod harmonyMethodPost = new HarmonyMethod(typeof(CameraPatches).GetMethod("PostUpdate"));
            Plugin.Harmony.Patch(method, harmonyMethodPre, harmonyMethodPost);
        }

        // Before The Camera Updates, Store The TrailerRot Variable
        public static void PreUpdate(PlayerCamera __instance)
        {
            Type t = typeof(PlayerCamera);
            FieldInfo field = t.GetField("TrailerRot", BindingFlags.Instance | BindingFlags.NonPublic);

            Melon<Plugin>.Instance.trailerRot = (Vector2)field.GetValue(__instance);
        }

        // Run New Camera Logic After The Original Logic Runs
        public static void PostUpdate(PlayerCamera __instance)
        {
            // Get The Private Player Variable
            Type t = typeof(RInput);
            RInput rin = GameObject.FindObjectOfType<RInput>();
            FieldInfo field = t.GetField("P", BindingFlags.NonPublic | BindingFlags.Instance);
            Player p = (Player)field.GetValue(rin);

            // Get The Camera State
            t = typeof(PlayerCamera);
            field = t.GetField("CameraState", BindingFlags.NonPublic | BindingFlags.Instance);

            PlayerCamera.State cState = (PlayerCamera.State)field.GetValue(__instance);

            // Only Run Custom Camera Logic If The Camera Is In The Normal State
            if (cState != PlayerCamera.State.Normal) return;

            // Add Custom Sensitivity To Camera Rotation Logic
            float y = Melon<Plugin>.Instance.trailerRot.y + Time.deltaTime * (-p.GetAxis("Right Stick X") * (float)((Singleton<Settings>.Instance.settings.InvertCamX == 1) ? 1 : -1)) * Melon<Plugin>.Instance.sensitivity.y;
            float x = Melon<Plugin>.Instance.trailerRot.x + Time.deltaTime * (p.GetAxis("Right Stick Y") * (float)((Singleton<Settings>.Instance.settings.InvertCamX == 1) ? 1 : -1)) * Melon<Plugin>.Instance.sensitivity.x;

            // Rest Is Vanilla, But Using The New Rotation Values

            if (x < -360f) x += 360f;
            if (x > 360f) x -= 360f;

            x = Mathf.Clamp(x, -40f, 40f);

            Quaternion rot = Quaternion.Euler(x, y, 0);

            field = t.GetField("TrailerDist", BindingFlags.Instance | BindingFlags.NonPublic);
            float trailDis = Mathf.Clamp((float)field.GetValue(__instance), 0.75f, 20f);
            field = t.GetField("PlayerBase", BindingFlags.Instance | BindingFlags.NonPublic);

            __instance.transform.rotation = rot;
            __instance.transform.position = ((PlayerBase)field.GetValue(__instance)).transform.position + Vector3.up * 0.3f - __instance.transform.forward * trailDis * 2;

            field = t.GetField("TrailerRot", BindingFlags.Instance | BindingFlags.NonPublic);

            field.SetValue(__instance, new Vector2(x, y));


            // Vanilla Camera Collision Won't Work In Normal State
            // Moved To Update Postfix To Restore Functionality
            field = t.GetField("Target", BindingFlags.Instance | BindingFlags.NonPublic);
            Transform target = (Transform)field.GetValue(__instance);

            Vector3 normalized = (__instance.transform.position - target.position).normalized;

            MethodInfo method = t.GetMethod("CameraCollision", BindingFlags.Instance | BindingFlags.NonPublic);

            method.Invoke(__instance, new object[] { target.position, normalized, Vector3.Distance(target.position, __instance.transform.position) });
        }
    }
}
