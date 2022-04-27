using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankEngine.achievements
{
    public static class AchievementHandler
    {
        public static IAchievements API;
        public static bool completedEnter = false;

        public static void Init()
        {
            try
            {
                API = TankEngine.Instance.ModHelper.Interaction.GetModApi<IAchievements>("xen.AchievementTracker");
            }
            catch(Exception)
            {
                return;
            }

            API.RegisterAchievement("TANK_ENGINE_SUN", false, TankEngine.Instance);
            API.RegisterAchievement("TANK_ENGINE_FISH", false, TankEngine.Instance);
            API.RegisterAchievement("TANK_ENGINE_ENTER", false, TankEngine.Instance);

            API.RegisterTranslationsFromFiles(TankEngine.Instance, "translations");

            TankEngine.Log("Achievements enabled");

            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
            GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
        }

        public static void OnPlayerDeath(DeathType deathType)
        {
            if(deathType == DeathType.Energy && PlayerState.AtFlightConsole())
            {
                API.EarnAchievement("TANK_ENGINE_SUN");
            }

            if (deathType == DeathType.Digestion && PlayerState.AtFlightConsole())
            {
                API.EarnAchievement("TANK_ENGINE_FISH");
            }
        }

        public static void OnEnterFlightConsole(OWRigidbody _)
        {
            if(!completedEnter)
            {
                API.EarnAchievement("TANK_ENGINE_ENTER");
                completedEnter = true;
            }
        }
    }
}
