namespace obj.mamagement {
    public abstract class ShapeBehaviour {

        public abstract ShapeBehaviorType BehaviorType { get; }

        public abstract void GameUpdate(Shape shape);
        public abstract void Save(GameDataWriter writer);
        public abstract void Load(GameDataReader reader);

        public abstract void Recycle();

    }
}
