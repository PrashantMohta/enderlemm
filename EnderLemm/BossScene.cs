using GlobalEnums;
using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Satchel.SceneUtils;
using Satchel.Futils;

namespace EnderLemm
{
    internal class Boss1
    {
        public static GameObject BossParent,ShowerR, ShowerL,cwl,cwr, DumbellPrefab,Mushroom;
        private static AssetBundle sceneBundle;
        private static bool isRunning = false;

        public static int MaxHealth = 300, Stage2 = 150;
        public static AssetBundle getAssetBundle()
        {
            if (sceneBundle == null)
            {
                sceneBundle = AssemblyUtils.GetAssetBundleFromResources("lemmbossscene");
            }
            return sceneBundle;
        }

        public static IEnumerator SceneLoadCoro()
        {
            yield return new WaitForSeconds(0.1f);
            Modding.Logger.Log("who it");
            yield return new WaitForSeconds(0.1f);

            var customAttacks = new List<GameObject>();
            var gameObjects = GameObjectUtils.GetAllGameObjectsInScene();

            foreach (var go in gameObjects)
            {
                Modding.Logger.Log($"{go.name} it");

                if (go == null) { continue; }
                if (go.name == "BossParent")
                {
                    BossParent = go;
                }
                if (go.name == "ShowerHeadR2L")
                {
                    // right shower head
                    ShowerR = go;
                    var wiggle = ShowerR.GetAddComponent<wiggle>();
                    wiggle.direction = new Vector3(0f,0f,5f);
                    cwr = ShowerR.FindGameObjectInChildren("Coldwater");
                }
                if (go.name == "ShowerHeadL2R")
                {
                    // left shower head
                    ShowerL = go;
                    var wiggle = ShowerL.GetAddComponent<wiggle>();
                    wiggle.direction = new Vector3(0f, 0f, -5f);
                    cwl = ShowerL.FindGameObjectInChildren("Coldwater");

                }
                if (go.name == "dumbbellPrefab")
                {
                    // dumbellprefab
                    DumbellPrefab = go;
                    var wiggle = DumbellPrefab.GetAddComponent<rotate>();

                }
                DamageHeroMarker att = go.GetComponent<DamageHeroMarker>();
                if (att == null) { continue; }
                // process an enemy only once
                if (!att.isProcessed()) { 
                    att.setProcessed();

                    customAttacks.Add(att.gameObject);
                    var dh = att.gameObject.AddComponent<DamageHero>();
                    att.gameObject.activateAlertRanges();

                    dh.damageDealt = att.damageDealt;
                    dh.hazardType = att.hazardType;
                    dh.shadowDashHazard = att.shadowDashHazard;
                    dh.resetOnEnable = att.resetOnEnable;
                }

            }
            yield return new WaitForSeconds(2f);
            yield return SpawnMushroom();

        }

        private static IEnumerator SpawnMushroom()
        {

            Mushroom = GameObject.Instantiate(EnderLemm.Preloads.mushroom);
            var hm = Mushroom.GetComponent<HealthManager>();
            hm.isDead = false;
            hm.hp = MaxHealth;
            hm.OnDeath += Boss1_OnDeath;
            Mushroom.SetScale(1.3f, 1.3f);
            var mat = Mushroom.GetComponent<tk2dSpriteAnimator>().GetClipByName("Idle").frames[0].spriteCollection.spriteDefinitions[0].material;
            mat.mainTexture = AssemblyUtils.GetTextureFromResources("atlas0.png");
            Mushroom.SetActive(false);
            var fsm = Mushroom.LocateMyFSM("Mush Roller");
            fsm.Intercept(new TransitionInterceptor
            {
                eventName = "FINISHED",
                fromState = "Turn",
                toStateCustom = "Idle",
                toStateDefault = "Idle",
                onIntercept = (a, v) => {
                    Modding.Logger.Log("Turn dumb");
                    CoroutineHelper.GetRunner().StartCoroutine(SpawnDumbells(1));
                },
                shouldIntercept = () => true
            });
            fsm.Intercept(new TransitionInterceptor
            {
                eventName = "FINISHED",
                fromState = "Roll Antic",
                toStateCustom = "Roll",
                toStateDefault = "Roll",
                onIntercept = (a, v) => {
                    Modding.Logger.Log("Attack Choice ROLL");
                    if (!isRunning)
                    {
                        CoroutineHelper.GetRunner().StartCoroutine(ShowShowerHead());
                    }
                },
                shouldIntercept = () => true
            });
            fsm.Intercept(new TransitionInterceptor
            {
                eventName = "LAND",
                fromState = "Jump Antic",
                toStateCustom = "Jump",
                toStateDefault = "Jump",
                onIntercept = (a, v) => {
                    Modding.Logger.Log("Attack Choice JUMP");
                    CoroutineHelper.GetRunner().StartCoroutine(SpawnDumbells());
                },
                shouldIntercept = () => true
            });
            var Wr = Mushroom.FindGameObjectInChildren("Wake Region");
            Wr.GetComponent<BoxCollider2D>().size = new Vector2(160.98f, 6.4009f);
            var Fr = Mushroom.FindGameObjectInChildren("Front Region");
            Fr.GetComponent<BoxCollider2D>().size = new Vector2(10.9317f * 2f, 6.4009f * 2f);
            var Br = Mushroom.FindGameObjectInChildren("Behind Region");
            Br.GetComponent<BoxCollider2D>().size = new Vector2(17.7826f, 11.8816f * 2f);
            var Rr = Mushroom.FindGameObjectInChildren("Roof Check");
            Rr.GetComponent<BoxCollider2D>().size = new Vector2(8.9484f * 1f, 3.3226f * 1f);
            Mushroom.SetActive(true);
            yield return new WaitForSeconds(1.8f);
            hm.isDead = false;
        }
        public static bool fakeout = true;

