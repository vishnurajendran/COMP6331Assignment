using System;
using System.Collections;
using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private Vector3 _otherPositionDelta;
    [SerializeField] private float _positionChangeDelay;
    [SerializeField] private float _speed;

    private int _direction = -1;
    private Vector3 _pos1;
    private Vector3 _pos2;
    
    private void Start()
    {
        _pos1 = transform.position;
        _pos2 = transform.position + _otherPositionDelta;
        StartCoroutine(SawMotion());
    }

    public IEnumerator SawMotion()
    {
        int dir = _direction;
        float totalTime = Vector3.Distance(_pos1, _pos2)/_speed;
        while (true)
        {   
            var nextPos = dir > 0 ? _pos1:_pos2;
            var myPos = transform.position;
            float timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / totalTime;
                transform.position = Vector3.Lerp(myPos, nextPos, timeStep);
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(_positionChangeDelay);
            dir *= -1;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        var hero = collider.GetComponentInParent<HeroController>();
        if (hero)
        {
            Destroy(hero.gameObject);
        }
    }
}
