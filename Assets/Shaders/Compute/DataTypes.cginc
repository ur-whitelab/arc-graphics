//we layout positions, velocity, and properties separately to 
//improve coalesced reads. 


struct ParticleProperties {
	uint alive;
	float life;
};

struct Source {
	float2 position;
	float2 velocity_1;
	float2 velocity_2;
	float life_start;
	uint spawn_period;
	uint spawn_amount;
};

struct Attractor {
	float2 position;
	float magnitude;
};