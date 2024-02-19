using System;
using System.Collections.Generic;
using Level;
using Unity.VisualScripting;
using UnityEngine;

namespace AgentControllers
{
    namespace AgentControllers
    {
        public class AgentUserController : AgentController
        {
            protected override BaseAgentParams Params => _params;

            [SerializeField] private BaseAgentParams _params;
            [SerializeField] private LayerMask _heroMask;
            
            private Vector3 _startPosition;
            private Camera _camera;
            
            protected override void Start()
            {
                base.Start();
                _startPosition = transform.position;
                _camera = Camera.main;
            }
            
            private void Update()
            {
                var horizontal = Input.GetAxis("Horizontal");
                var vertical = Input.GetAxis("Vertical");

                var forward = _camera.transform.forward;
                var right = _camera.transform.right;
                forward.y = 0;
                right.y = 0;
                var dir = forward * vertical + right * horizontal;
                _agent.Move(dir.normalized, _params.AgentSpeed, _params.LookSpeed, Time.deltaTime);
            }

            private void LateUpdate()
            {
                TryCaptureHero();
            }

            private void TryCaptureHero()
            {
                var caughtHeros = new HashSet<HeroController>();
                var cols = Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _heroMask);
                foreach (var col in cols)
                {
                    var hero = col.GetComponentInParent<HeroController>();
                    if (hero && !caughtHeros.Contains(hero))
                    {
                        caughtHeros.Add(hero);
                        OnheroCaught(hero);
                        break;
                    }
                }
            }

            private void OnheroCaught(HeroController hero)
            {
                if (!hero.Prisoner)
                {
                    hero.CaughtByPlayer(false);
                    LevelManager.Instance.SpawnGuardRandomly();
                }
                else
                {
                    var prisoner = hero.Prisoner;
                    hero.Prisoner.SetTarget(this.transform);
                    LevelManager.Instance.MarkPrisonerNotAvaialble(prisoner.transform);
                    hero.CaughtByPlayer(true);
                }
            }

            public void ResetPlayer()
            {
                _agent.Teleport(_startPosition);
            }
        }
    }
    
}