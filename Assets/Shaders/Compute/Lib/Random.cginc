//http://www.reedbeta.com/blog/2013/01/12/quick-and-easy-gpu-random-numbers-in-d3d11/

uint rng_state;

uint rand_xorshift()
{
	// Xorshift algorithm from George Marsaglia's paper
	rng_state ^= (rng_state << 13);
	rng_state ^= (rng_state >> 17);
	rng_state ^= (rng_state << 5);
	return rng_state;
}

uint rand_lcg()
{
	// LCG values from Numerical Recipes
	//Use this if the other is too slow
	rng_state = 1664525 * rng_state + 1013904223;
	return rng_state;
}

uint wang_hash(uint seed)
{
	seed = (seed ^ 61) ^ (seed >> 16);
	seed *= 9;
	seed = seed ^ (seed >> 4);
	seed *= 0x27d4eb2d;
	seed = seed ^ (seed >> 15);
	return seed;
}

uint rand_float() {
	return  float(rand_xorshift()) * (1.0 / 4294967296.0);
}

/*
// Example of using Xorshift in a compute shader
[numthreads(256, 1, 1)]
void cs_main(uint3 threadId : SV_DispatchThreadID)
{
	// Seed the PRNG using the thread ID
	rng_state = threadId.x;

	// Generate a few numbers...
	uint r0 = rand_xorshift();
	uint r1 = rand_xorshift();
	// Do some stuff with them...

	// Generate a random float in [0, 1)...
	float f0 = float(rand_xorshift()) * (1.0 / 4294967296.0);

	// ...etc.
}
*/