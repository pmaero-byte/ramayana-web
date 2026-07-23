// RamayanaHudBootstrap — ensures the feedback HUDs exist when entering play mode.
using UnityEngine;
using Jambudweep.Ramayana.Feedback;

namespace Ramayana.Runtime
{
    public class RamayanaHudBootstrap : MonoBehaviour
    {
        private void Start()
        {
            KandaPortraitHUD.EnsureCreated();
            KandaPortraitHUD.Instance.Show(1, 0);
            VerseStreakHUD.EnsureCreated();
            DayDotStrip.EnsureCreated();
            DayDotStrip.Instance.SetDay(1, 0);
            SanskritTitle.EnsureCreated();
            SanskritTitle.Instance.Show(1);
        }
    }
}
