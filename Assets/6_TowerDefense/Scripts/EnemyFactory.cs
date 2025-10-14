using System.Collections;
using System.Collections.Generic;
using tower.defense;
using UnityEngine;


[CreateAssetMenu(menuName = "tower/Create Enemy Factory", fileName = "EnemyFactory")]
public class EnemyFactory : GameObjectFactory {

    [SerializeField] Enemy enemyPrefab;
    
    [SerializeField, FloatRangeSlider(0.5f, 2f)] 
    FloatRange scale = new(1f);

    [SerializeField, FloatRangeSlider(0.2f, 5f)]
    FloatRange speed = new FloatRange(1f);

    [SerializeField, FloatRangeSlider(-0.4f, 0.4f)]
    FloatRange pathOffset = new FloatRange(0f);


    public Enemy Get() {
        Enemy instance = CreateGameObjectInstance(enemyPrefab);
        instance.Initialize(scale.RandomValueInRange,speed.RandomValueInRange,pathOffset.RandomValueInRange);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(Enemy enemy) {
        Debug.Assert(enemy.OriginFactory == this, "Wrong Factory Reclaimed!");
        Destroy(enemy.gameObject);
    }

}
