//we layout positions, velocity, and properties separately to 
//improve coalesced reads. 

// BEFORE DEBUGGING!!!!!
//NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE
//Please make sure these are consistent with what is in the shader
#define PARTICLE_BLOCKSIZE 128

#define PARTICLE_STATE_DEAD 0
#define PARTICLE_STATE_ALIVE 1
#define PARTICLE_STATE_NLIST_VALID 2
#define PARTICLE_STATE_EXPLODING 3

#define PARTICLE_MODIFIER_SPAWN 0
#define PARTICLE_MODIFIER_INTEGRATOR 1
#define PARTICLE_MODIFIER_TARGET 2
#define PARTICLE_MODIFIER_PARTICLE 3

#define NLIST_INDEX(i,j) ((j) * NP + (i))
//#define NLIST_INDEX(i,j) ((i) * maxNeighbors + (j))

struct ParticleProperties {
	uint state;
	uint lastModifier;
	uint lastModifierIndex;
	float life;
	float4 color;
};

struct ParticleGInfo {
	uint group;
};

struct Source {
	float2 position;
	float2 velocity;
	float life_start;
	int spawn_period;
	uint group;
};

struct Attractor {
	float2 position;
	float magnitude;
};

struct Wall {
	float2 position;
	float2 norm;
};

struct Target {
	float2 position;
	float radius;
};