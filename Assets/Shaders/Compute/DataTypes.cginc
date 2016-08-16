//we layout positions, velocity, and properties separately to 
//improve coalesced reads. 

// BEFORE DEBUGGING!!!!!
//NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE
//Please make sure these are consistent with what is in the shader
#define PARTICLE_BLOCKSIZE 256

struct ParticleProperties {
	uint alive;
	float life;
	float4 color;
};

struct Source {
	float2 position;
	float2 velocity_1;
	float2 velocity_2;
	float life_start;
	uint spawn_period;
	int spawn_amount;
};

struct Attractor {
	float2 position;
	float magnitude;
};

struct Wall {
	float2 position;
	float2 norm;
};