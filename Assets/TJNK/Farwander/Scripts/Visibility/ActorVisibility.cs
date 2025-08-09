using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Actors;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Systems.Visibility
{
    [DisallowMultipleComponent]
    public class ActorVisibility : MonoBehaviour
    {
        private VisibilityMap _vis;
        private Actor _actor;
        private SpriteRenderer[] _renders;

        void Awake()
        {
            _actor = GetComponent<Actor>();
            _renders = GetComponentsInChildren<SpriteRenderer>(true);
        }

        public void Init(VisibilityMap vis)
        {
            if (_vis != null) _vis.OnVisibilityChanged -= Apply;
            _vis = vis;
            if (_vis != null) _vis.OnVisibilityChanged += Apply;
            Apply();
        }

        void OnDestroy()
        {
            if (_vis != null) _vis.OnVisibilityChanged -= Apply;
        }

        private void Apply()
        {
            if (_vis == null || _actor == null || !_actor.HasGrid) return;
            GridPosition p = _actor.Pos;
            bool show = _vis.IsVisible(p.x, p.y); // hide unless currently visible

            for (int i = 0; i < _renders.Length; i++)
                _renders[i].enabled = show;

            // Hide any world-space UI under this actor (e.g., HealthBar)
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(show);
        }
    }
}