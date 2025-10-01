
using Unity.Mathematics;

public struct BulletState {
    public float2 position, velocity;
    public float timeRemaining;
    public bool Alive => timeRemaining > 0f;

    public bool exploded;


	public void Explode()
	{
		exploded = true;
		timeRemaining = 0f;
	}


}
