namespace obj.mamagement {
    public abstract class ShapeBehavior {

        public abstract ShapeBehaviorType BehaviorType { get; }

        public abstract bool GameUpdate(Shape shape);
        public abstract void Save(GameDataWriter writer);
        public abstract void Load(GameDataReader reader);

        public virtual void ResolveShapeInstances(){
            
        }

        public abstract void Recycle();

    }
}
