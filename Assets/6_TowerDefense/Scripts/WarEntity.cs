using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tower.defense {

    public abstract class WarEntity : GameBehavior {

        WarFactory originFactory;

        public WarFactory OriginFactory {
            get => originFactory;
            set {
                Debug.Assert(originFactory == null, "Redefined origin factory!", this);
                originFactory = value;
            }
        }

        public void Recycle(){
            OriginFactory.Reclaim(this);
        }

    }
}