        private static void Boss1_OnDeath()
        {
            Modding.Logger.Log("Boss Dead");
            if (fakeout) {
                CoroutineHelper.GetRunner().StartCoroutine(SpawnFakeouts());
                fakeout = false;
            }
        }

        public static IEnumerator SpawnFakeouts()
        {
            yield return new WaitForSeconds(0.1f);
            yield return SpawnMushroom();
            Mushroom.GetComponent<HealthManager>().hp = 50;
            Mushroom.SetScale(2f, 2f);
            Mushroom.SetActive(true);
        }

        public static IEnumerator SpawnDumbells(int _limit = 0)
        {
            var i = 0;
            var limit = _limit > 0 ? _limit : (Mushroom.GetComponent<HealthManager>().hp < Stage2 ? 5 : 2); 
            while (true) { 
                var d = GameObject.Instantiate(DumbellPrefab);
                d.transform.position = Mushroom.transform.position;
                d.SetActive(true);
                yield return new WaitForSeconds(0.05f);
                i++;
                if(i > limit)
                {
                    break;
                }
            }


        }
        public static IEnumerator ShowShowerHead()
        {
            var tele = Mushroom.GetComponent<HealthManager>().hp < Stage2 ? 0.3f : 0.5f;

            isRunning = true;
            if (Mushroom.transform.localScale.x < 0) {
                BossParent.transform.position = new Vector3(16f, 5f, 0f);
                ShowerL.SetActive(true);
                cwl.SetActive(false);
                yield return new WaitForSeconds(tele); ;
                cwl.SetActive(true);
                yield return new WaitForSeconds(0.2f + tele);
                ShowerL.SetActive(false);
            } else {
                BossParent.transform.position = new Vector3(16f, 5f, 0f);
                ShowerR.SetActive(true);
                cwr.SetActive(false);
                yield return new WaitForSeconds(tele);
                cwr.SetActive(true);
                yield return new WaitForSeconds(0.2f + tele);
                ShowerR.SetActive(false);
            }
            isRunning = false;
            BossParent.transform.position =  new Vector3(16f, 5f, 0f);

        }

        public static void OnSceneLoad()
        {
            isRunning = false;
            fakeout = true;
            CoroutineHelper.GetRunner().StartCoroutine(SceneLoadCoro());
            
        }

        private static void HealthManager_OnEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
        {
            orig(self);
            if(Mushroom != null && self.gameObject.name == Mushroom.name)
            {
                self.SetIsDead(false);
                self.gameObject.SetActive(true);
            }
        }

        public static void CreateScene()
        {
            var sceneName = "lemmBossScene";
            var customScene = EnderLemm.satchel.GetCustomScene(
                sceneName,
                EnderLemm.Preloads.TileMap,
                EnderLemm.Preloads._SceneManager
                );
            var settings = new CustomSceneManagerSettings(SceneUtils.getSceneManagerFromPrefab(customScene.SceneManager))
            {
                mapZone = MapZone.OVERGROWN_MOUND,
                overrideParticlesWith = MapZone.NONE//,
                //backgroundMusicGet = () => WavUtils.ToAudioClip(AssemblyUtils.GetBytesFromResources("mystic.wav"), 0)
            };
            customScene.Config(32, 32, settings);
            // right entrance from crossroads 
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = $"{sceneName} entry left",
                    pos = new Vector2(12.5f, 12.5f),
                    size = new Vector2(1, 4),
                    fromScene = "Town",
                    toScene = sceneName,
                    entryGate = $"{sceneName} gate right",
                    respawnPoint = new Vector2(3, 0),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.GrimmDream
                }
            );
            customScene.AddGateway(
                new GatewayParams
                {
                    gateName = $"{sceneName} gate right",
                    pos = new Vector2(16f, 16f),
                    size = new Vector2(1, 4),
                    fromScene = sceneName,
                    toScene = "Town",
                    entryGate = $"{sceneName} entry left",
                    respawnPoint = new Vector2(0, -2f),
                    onlyOut = false,
                    vis = GameManager.SceneLoadVisualizations.Default
                }
            );
            On.HealthManager.OnEnable += HealthManager_OnEnable;
            customScene.OnLoaded += (_, e) => { OnSceneLoad(); };
        }
    }
}
