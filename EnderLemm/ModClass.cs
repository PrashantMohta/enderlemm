using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

using Satchel;
using Satchel.BetterPreloads;
namespace EnderLemm
{
    //                preloads["Fungus1_03"]["TileMap"],
    //  preloads["Fungus1_03"]["_SceneManager"]

#pragma warning disable CS0649 //because we'll set the values later using reflection
    public class Preloads
    {
        [Preload("Fungus2_07", "Mushroom Roller")]
        public GameObject mushroom { get; set; }

        [Preload("Fungus1_03", "TileMap")]
        public GameObject TileMap { get; set; }

        [Preload("Fungus1_03", "_SceneManager")]
        public GameObject _SceneManager { get; set; }
    }
    public class EnderLemm : BetterPreloadsMod<Preloads>
    {
        internal static EnderLemm Instance;

        internal static Satchel.Core satchel = new Core();

        public override void Initialize()
        {
            if(EnderLemm.Preloads.mushroom != null)
            {
                Log("Preloaded!");

                Boss1.getAssetBundle();
                Boss1.CreateScene();
            }
        }

        public EnderLemm()
        {
            Instance = this;
        }
    }
}