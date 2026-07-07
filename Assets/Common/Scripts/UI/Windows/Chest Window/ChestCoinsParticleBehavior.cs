using UnityEngine;

namespace OctoberStudio.UI.Chest
{
    public class ChestCoinsParticleBehavior : MonoBehaviour
    {
        [SerializeField] ParticleSystem particle;

        public void Show()
        {
            transform.position = PlayerBehavior.Player.transform.position + Vector3.right * 100;

            gameObject.SetActive(true);
        }

        public void PlayParticle()
        {
            particle.Play();
        }

        public void StopParticle()
        {
            particle.Stop();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}